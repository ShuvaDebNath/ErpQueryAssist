
namespace ErpQueryAssist.Application.Models.Pivot;

public class ClientMonthPivotData
{
    public string ClientName { get; set; }
    public string MonthLabel { get; set; }
    public int TotalPIs { get; set; }
    public decimal TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
}
