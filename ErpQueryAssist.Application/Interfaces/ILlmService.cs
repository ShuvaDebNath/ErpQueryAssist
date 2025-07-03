namespace ErpQueryAssist.Application.Interfaces;

public interface ILlmService
{
    Task<string> AskAsync(string prompt);
}
