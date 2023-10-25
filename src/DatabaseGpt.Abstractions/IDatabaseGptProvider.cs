using System.Data;

namespace DatabaseGpt.Abstractions;

public interface IDatabaseGptProvider
{
    string Name { get; }

    string Language { get; }

    Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables);

    Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns);

    Task<IDataReader> ExecuteQueryAsync(string query);
}
