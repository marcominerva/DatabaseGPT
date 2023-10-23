using DatabaseGpt.Abstractions;
using DatabaseGpt.Npgsql;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseGpt;

public static class NpgsqlDatabaseGptExtensions
{
    public static void UseNpgsql(this IDatabaseGptSettings databaseGptSettings, string connectionString)
    {
        databaseGptSettings.SetDatabaseGptProviderFactory(() => new NpgsqlDatabaseGptProvider(new() { ConnectionString = connectionString }));
    }
}