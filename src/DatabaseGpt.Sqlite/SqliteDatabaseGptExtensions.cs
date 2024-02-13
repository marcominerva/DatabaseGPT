using DatabaseGpt.Abstractions;
using DatabaseGpt.Sqlite;

namespace DatabaseGpt;

public static class SqliteDatabaseGptExtensions
{
    public static void UseSqlite(this IDatabaseGptSettings databaseGptSettings, string? connectionString)
    {
        databaseGptSettings.SetDatabaseGptProviderFactory(() => new SqliteDatabaseGptProvider(new()
        {
            ConnectionString = connectionString
        }));
    }
}