namespace RotoMonsterTwitter.Contracts.Requests;

public class AddTweetRequest
{
    public string Text { get; set; } = "";
    public int SportId { get; set; }
    public string? ScreenUsername { get; set; }
    public string? DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool? IsNews { get; set; }
    public bool? IsTop { get; set; }
}
