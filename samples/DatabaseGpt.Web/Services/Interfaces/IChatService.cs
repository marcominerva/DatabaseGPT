using DatabaseGpt.Web.Models;
using OperationResults;

namespace DatabaseGpt.Web.Services.Interfaces;

public interface IChatService
{
    Task<Result<ChatResponse>> AskQueryAsync(ChatRequest request);

    IAsyncEnumerable<string> AskStreamAsync(ChatRequest request);
}