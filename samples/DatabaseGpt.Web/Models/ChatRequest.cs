namespace DatabaseGpt.Web.Models;

public record class ChatRequest(Guid ConversationId, string Message);