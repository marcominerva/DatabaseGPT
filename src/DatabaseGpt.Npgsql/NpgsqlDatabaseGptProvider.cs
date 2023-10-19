using System.Data;
using System.Text;
using Dapper;
using DatabaseGpt.Abstractions;
using DatabaseGpt.Abstractions.Exceptions;
using Npgsql;

namespace DatabaseGpt.Npgsql;

public class NpgsqlDatabaseGptProvider : IDatabaseGptProvider, IDisposable
{
    private readonly NpgsqlDatabaseGptProviderConfiguration settings;
    private bool disposedValue;

    public NpgsqlDatabaseGptProvider(NpgsqlDatabaseGptProviderConfiguration settings)
    {
        this.settings = settings;
        this.connection = new NpgsqlConnection(settings.ConnectionString);
        connection.Open();
    }

    private readonly NpgsqlConnection connection;

    public string Name => "Postgres";
    public string Language => "Postgres SQL";

    public async Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> excludedTables)
    {
        var tables = await connection.QueryAsync<string>("""
            SELECT TABLE_SCHEMA || '.' || TABLE_NAME AS Tables
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA NOT IN ('pg_catalog', 'information_schema')
            """);
        return tables.Where(t => !excludedTables.Contains(t));
    }

    public async Task<IDataReader> ExecuteQueryAsync(string query)
    {
        try
        {
            return await connection.ExecuteReaderAsync(query);
        }
        catch (NpgsqlException ex)
        {
            throw new DatabaseGptException("An error occured. Se inner exception for details", ex);
        }
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
