using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RotoMonsterTwitter.Model.Configuration;
using RotoMonsterTwitter.Contracts.Results;

namespace RotoMonsterTwitter.Model.Services;

public class TwitterPosterService : ITwitterPosterService
{
    private static readonly DateTime UnixEpoch =
        new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly HttpClient _http;
    private readonly TwitterPostOptions _options;

    public TwitterPosterService(HttpClient http, IOptions<TwitterPostOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public Task<PostTweetResult> PostTweetAsync(string text, CancellationToken ct = default)
        => PostTweetAsync(text, null, ct);

    public async Task<PostTweetResult> PostTweetAsync(
        string text, string? account, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Fail("Tweet text is required.");
        }

        var accounts = _options.AllAccounts();
        if (accounts.Count == 0)
        {
            return Fail("X API credentials are not configured. Add accounts under TwitterPost:Accounts "
                + "(Name, ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret).");
        }

        List<TwitterPostAccount> toTry;
        if (string.IsNullOrWhiteSpace(account))
        {
            toTry = accounts;
        }
        else
        {
            var match = accounts.FirstOrDefault(a =>
                string.Equals(a.Name, account.Trim().TrimStart('@'), StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                return Fail($"No posting account named '{account}'. Configured: "
                    + string.Join(", ", accounts.Select(a => a.Name)));
            }
            toTry = new List<TwitterPostAccount> { match };
        }

        PostTweetResult? last = null;
        var tried = new List<string>();

        foreach (var acct in toTry)
        {
            tried.Add(acct.Name);
            last = await PostAsAsync(acct, text, ct);
            last.AccountsTried = new List<string>(tried);

            if (last.Success || !last.LimitReached)
            {
                return last;
            }
        }

        return last!;
    }

    private async Task<PostTweetResult> PostAsAsync(
        TwitterPostAccount acct, string text, CancellationToken ct)
    {
        var result = new PostTweetResult { AccountUsed = acct.Name };

        try
        {
            var request = BuildRequest(acct, text);
            var response = await _http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            ReadLimits(response, result);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                result.Success = false;
                result.LimitReached = true;
                result.ErrorMessage = $"{acct.Name} hit its posting limit: {Trim(body)}";
                return result;
            }

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                result.ErrorMessage = $"X API returned {(int)response.StatusCode} for {acct.Name}: {Trim(body)}";
                return result;
            }

            var data = JObject.Parse(body)["data"];
            result.TweetId = data?["id"]?.ToString();
            result.Text = data?["text"]?.ToString() ?? text;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private static void ReadLimits(HttpResponseMessage response, PostTweetResult result)
    {
        result.Limit24h = HeaderInt(response, "x-user-limit-24hour-limit")
            ?? HeaderInt(response, "x-app-limit-24hour-limit");
        result.Remaining24h = HeaderInt(response, "x-user-limit-24hour-remaining")
            ?? HeaderInt(response, "x-app-limit-24hour-remaining");
        var reset = HeaderInt(response, "x-user-limit-24hour-reset")
            ?? HeaderInt(response, "x-app-limit-24hour-reset");
        if (reset.HasValue)
        {
            result.ResetsAt = UnixEpoch.AddSeconds(reset.Value);
        }
    }

    private static int? HeaderInt(HttpResponseMessage response, string name)
    {
        if (response.Headers.TryGetValues(name, out var values)
            && int.TryParse(values.FirstOrDefault(), out var n))
        {
            return n;
        }
        return null;
    }

    private HttpRequestMessage BuildRequest(TwitterPostAccount acct, string text)
    {
        var oauth = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["oauth_consumer_key"] = acct.ConsumerKey,
            ["oauth_nonce"] = Guid.NewGuid().ToString("N"),
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = ((long)(DateTime.UtcNow - UnixEpoch).TotalSeconds).ToString(),
            ["oauth_token"] = acct.AccessToken,
            ["oauth_version"] = "1.0"
        };

        var parameterString = string.Join("&",
            oauth.Select(kv => $"{Escape(kv.Key)}={Escape(kv.Value)}"));

        var signatureBase = string.Concat(
            "POST&", Escape(_options.PostUrl), "&", Escape(parameterString));

        var signingKey = string.Concat(
            Escape(acct.ConsumerSecret), "&", Escape(acct.AccessTokenSecret));

        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBase));

        oauth["oauth_signature"] = Convert.ToBase64String(hash);

        var header = string.Join(", ",
            oauth.Select(kv => $"{Escape(kv.Key)}=\"{Escape(kv.Value)}\""));

        var payload = new JObject { ["text"] = text }.ToString(Newtonsoft.Json.Formatting.None);

        var request = new HttpRequestMessage(HttpMethod.Post, _options.PostUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", header);

        return request;
    }

    private static string Escape(string value)
    {
        const string unreserved =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";

        var builder = new StringBuilder(value.Length * 2);

        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            var c = (char)b;
            if (unreserved.IndexOf(c) >= 0)
            {
                builder.Append(c);
            }
            else
            {
                builder.Append('%').Append(b.ToString("X2"));
            }
        }

        return builder.ToString();
    }

    private static PostTweetResult Fail(string message)
        => new() { Success = false, ErrorMessage = message };

    private static string Trim(string value)
        => value.Length > 400 ? value[..400] + "..." : value;
}
