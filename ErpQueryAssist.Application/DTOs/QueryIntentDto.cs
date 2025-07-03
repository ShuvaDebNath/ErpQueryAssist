using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpQueryAssist.Application.DTOs;

public class QueryIntentDto
{
    public string Action { get; set; } // summary, details, chart
    public string Table { get; set; } = "Order";
    public List<FilterDto> Filters { get; set; } = new();
    public OrderByDto? OrderBy { get; set; }
    public List<string>? Metrics { get; set; } // like COUNT(*), SUM(...)
    public string? GroupBy { get; set; }
    public int? Limit { get; set; }
}
