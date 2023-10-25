using DatabaseGpt.Abstractions;
using DatabaseGpt.SqlServer;

namespace DatabaseGpt;

public static class SqlServerDatabaseGptExtensions
{
    public static void UseSqlServer(this IDatabaseGptSettings databaseGptSettings, string? connectionString)
    {
        databaseGptSettings.SetDatabaseGptProviderFactory(() => new SqlServerDatabaseGptProvider(new()
        {
            ConnectionString = connectionString
        }));
    }
}