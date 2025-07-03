namespace ErpQueryAssist.Web.ViewModels;

public class UserQueryResultViewModel
{
    public string QueryText { get; set; } = string.Empty;
    public string? SqlGenerated { get; set; }
    public string? Summary { get; set; }
    public string? JsonData { get; set; }
    public string? ChartType { get; set; }
}
