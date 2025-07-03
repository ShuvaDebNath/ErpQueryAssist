using ErpQueryAssist.Application.DTOs;
using ErpQueryAssist.Application.Interfaces;
using ErpQueryAssist.Domain.Entities;
using ErpQueryAssist.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ErpQueryAssist.Infrastructure.Services;

public class UserQueryService : IUserQueryService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public UserQueryService(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                         ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new Exception("Default connection string is missing.");
    }

    public async Task<int> SaveUserQueryAsync(string queryText)
    {
        var query = new UserQuery { QueryText = queryText };
        _db.UserQueries.Add(query);
        await _db.SaveChangesAsync();
        return query.Id;
    }

    public async Task UpdateSqlGeneratedAsync(int userQueryId, string sql)
    {
        var query = await _db.UserQueries.FindAsync(userQueryId);
        if (query != null)
        {
            query.SqlGenerated = sql;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> SaveQueryResultAsync(int userQueryId, UserQueryResultDto resultDto)
    {
        var result = new UserQueryResult
        {
            UserQueryId = userQueryId,
            JsonData = resultDto.JsonData,
            ChartType = resultDto.ChartType,
            SummaryText = resultDto.SummaryText
        };

        _db.UserQueryResults.Add(result);
        await _db.SaveChangesAsync();
        return result.Id;
    }

    public async Task<UserQueryDto?> GetQueryWithResultsAsync(int userQueryId)
    {
        var query = await _db.UserQueries
            .Include(q => q.Results)
            .FirstOrDefaultAsync(q => q.Id == userQueryId);

        if (query == null) return null;

        return new UserQueryDto
        {
            Id = query.Id,
            QueryText = query.QueryText,
            SqlGenerated = query.SqlGenerated,
            CreatedAt = query.CreatedAt,
            Results = query.Results.Select(r => new UserQueryResultDto
            {
                JsonData = r.JsonData,
                ChartType = r.ChartType,
                SummaryText = r.SummaryText
            }).ToList()
        };
    }


    public async Task<string> ExecuteSqlAsync(string sql)
    {
        CleanSqlOutput(sql);

        Debug.WriteLine(sql);
        sql = sql.Trim();

        if (!Regex.IsMatch(sql, @"^\s*SELECT", RegexOptions.IgnoreCase))
        {
            Debug.WriteLine("[Blocked SQL]: " + sql.Substring(0, Math.Min(50, sql.Length)));
            throw new InvalidOperationException("Only SELECT statements are allowed.");
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            var dataTable = new DataTable();

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(sql, conn);
            using var adapter = new SqlDataAdapter(cmd);

            await conn.OpenAsync();
            adapter.Fill(dataTable);

            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                    dict[col.ColumnName] = row[col];
                rows.Add(dict);
            }

            return JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("SQL Execution failed. Please try again later or contact admin.", ex);

            //var error = new Dictionary<string, object>
            //{
            //    ["error"] = ex.Message,
            //    ["query"] = sql
            //};
            //return JsonSerializer.Serialize(new[] { error });
        }
    }


    private string CleanSqlOutput(string sql)
    {
        sql = sql.Trim();

        // Remove common Markdown wrappers
        sql = sql.Replace("```sql", "", StringComparison.OrdinalIgnoreCase)
                 .Replace("```", "")
                 .Replace("'''sql", "", StringComparison.OrdinalIgnoreCase)
                 .Replace("'''", "");

        // Strip everything before actual SELECT if needed
        var selectIndex = sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
        if (selectIndex > 0)
        {
            sql = sql.Substring(selectIndex);
        }

        return sql.Trim();
    }

}
