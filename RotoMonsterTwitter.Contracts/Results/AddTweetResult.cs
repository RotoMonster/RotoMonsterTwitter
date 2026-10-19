namespace RotoMonsterTwitter.Contracts.Results;

public class AddTweetResult : BaseResult
{
    public string TweetId { get; set; } = "";
    public TweetResult? Tweet { get; set; }
}
