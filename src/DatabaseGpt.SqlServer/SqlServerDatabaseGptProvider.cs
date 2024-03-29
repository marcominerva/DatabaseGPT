﻿using System.Data;
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
                SELECT TABLE_SCHEMA AS [SCHEMA], TABLE_NAME AS [TABLE], COLUMN_NAME AS [COLUMN],
                    UPPER(DATA_TYPE) + ISNULL('(' + IIF(CHARACTER_MAXIMUM_LENGTH = -1, 'MAX', CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10))) + ')','') + ' ' + 
                    CASE WHEN IS_NULLABLE = 'YES' THEN 'NULL' ELSE 'NOT NULL' END AS DESCRIPTION
                FROM
                    INFORMATION_SCHEMA.COLUMNS
                WHERE
                    TABLE_SCHEMA = @schema AND TABLE_NAME = @table
                """;

            var allColumns = await connection.QueryAsync<ColumnEntity>(query, new { schema = table.Schema, table = table.Name });

            var columns = allColumns.Where(c => IsIncluded(c, excludedColumns))
                .Select(c => $"[{c.Column}] {c.Description}").ToList();

            result.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] ({string.Join(',', columns)});");
        }

        return result.ToString();

        static bool IsIncluded(ColumnEntity column, IEnumerable<string> excludedColumns)
        {
            // Checks if the column should be included or not, verifying if it is present in the list of excluded columns.
            var isIncluded = excludedColumns.All(e =>
            {
                var parts = e.Split('.');
                var schemaOrColumnName = parts.ElementAtOrDefault(0)?.Trim();
                var tableName = parts.ElementAtOrDefault(1)?.Trim();
                var columnName = parts.ElementAtOrDefault(2)?.Trim();

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    // The columnName variable is null: it means that the excluded column has been specified without the full qualified name.
                    // In this case, the schemaOrColumnName variable contains the column name and it is a column that must be excluded from all tables.
                    var isExcluded = column.Column.Equals(schemaOrColumnName, StringComparison.OrdinalIgnoreCase);
                    return !isExcluded;
                }
                else
                {
                    // The excluded column has been specified using the full qualified name.
                    var isExcluded = column.Schema.Equals(schemaOrColumnName, StringComparison.OrdinalIgnoreCase)
                       && column.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                       && column.Column.Equals(columnName, StringComparison.OrdinalIgnoreCase);

                    return !isExcluded;
                }
            });

            return isIncluded;
        }
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
