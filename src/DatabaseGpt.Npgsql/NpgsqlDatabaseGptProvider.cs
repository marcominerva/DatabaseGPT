using System.Data;
using System.Text;
using Dapper;
using DatabaseGpt.Abstractions;
using DatabaseGpt.Abstractions.Exceptions;
using Npgsql;

namespace DatabaseGpt.Npgsql;

public class NpgsqlDatabaseGptProvider : IDatabaseGptProvider, IDisposable
{
    private readonly NpgsqlConnection connection;
    private bool disposedValue;

    public string Name => "PostgreSQL";

    public string Language => "PL/pgSQL";

    public NpgsqlDatabaseGptProvider(NpgsqlDatabaseGptProviderConfiguration settings)
    {
        connection = new NpgsqlConnection(settings.ConnectionString);
    }

    public async Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables)
    {
        var tables = await connection.QueryAsync<string>("""
            SELECT TABLE_SCHEMA || '.' || TABLE_NAME AS Tables
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA NOT IN ('pg_catalog', 'information_schema')
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

    public async Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns)
    {
        var result = new StringBuilder();
        var splittedTableNames = tables.Select(t =>
        {
            var parts = t.Split('.');
            var schema = parts[0].Trim();
            var name = parts[1].Trim();
            return new { Schema = schema, Name = name };
        });

        foreach (var table in splittedTableNames)
        {
            var query = $"""
                SELECT '[' || COLUMN_NAME || '] ' || 
                    UPPER(DATA_TYPE) || COALESCE('(' || 
                    CASE WHEN CHARACTER_MAXIMUM_LENGTH = -1 THEN 'MAX' ELSE CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) END || ')','') || ' ' || 
                    CASE WHEN IS_NULLABLE = 'YES' THEN 'NULL' ELSE 'NOT NULL' END
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table AND COLUMN_NAME NOT IN (SELECT UNNEST(@excludedColumns));
                """;

            var columns = await connection.QueryAsync<string>(query, new { schema = table.Schema, table = table.Name, ExcludedColumns = excludedColumns });
            result.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] ({string.Join(", ", columns)});");
        }

        return result.ToString();
    }

    public async Task<IDataReader> ExecuteQueryAsync(string query)
    {
        try
        {
            return await connection.ExecuteReaderAsync(query);
        }
        catch (NpgsqlException ex)
        {
            throw new DatabaseGptException("An error occurred while executing the query. See the inner exception for details", ex);
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
}
