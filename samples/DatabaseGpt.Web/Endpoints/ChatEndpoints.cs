using DatabaseGpt.Web.Models;
using DatabaseGpt.Web.Services.Interfaces;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace ChatGptPlayground.Endpoints;

public class ChatEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var chatGroupApi = endpoints.MapGroup("/api/chat");

        chatGroupApi.MapPost("ask", AskAsync)
            .WithName("Ask")
            .Produces<ChatResponse>()
            .ProducesValidationProblem()
            .WithOpenApi();
    }

    public static async Task<IResult> AskAsync(ChatRequest request, IChatService chatService, HttpContext httpContext)
    {
        var result = await chatService.AskAsync(request);

        var response = httpContext.CreateResponse(result);
        return response;
    }
}