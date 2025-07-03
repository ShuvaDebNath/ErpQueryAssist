namespace ErpQueryAssist.Domain.Entities;

public class UserQuery
{
    public int Id { get; set; }
    public string QueryText { get; set; } = null!;
    public string? SqlGenerated { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<UserQueryResult> Results { get; set; } = new List<UserQueryResult>();
}
