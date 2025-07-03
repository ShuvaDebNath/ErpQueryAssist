using ErpQueryAssist.Application.DTOs;

namespace ErpQueryAssist.Application.Interfaces;

public interface IUserQueryService
{
    Task<int> SaveUserQueryAsync(string queryText);
    Task UpdateSqlGeneratedAsync(int userQueryId, string sql);
    Task<int> SaveQueryResultAsync(int userQueryId, UserQueryResultDto resultDto);
    Task<UserQueryDto?> GetQueryWithResultsAsync(int userQueryId);

    Task<string> ExecuteSqlAsync(string sql);
}
