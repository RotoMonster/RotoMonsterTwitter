using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RotoMonsterTwitter.Model.Configuration;
using RotoMonsterTwitter.Model.Data;
using RotoMonsterTwitter.Model.Entities;
using RotoMonsterTwitter.Contracts.Requests;
using RotoMonsterTwitter.Contracts.Results;

namespace RotoMonsterTwitter.Model.Services;

public class TweetIngestService : ITweetIngestService
{
    private readonly TwitterDbContext _db;
    private readonly ITweetProcessingService _processing;
    private readonly ITwitterApiService _api;
    private readonly XApiService _x;
    private readonly TwitterApiOptions _options;

    public TweetIngestService(TwitterDbContext db, ITwitterApiService api,
        IOptions<TwitterApiOptions> options,
        ITweetProcessingService processing,
        XApiService x)
    {
        _db = db;
        _api = api;
        _x = x;
        _options = options.Value;
        _processing = processing;
    }

    public Task<IngestResult> IngestListAsync(long listId, CancellationToken ct = default)
        => IngestListAsync(listId, null, ct);

    public async Task<IngestResult> IngestListAsync(long listId, string? source, CancellationToken ct = default)
    {
        var (list, result) = await LoadListAsync(listId, ct);
        if (list == null) return result;

        var collected = new List<ParsedTweet>();
        string? cursor = null;

        var useX = IsX(source ?? _options.Source);
        ITwitterApiService api = useX ? _x : _api;
        result.Source = useX ? "x" : "twitterapi.io";
        var maxPages = useX ? _options.XMaxPagesPerFetch : _options.MaxPagesPerFetch;

        for (var page = 0; page < Math.Max(1, maxPages); page++)
        {
            var fetched = await api.GetListTweetsPageAsync(listId, list.LastFetchedUnix, cursor, ct);
            result.PagesFetched++;
            collected.AddRange(fetched.Tweets);

            if (!fetched.HasNextPage || string.IsNullOrWhiteSpace(fetched.NextCursor))
            {
                break;
            }

            cursor = fetched.NextCursor;
        }

        return await PersistAsync(list, collected, result, ct);
    }

    private static bool IsX(string? source)
        => !string.IsNullOrWhiteSpace(source) &&
           (source.Equals("x", StringComparison.OrdinalIgnoreCase) ||
            source.Equals("twitter", StringComparison.OrdinalIgnoreCase) ||
            source.Equals("xapi", StringComparison.OrdinalIgnoreCase));

    public async Task<IngestResult> IngestJsonAsync(long listId, string json,
        CancellationToken ct = default)
    {
        var (list, result) = await LoadListAsync(listId, ct);
        if (list == null) return result;

        TweetPage page;
        try
        {
            page = TwitterApiService.ParsePage(json);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Could not parse the supplied JSON: {ex.Message}";
            return result;
        }

        result.PagesFetched = 1;
        return await PersistAsync(list, page.Tweets, result, ct);
    }

    public async Task<AddTweetResult> AddTweetAsync(AddTweetRequest request, CancellationToken ct = default)
    {
        var result = new AddTweetResult();

        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            result.Success = false;
            result.ErrorMessage = "Text is required.";
            return result;
        }

        if (request.SportId <= 0)
        {
            result.Success = false;
            result.ErrorMessage = "SportId is required.";
            return result;
        }

        var now = DateTime.UtcNow;
        var screen = string.IsNullOrWhiteSpace(request.ScreenUsername)
            ? "RotoMonster"
            : request.ScreenUsername.Trim().TrimStart('@');
        var userId = "manual-" + screen.ToLowerInvariant();

        var user = await _db.TweetUsers.FirstOrDefaultAsync(u => u.TwitterUserId == userId, ct);
        if (user == null)
        {
            user = new TweetUser
            {
                TwitterUserId = userId,
                ScreenUsername = screen,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? screen : request.DisplayName.Trim(),
                ImageUrl = request.ImageUrl?.Trim() ?? "",
                IsNews = request.IsNews ?? true,
                IsTop = request.IsTop ?? true,
                LastSeenAt = now
            };
            _db.TweetUsers.Add(user);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.DisplayName)) user.DisplayName = request.DisplayName.Trim();
            if (!string.IsNullOrWhiteSpace(request.ImageUrl)) user.ImageUrl = request.ImageUrl.Trim();
            if (request.IsNews.HasValue) user.IsNews = request.IsNews.Value;
            if (request.IsTop.HasValue) user.IsTop = request.IsTop.Value;
            user.LastSeenAt = now;
        }

        var created = request.CreatedDate ?? now;
        created = created.Kind switch
        {
            DateTimeKind.Utc => created,
            DateTimeKind.Local => created.ToUniversalTime(),
            _ => DateTime.SpecifyKind(created, DateTimeKind.Utc)
        };

        var tweet = new Tweet
        {
            TweetId = "manual-" + Guid.NewGuid().ToString("N"),
            TwitterUserId = userId,
            SportId = request.SportId,
            Text = request.Text.Trim(),
            CreatedDate = created,
            DateAdded = now
        };

        _db.Tweets.Add(tweet);
        await _db.SaveChangesAsync(ct);

        try
        {
            await _processing.ProcessAsync(batchSize: 500, reprocessAll: false, ct: ct);
        }
        catch
        {
        }

        _db.ChangeTracker.Clear();
        result.TweetId = tweet.TweetId;
        return result;
    }

    private async Task<(TwitterList? list, IngestResult result)> LoadListAsync(
        long listId, CancellationToken ct)
    {
        var list = await _db.TwitterLists.FirstOrDefaultAsync(l => l.ListId == listId, ct);

        if (list == null)
        {
            return (null, new IngestResult
            {
                Success = false,
                ListId = listId,
                ErrorMessage = $"No list configured with id {listId}. Add a row to TwitterLists first."
            });
        }

        return (list, new IngestResult
        {
            ListId = listId,
            SportId = list.SportId,
            PreviousSinceUnix = list.LastFetchedUnix,
            NewSinceUnix = list.LastFetchedUnix
        });
    }

    private async Task<IngestResult> PersistAsync(TwitterList list,
        List<ParsedTweet> collected, IngestResult result, CancellationToken ct)
    {
        result.TweetsReturned = collected.Count;

        if (collected.Count == 0)
        {
            list.LastFetchedAt = DateTime.UtcNow;
            list.LastTweetCount = 0;
            await _db.SaveChangesAsync(ct);
            return result;
        }

        // Users first, so the FK on Tweets resolves.
        var incomingUsers = collected
            .GroupBy(p => p.User.TwitterUserId)
            .Select(g => g.Last().User)
            .ToList();

        var incomingUserIds = incomingUsers.Select(u => u.TwitterUserId).ToList();

        var existingUsers = await _db.TweetUsers
            .Where(u => incomingUserIds.Contains(u.TwitterUserId))
            .ToDictionaryAsync(u => u.TwitterUserId, ct);

        foreach (var incoming in incomingUsers)
        {
            if (existingUsers.TryGetValue(incoming.TwitterUserId, out var existing))
            {
                existing.ScreenUsername = incoming.ScreenUsername;
                existing.DisplayName = incoming.DisplayName;
                existing.ImageUrl = incoming.ImageUrl;
                existing.IsVerified = incoming.IsVerified;
                existing.IsBlueVerified = incoming.IsBlueVerified;
                existing.VerifiedType = incoming.VerifiedType;
                existing.LastSeenAt = DateTime.UtcNow;
            }
            else
            {
                _db.TweetUsers.Add(incoming);
                result.NewUsers++;
            }
        }

        await _db.SaveChangesAsync(ct);

        // Then tweets, skipping any already stored.
        var incomingIds = collected.Select(p => p.Tweet.TweetId).Distinct().ToList();

        var existingIds = await _db.Tweets
            .Where(t => incomingIds.Contains(t.TweetId))
            .Select(t => t.TweetId)
            .ToListAsync(ct);

        var existingSet = existingIds.ToHashSet();
        var seen = new HashSet<string>();

        foreach (var parsed in collected)
        {
            if (existingSet.Contains(parsed.Tweet.TweetId)) continue;
            if (!seen.Add(parsed.Tweet.TweetId)) continue;

            parsed.Tweet.SportId = list.SportId;
            parsed.Tweet.Media = parsed.Media;

            _db.Tweets.Add(parsed.Tweet);
            result.NewTweets++;
        }

        // Advance the cursor to the newest tweet seen.
        var newest = collected.Max(p => p.Tweet.CreatedDate);
        var newestUnix = TwitterApiService.ToUnix(DateTime.SpecifyKind(newest, DateTimeKind.Utc));

        if (newestUnix > list.LastFetchedUnix)
        {
            list.LastFetchedUnix = newestUnix;
        }

        list.LastFetchedAt = DateTime.UtcNow;
        list.LastTweetCount = result.TweetsReturned;
        result.NewSinceUnix = list.LastFetchedUnix;

        await _db.SaveChangesAsync(ct);

        // A pull should leave tweets ready to read, so process the new ones now.
        if (result.Success && result.NewTweets > 0)
        {
            try
            {
                var processed = await _processing.ProcessAsync(
                    batchSize: Math.Max(result.NewTweets, 100),
                    reprocessAll: false, ct: ct);

                result.TweetsProcessed = processed.TweetsProcessed;
                result.MatchesCreated = processed.KeywordMatches
                    + processed.PlayerMatches + processed.TeamMatches;
            }
            catch
            {
                // Ingest succeeded; don't fail the whole call if the follow-on
                // processing hiccups. The tweets are stored and the next run
                // will process them.
            }
        }

        return result;
    }
}
