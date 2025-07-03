namespace ErpQueryAssist.Web.ViewModels;

public class MonthPivotResultViewModel
{
    public string OriginalQuestion { get; set; }
    public List<MonthPivotRow> PivotRows { get; set; } = [];
    public List<string> MonthHeaders { get; set; } = []; 
    public string Title { get; set; } = "Month wise Order PIVOT Report";
}

public class MonthPivotRow
{
    public string Metric { get; set; }
    public Dictionary<string, decimal> MonthValues { get; set; } = []; 
}
