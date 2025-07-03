namespace ErpQueryAssist.Web.ViewModels;

public class YearPivotResultViewModel
{
    public string OriginalQuestion { get; set; }
    public List<YearPivotRow> PivotRows { get; set; } = [];
    public List<string> YearHeaders { get; set; } = []; 
    public string Title { get; set; } = "Year wise Order PIVOT Report";
}

public class YearPivotRow
{
    public string Metric { get; set; } 
    public Dictionary<string, decimal> YearValues { get; set; } = []; 
}
