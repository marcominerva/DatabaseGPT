using System.Data.Common;

namespace DatabaseGpt.Abstractions;

public interface IDatabaseGptProvider : IDisposable
{
    string Name { get; }

    string Language { get; }

    Task<string?> GetQueryHintsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables, CancellationToken cancellationToken = default);

    Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns, CancellationToken cancellationToken = default);

    Task<string> NormalizeQueryAsync(string query, CancellationToken cancellationToken = default);

    Task<DbDataReader> ExecuteQueryAsync(string query, CancellationToken cancellationToken = default);
}
