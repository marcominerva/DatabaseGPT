using ChatGptNet;
using ChatGptNet.Extensions;
using DatabaseGpt.Web.Models;
using DatabaseGpt.Web.Services.Interfaces;
using OperationResults;

namespace DatabaseGpt.Web.Services;

public class ChatService(IChatGptClient chatGptClient) : IChatService
{
    private const string ContentFilteredMessage = "***** (The response was filtered by the content filtering system. Please modify your prompt and retry. To learn more about content filtering policies please read the documentation: https://go.microsoft.com/fwlink/?linkid=2198766)";

    public async Task<Result<ChatResponse>> AskAsync(ChatRequest request)
    {
        var response = await chatGptClient.AskAsync(request.ConversationId, request.Message);

        var message = !response.IsContentFiltered ? response.GetContent() : ContentFilteredMessage;
        return new ChatResponse(message);
    }

    public async IAsyncEnumerable<string> AskStreamAsync(ChatRequest request)
    {
        var responseStream = chatGptClient.AskStreamAsync(request.ConversationId, request.Message);

        await foreach (var response in responseStream)
        {
            var message = !response.IsContentFiltered ? response.GetContent() : ContentFilteredMessage;
            yield return message;
        }
    }

    public async Task<Result> DeleteAsync(Guid conversationId)
    {
        await chatGptClient.DeleteConversationAsync(conversationId);
        return Result.Ok();
    }
}
