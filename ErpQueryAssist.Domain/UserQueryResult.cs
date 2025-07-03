namespace ErpQueryAssist.Domain.Entities;

public class UserQueryResult
{
    public int Id { get; set; }
    public int UserQueryId { get; set; }

    public string? JsonData { get; set; }
    public string? ChartType { get; set; }
    public string? SummaryText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public UserQuery UserQuery { get; set; } = null!;
}
