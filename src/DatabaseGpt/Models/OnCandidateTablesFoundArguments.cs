namespace DatabaseGpt.Models;

public class OnCandidateTablesFoundArguments(Guid sessionId, string question, IEnumerable<string> tables) : CallbackArguments(sessionId, question)
{
    public IEnumerable<string> Tables { get; } = tables;
}
