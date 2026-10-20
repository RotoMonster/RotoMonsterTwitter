using System;
using System.Collections.Generic;

namespace RotoMonsterTwitter.Contracts.Results;

public class PostTweetResult : BaseResult
{
    public string? TweetId { get; set; }
    public string? Text { get; set; }
    public string? AccountUsed { get; set; }
    public int? Limit24h { get; set; }
    public int? Remaining24h { get; set; }
    public DateTime? ResetsAt { get; set; }
    public bool LimitReached { get; set; }
    public List<string> AccountsTried { get; set; } = new List<string>();
}
