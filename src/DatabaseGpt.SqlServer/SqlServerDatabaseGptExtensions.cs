using DatabaseGpt.Abstractions;
using DatabaseGpt.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseGpt;

public static class SqlServerDatabaseGptExtensions
{
    public static void AddSqlServerDatabaseGptProvider(this IServiceCollection services, string connectionString, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.AddSingleton(new SqlServerDatabaseGptProviderConfiguration { ConnectionString = connectionString });
        services.Add(new ServiceDescriptor(typeof(IDatabaseGptProvider), typeof(SqlServerDatabaseGptProvider), serviceLifetime));
    }
}