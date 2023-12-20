namespace DatabaseGpt.Models;

public class OnQueryGeneratedArguments(Guid sessionId, string question, IEnumerable<string> tables, string sql) : OnCandidateTablesFoundArguments(sessionId, question, tables)
{
    public string Sql { get; } = sql;
}
