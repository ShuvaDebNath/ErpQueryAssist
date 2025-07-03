using System.Collections.Generic;

namespace ErpQueryAssist.Application.Services
{
    public static class ColumnNormalizer
    {
        private static readonly Dictionary<string, string> columnMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "created", "MakeDate" },
            { "createddate", "MakeDate" },
            { "created_date", "MakeDate" },
            { "date", "MakeDate" },
            { "makeby", "MakeBy" },
            { "createdby", "MakeBy" },
            { "creator", "MakeBy" },
            { "who", "MakeBy" },
            { "amount", "TotalAmount" },
            { "qty", "TotalPIQty" },
            { "closed PI", "Status" },
            // Add more mappings as needed
        };

        public static string Normalize(string inputColumn)
        {
            if (columnMap.TryGetValue(inputColumn, out var normalized))
            {
                return normalized;
            }

            // Default: return as-is if no mapping exists
            return inputColumn;
        }
    }
}
