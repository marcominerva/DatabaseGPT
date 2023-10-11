namespace DatabaseGpt.Models;

public class OnQueryGeneratedArguments : OnCandidateTablesFoundArguments
{
    public string Sql { get; }

    public OnQueryGeneratedArguments(Guid sessionId, string question, IEnumerable<string> tables, string sql)
        : base(sessionId, question, tables)
    {
        Sql = sql;
    }
}
