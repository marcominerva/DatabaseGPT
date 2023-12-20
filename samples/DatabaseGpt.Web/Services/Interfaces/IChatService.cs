using DatabaseGpt.Web.Models;
using OperationResults;

namespace DatabaseGpt.Web.Services.Interfaces;

public interface IChatService
{
    Task<Result<ChatResponse>> AskAsync(ChatRequest request);

    IAsyncEnumerable<string> AskStreamAsync(ChatRequest request);

    Task<Result> DeleteAsync(Guid conversationId);
}