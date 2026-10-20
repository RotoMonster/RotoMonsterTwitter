namespace RotoMonsterTwitter.Model.Configuration;

public class TwitterPostOptions
{
    public const string SectionName = "TwitterPost";

    public List<TwitterPostAccount> Accounts { get; set; } = new();

    public string ConsumerKey { get; set; } = "";
    public string ConsumerSecret { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string AccessTokenSecret { get; set; } = "";

    public string PostUrl { get; set; } = "https://api.twitter.com/2/tweets";

    public List<TwitterPostAccount> AllAccounts()
    {
        var list = Accounts.Where(a => a.IsConfigured).ToList();
        var legacy = new TwitterPostAccount
        {
            Name = "default",
            ConsumerKey = ConsumerKey,
            ConsumerSecret = ConsumerSecret,
            AccessToken = AccessToken,
            AccessTokenSecret = AccessTokenSecret
        };
        if (list.Count == 0 && legacy.IsConfigured)
        {
            list.Add(legacy);
        }
        return list;
    }

    public bool IsConfigured => AllAccounts().Count > 0;
}

public class TwitterPostAccount
{
    public string Name { get; set; } = "";
    public string ConsumerKey { get; set; } = "";
    public string ConsumerSecret { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string AccessTokenSecret { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ConsumerKey) &&
        !string.IsNullOrWhiteSpace(ConsumerSecret) &&
        !string.IsNullOrWhiteSpace(AccessToken) &&
        !string.IsNullOrWhiteSpace(AccessTokenSecret);
}
