using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpQueryAssist.Application.DTOs;

public class FilterDto
{
    public string Column { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty; // =, >=, BETWEEN, etc.
    public string Value { get; set; } = string.Empty;
}
