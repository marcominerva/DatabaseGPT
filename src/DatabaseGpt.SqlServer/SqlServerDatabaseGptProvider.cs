using System.Data;
using System.Text;
using Dapper;
using DatabaseGpt.Abstractions;
using DatabaseGpt.Abstractions.Exceptions;
using DatabaseGpt.SqlServer.TypeHandlers;
using Microsoft.Data.SqlClient;

namespace DatabaseGpt.SqlServer;

public class SqlServerDatabaseGptProvider : IDatabaseGptProvider, IDisposable
{
    private readonly SqlConnection connection;
    private bool disposedValue;

    public string Name => "SQL Server";

    public string Language => "T-SQL";

    static SqlServerDatabaseGptProvider()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }

    public SqlServerDatabaseGptProvider(SqlServerDatabaseGptProviderConfiguration settings)
    {
        connection = new SqlConnection(settings.ConnectionString);
        connection.Open();
    }

    public async Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> includedTables, IEnumerable<string> excludedTables)
    {
        var tables = await connection.QueryAsync<string>("SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS Tables FROM INFORMATION_SCHEMA.TABLES;");

        if (includedTables?.Any() ?? false)
        {
            tables = tables.Where(t => includedTables.Contains(t));
        }
        else if (excludedTables?.Any() ?? false)
        {
            tables = tables.Where(t => !excludedTables.Contains(t));
        }

        return tables;
    }

    public async Task<IDataReader> ExecuteQueryAsync(string query)
    {
        try
        {
            return await connection.ExecuteReaderAsync(query);
        }
        catch (SqlException ex)
        {
            throw new DatabaseGptException("An error occurred while executing the query. See the inner exception for details", ex);
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
                SELECT STUFF(
                	(SELECT ',' + '[' + COLUMN_NAME + '] ' + 
                	    UPPER(DATA_TYPE) + ISNULL('(' + IIF(CHARACTER_MAXIMUM_LENGTH = -1, 'MAX', CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10))) + ')','') + ' ' + 
                	    CASE WHEN IS_NULLABLE = 'YES' THEN 'NULL' ELSE 'NOT NULL' END
                	FROM
                        INFORMATION_SCHEMA.COLUMNS
                	WHERE
                        TABLE_SCHEMA = @schema AND TABLE_NAME = @table AND COLUMN_NAME NOT IN @excludedColumns
                	FOR XML PATH('')), 1, 1, ''
                );
                """;

            var columns = await connection.QueryAsync<string>(query, new { schema = table.Schema, table = table.Name, ExcludedColumns = excludedColumns });
            result.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] ({columns});");
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
