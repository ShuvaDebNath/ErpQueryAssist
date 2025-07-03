namespace ErpQueryAssist.Application.DTOs;

public class UserQueryDto
{
    public int Id { get; set; }
    public string QueryText { get; set; } = null!;
    public string? SqlGenerated { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<UserQueryResultDto> Results { get; set; } = new();
}
