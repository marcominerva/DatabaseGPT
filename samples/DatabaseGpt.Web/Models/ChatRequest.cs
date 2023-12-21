using DatabaseGpt.Web.Models.Enums;

namespace DatabaseGpt.Web.Models;

public record class ChatRequest(Guid ConversationId, string Message, ResponseType ResponseType);