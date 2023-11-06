using System.Data.Common;

namespace DatabaseGpt.Abstractions;

public interface IDatabaseGptProvider : IDisposable
{
    string Name { get; }

    string Language { get; }

    Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables);

    Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns);

    Task<DbDataReader> ExecuteQueryAsync(string query);
}
