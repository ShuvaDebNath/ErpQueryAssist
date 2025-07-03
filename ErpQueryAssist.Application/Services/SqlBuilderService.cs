using ErpQueryAssist.Application.DTOs;
using System.Text;

namespace ErpQueryAssist.Application.Services
{
    public static class SqlBuilderService
    {
        public static string BuildSql(QueryIntentDto intent)
        {
            var sb = new StringBuilder();

            // Determine SELECT clause
            if (intent.Action == "summary")
            {
                var metrics = intent.Metrics != null && intent.Metrics.Any()
                    ? string.Join(", ", intent.Metrics)
                    : "COUNT(*)";

                sb.Append($"SELECT {metrics} ");
            }
            else
            {
                sb.Append("SELECT pio.PINumber, pio.MakeDate, pio.MakeBy, pom.TotalPIQty, pom.TotalAmount ");
            }

            sb.Append("FROM tblPIOderQty_infomation pio ");
            sb.Append("JOIN tblPIOrder_Qty_Master_Info pom ON pio.PI_Id = pom.PI_Id ");
            sb.Append("JOIN tblClientInformation ci ON pio.ClinetId = ci.ClinetId ");

            // WHERE clause
            if (intent.Filters != null && intent.Filters.Any())
            {
                var whereConditions = intent.Filters.Select(f =>
                {
                    if (f.Operator.Equals("MONTH", StringComparison.OrdinalIgnoreCase))
                        return $"MONTH(pio.{f.Column}) = MONTH(GETDATE()) AND YEAR(pio.{f.Column}) = YEAR(GETDATE())";
                    if (f.Operator.Equals("YEAR", StringComparison.OrdinalIgnoreCase))
                        return $"YEAR(pio.{f.Column}) = YEAR(GETDATE()) - 1";
                    if (f.Operator.Equals("=", StringComparison.OrdinalIgnoreCase))
                        return $"pio.{f.Column} = '{f.Value}'";
                    if (f.Operator.Equals(">=", StringComparison.OrdinalIgnoreCase))
                        return $"pio.{f.Column} >= '{f.Value}'";
                    return $"pio.{f.Column} {f.Operator} '{f.Value}'";
                });

                sb.Append("WHERE " + string.Join(" AND ", whereConditions) + " ");
            }

            // GROUP BY
            if (!string.IsNullOrEmpty(intent.GroupBy))
            {
                sb.Append($"GROUP BY pio.{intent.GroupBy} ");
            }

            // ORDER BY
            if (intent.OrderBy != null)
            {
                string orderColumn = intent.OrderBy.Column;

                // If GroupBy is used and orderColumn is not in GroupBy or aggregate, default to aggregate
                if (!string.IsNullOrEmpty(intent.GroupBy) && intent.GroupBy != orderColumn)
                {
                    if (orderColumn.Equals("MakeDate", StringComparison.OrdinalIgnoreCase))
                        orderColumn = "MAX(pio.MakeDate)";
                    else if (orderColumn.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase))
                        orderColumn = "SUM(pom.TotalAmount)";
                    // Add more as needed
                }

                sb.Append($"ORDER BY {orderColumn} {intent.OrderBy.Direction} ");
            }

            // LIMIT (pagination)
            if (intent.Limit.HasValue && intent.Limit.Value > 0)
            {
                sb.Append($"OFFSET 0 ROWS FETCH NEXT {intent.Limit.Value} ROWS ONLY");
            }

            return sb.ToString().Trim();
        }
    }
}
