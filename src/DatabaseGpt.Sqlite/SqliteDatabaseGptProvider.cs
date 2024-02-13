using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using DatabaseGpt.Abstractions;
using DatabaseGpt.Abstractions.Exceptions;
using Microsoft.Data.Sqlite;

namespace DatabaseGpt.Sqlite;

public class SqliteDatabaseGptProvider(SqliteDatabaseGptProviderConfiguration settings) : IDatabaseGptProvider
{
    private readonly SqliteConnection connection = new(settings.ConnectionString);

    private bool disposedValue;

    public string Name => "SQLite";

    public string Language => "SQLite";

    public async Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var tables = await connection.QueryAsync<string>("""
            SELECT TBL_NAME AS Tables
            FROM sqlite_schema
            WHERE type IN ('table','view') AND name NOT LIKE 'sqlite_%'
            """);

        if (includedTables?.Any() ?? false)
        {
            tables = tables.Where(t => includedTables.Contains(t, StringComparer.InvariantCultureIgnoreCase));
        }
        else if (excludedTables?.Any() ?? false)
        {
            tables = tables.Where(t => !excludedTables.Contains(t, StringComparer.InvariantCultureIgnoreCase));
        }

        return tables;
    }

    public async Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var result = new StringBuilder();

        foreach (var table in tables)
        {
            var query = $"""
                SELECT '[' || NAME || '] ' ||
                    UPPER(TYPE) || ' ' ||
                    CASE WHEN [NOTNULL] = 0 THEN 'NULL' ELSE 'NOT NULL' END
                FROM PRAGMA_TABLE_INFO(@table)
                WHERE NAME NOT IN (@excludedColumns)
                AND @table || '.' || NAME NOT IN (@excludedColumns);
                """;

            var columns = await connection.QueryAsync<string>(query, new { table, excludedColumns });
            result.AppendLine($"CREATE TABLE [{table}] ({string.Join(", ", columns)});");
        }

        return result.ToString();
    }

    public Task<string?> GetQueryHintsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return Task.FromResult<string?>(null);
    }

    public Task<string> NormalizeQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return Task.FromResult(query);
    }

    public async Task<DbDataReader> ExecuteQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            return await connection.ExecuteReaderAsync(query);
        }
        catch (SqliteException ex)
        {
            throw new DatabaseGptException("An error occurred while executing the query. See the inner exception for details.", ex);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                connection.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(disposedValue, this);
}
