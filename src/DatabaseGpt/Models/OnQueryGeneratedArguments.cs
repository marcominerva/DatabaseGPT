namespace DatabaseGpt.Models;

public class OnQueryGeneratedArguments(Guid sessionId, string question, IEnumerable<string> tables, string query) : OnCandidateTablesFoundArguments(sessionId, question, tables)
{
    public string Query { get; } = query;
}
