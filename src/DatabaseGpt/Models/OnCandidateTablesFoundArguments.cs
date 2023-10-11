namespace DatabaseGpt.Models;

public class OnCandidateTablesFoundArguments : CallbackArguments
{
    public IEnumerable<string> Tables { get; }

    public OnCandidateTablesFoundArguments(Guid sessionId, string question, IEnumerable<string> tables)
        : base(sessionId, question)
    {
        Tables = tables;
    }
}
