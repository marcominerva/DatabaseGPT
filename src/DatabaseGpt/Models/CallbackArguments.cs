namespace DatabaseGpt.Models;

public abstract class CallbackArguments
{
    public Guid SessionId { get; }

    public string Question { get; }

    public CallbackArguments(Guid sessionId, string question)
    {
        SessionId = sessionId;
        Question = question;
    }
}