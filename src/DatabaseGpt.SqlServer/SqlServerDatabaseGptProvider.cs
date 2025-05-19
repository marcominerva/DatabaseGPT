using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using DatabaseGpt.Abstractions;
using DatabaseGpt.Abstractions.Exceptions;
using DatabaseGpt.SqlServer.Models;
using Microsoft.Data.SqlClient;

namespace DatabaseGpt.SqlServer;

public class SqlServerDatabaseGptProvider(SqlServerDatabaseGptProviderConfiguration settings) : IDatabaseGptProvider
{
    private readonly SqlConnection connection = new(settings.ConnectionString);

    private bool disposedValue;

    public string Name => "SQL Server";

    public string Language => "T-SQL";

    public async Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var query = "SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS TABLES FROM INFORMATION_SCHEMA.TABLES";
        IEnumerable<string>? tablesToQuery = null;

        if (includedTables?.Any() ?? false)
        {
            query = $"{query} WHERE TABLE_SCHEMA + '.' + TABLE_NAME IN @tables";
            tablesToQuery = includedTables;
        }
        else if (excludedTables?.Any() ?? false)
        {
            query = $"{query} WHERE TABLE_SCHEMA + '.' + TABLE_NAME NOT IN @tables";
            tablesToQuery = excludedTables;
        }

        var tables = await connection.QueryAsync<string>(query, new { tables = tablesToQuery });
        return tables;
    }

    public async Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var result = new StringBuilder();
        var splittedTableNames = tables.Select(t =>
        {
            var parts = t.Split('.', StringSplitOptions.TrimEntries);
            var schema = parts[0].TrimStart('[').TrimEnd(']');
            var name = parts[1].TrimStart('[').TrimEnd(']');
            return new { Schema = schema, Name = name };
        });

        foreach (var table in splittedTableNames)
        {
            var query = $"""
                SELECT '[' + COLUMN_NAME + '] ' + 
                    UPPER(DATA_TYPE) + COALESCE('(' + 
                    CASE WHEN CHARACTER_MAXIMUM_LENGTH = -1 THEN 'MAX' ELSE CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) END + ')','') + ' ' + 
                    CASE WHEN IS_NULLABLE = 'YES' THEN 'NULL' ELSE 'NOT NULL' END
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @schema
                    AND TABLE_NAME = @table
                    AND COLUMN_NAME NOT IN @{nameof(excludedColumns)}
                    AND TABLE_SCHEMA + '.' + TABLE_NAME + '.' + COLUMN_NAME NOT IN @{nameof(excludedColumns)};
                """;

            var columns = await connection.QueryAsync<ColumnEntity>(query, new { schema = table.Schema, table = table.Name, excludedColumns });

            result.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] ({string.Join(',', columns)});");
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
        catch (SqlException ex)
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
