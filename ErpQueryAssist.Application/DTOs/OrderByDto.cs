using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpQueryAssist.Application.DTOs;

public class OrderByDto
{
    public string Column { get; set; } = string.Empty;
    public string Direction { get; set; } = "DESC"; // or ASC
}
