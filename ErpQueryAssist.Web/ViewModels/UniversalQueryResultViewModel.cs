namespace ErpQueryAssist.Web.ViewModels;

public class UniversalQueryResultViewModel
{
    public string OriginalQuestion { get; set; }
    public string SummaryJson { get; set; }
    public string DetailsJson { get; set; }
    public string ChartJson { get; set; }
    public string SummaryTitle { get; set; } = "Summary";
    public string DetailsTitle { get; set; } = "Details";
    public string ChartTitle { get; set; } = "Chart";

    public List<SummaryData> SummaryDataList { get; set; } = new();
    public List<DetailsData> DetailsDataList { get; set; }
}
