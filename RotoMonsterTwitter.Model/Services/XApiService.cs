using System.Globalization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RotoMonsterTwitter.Model.Configuration;
using RotoMonsterTwitter.Model.Entities;

namespace RotoMonsterTwitter.Model.Services;

public class XApiService : ITwitterApiService
{
    private const string TweetFields = "created_at,author_id,public_metrics,attachments,note_tweet,referenced_tweets";
    private const string UserFields = "name,username,profile_image_url,verified,verified_type,public_metrics";
    private const string MediaFields = "type,url,preview_image_url,variants,duration_ms";

    private readonly HttpClient _http;
    private readonly TwitterApiOptions _options;

    public XApiService(HttpClient http, IOptions<TwitterApiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<string> GetListTweetsRawAsync(
        long listId, long sinceUnixTimestamp = 0, string? cursor = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.XBearerToken))
        {
            throw new InvalidOperationException("TwitterApi:XBearerToken is not set.");
        }

        var max = Math.Clamp(_options.XMaxResults, 5, 100);
        var url = $"{_options.XBaseUrl.TrimEnd('/')}/lists/{listId}/tweets?max_results={max}"
            + "&expansions=author_id,attachments.media_keys"
            + $"&tweet.fields={TweetFields}&user.fields={UserFields}&media.fields={MediaFields}";

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            url += $"&pagination_token={Uri.EscapeDataString(cursor)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.XBearerToken);

        var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"X API returned {(int)response.StatusCode}: {Trim(body)}");
        }

        return body;
    }

    public async Task<TweetPage> GetListTweetsPageAsync(
        long listId, long sinceUnixTimestamp = 0, string? cursor = null,
        CancellationToken ct = default)
    {
        var body = await GetListTweetsRawAsync(listId, sinceUnixTimestamp, cursor, ct);
        return ParsePage(body, sinceUnixTimestamp);
    }

    public static TweetPage ParsePage(string json, long sinceUnixTimestamp = 0)
    {
        var page = new TweetPage();
        var root = JObject.Parse(json);

        var users = new Dictionary<string, JToken>();
        foreach (var u in root["includes"]?["users"]?.Children() ?? Enumerable.Empty<JToken>())
        {
            var id = u["id"]?.ToString();
            if (!string.IsNullOrEmpty(id)) users[id] = u;
        }

        var media = new Dictionary<string, JToken>();
        foreach (var m in root["includes"]?["media"]?.Children() ?? Enumerable.Empty<JToken>())
        {
            var key = m["media_key"]?.ToString();
            if (!string.IsNullOrEmpty(key)) media[key] = m;
        }

        var reachedSince = false;
        foreach (var token in root["data"]?.Children() ?? Enumerable.Empty<JToken>())
        {
            var parsed = ParseOne(token, users, media);
            if (parsed == null) continue;

            if (sinceUnixTimestamp > 0 && TwitterApiService.ToUnix(parsed.Tweet.CreatedDate) <= sinceUnixTimestamp)
            {
                reachedSince = true;
                continue;
            }

            page.Tweets.Add(parsed);
        }

        page.NextCursor = root["meta"]?["next_token"]?.ToString();
        page.HasNextPage = !reachedSince && !string.IsNullOrWhiteSpace(page.NextCursor);

        return page;
    }

    private static ParsedTweet? ParseOne(JToken token, Dictionary<string, JToken> users, Dictionary<string, JToken> media)
    {
        var tweetId = token["id"]?.ToString();
        var authorId = token["author_id"]?.ToString();

        if (string.IsNullOrEmpty(tweetId) || string.IsNullOrEmpty(authorId))
        {
            return null;
        }

        users.TryGetValue(authorId, out var author);
        var verifiedType = author?["verified_type"]?.ToString();

        var user = new TweetUser
        {
            TwitterUserId = authorId,
            ScreenUsername = author?["username"]?.ToString() ?? "",
            DisplayName = author?["name"]?.ToString() ?? "",
            ImageUrl = author?["profile_image_url"]?.ToString() ?? "",
            IsVerified = author?["verified"]?.Value<bool?>() ?? false,
            IsBlueVerified = string.Equals(verifiedType, "blue", StringComparison.OrdinalIgnoreCase),
            VerifiedType = string.IsNullOrEmpty(verifiedType) || verifiedType == "none" ? null : verifiedType,
            LastSeenAt = DateTime.UtcNow
        };

        var text = token["note_tweet"]?["text"]?.ToString();
        if (string.IsNullOrEmpty(text)) text = token["text"]?.ToString() ?? "";

        var tweet = new Tweet
        {
            TweetId = tweetId,
            TwitterUserId = authorId,
            Text = text,
            CreatedDate = ParseDate(token["created_at"]?.ToString()),
            DateAdded = DateTime.UtcNow,
            RetweetCount = token["public_metrics"]?["retweet_count"]?.Value<int?>(),
            Followers = author?["public_metrics"]?["followers_count"]?.Value<int?>()
        };

        var list = new List<TweetMedia>();
        short order = 0;
        foreach (var keyToken in token["attachments"]?["media_keys"]?.Children() ?? Enumerable.Empty<JToken>())
        {
            if (!media.TryGetValue(keyToken.ToString(), out var m)) continue;

            var type = m["type"]?.ToString() ?? "photo";
            var image = m["url"]?.ToString();
            if (string.IsNullOrEmpty(image)) image = m["preview_image_url"]?.ToString();
            if (string.IsNullOrEmpty(image)) continue;

            list.Add(new TweetMedia
            {
                TweetId = tweetId,
                DisplayOrder = order++,
                ImageUrl = image,
                MediaType = type,
                VideoUrl = PickBestVideoUrl(m["variants"]),
                DurationMillis = m["duration_ms"]?.Value<int?>()
            });
        }

        return new ParsedTweet { Tweet = tweet, User = user, Media = list };
    }

    private static string? PickBestVideoUrl(JToken? variants)
    {
        if (variants == null) return null;

        string? best = null;
        var bestBitrate = -1;

        foreach (var v in variants)
        {
            if (!string.Equals(v["content_type"]?.ToString(), "video/mp4", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var bitrate = v["bit_rate"]?.Value<int?>() ?? 0;
            if (bitrate > bestBitrate)
            {
                bestBitrate = bitrate;
                best = v["url"]?.ToString();
            }
        }

        return best;
    }

    private static DateTime ParseDate(string? value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private static string Trim(string value) => value.Length > 400 ? value[..400] + "..." : value;
}
