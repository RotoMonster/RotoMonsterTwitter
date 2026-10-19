namespace RotoMonsterTwitter.Model.Configuration;

public class TwitterApiOptions
{
    public const string SectionName = "TwitterApi";

    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.twitterapi.io";
    public string UserAgent { get; set; } = "RotoMonsterTwitter/1.0";
    public int MaxPagesPerFetch { get; set; } = 20;

    public string Source { get; set; } = "twitterapi.io";

    public string XBearerToken { get; set; } = "";

    public string XBaseUrl { get; set; } = "https://api.x.com/2";

    public int XMaxResults { get; set; } = 100;

    public int XMaxPagesPerFetch { get; set; } = 1;
}
