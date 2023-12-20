namespace DatabaseGpt.Models;

public abstract class CallbackArguments(Guid sessionId, string question)
{
    public Guid SessionId { get; } = sessionId;

    public string Question { get; } = question;
}