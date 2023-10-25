using DatabaseGpt.Abstractions;
using DatabaseGpt.Npgsql;

namespace DatabaseGpt;

public static class NpgsqlDatabaseGptExtensions
{
    public static void UseNpgsql(this IDatabaseGptSettings databaseGptSettings, string? connectionString)
    {
        databaseGptSettings.SetDatabaseGptProviderFactory(() => new NpgsqlDatabaseGptProvider(new()
        {
            ConnectionString = connectionString
        }));
    }
}