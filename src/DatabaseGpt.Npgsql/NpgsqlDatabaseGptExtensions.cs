using DatabaseGpt.Abstractions;
using DatabaseGpt.Npgsql;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseGpt;

public static class NpgsqlDatabaseGptExtensions
{
    public static void AddNpgsqlDatabaseGptProvider(this IServiceCollection services, string connectionString, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.AddSingleton(new NpgsqlDatabaseGptProviderConfiguration { ConnectionString = connectionString });
        services.Add(new ServiceDescriptor(typeof(IDatabaseGptProvider), typeof(NpgsqlDatabaseGptProvider), serviceLifetime));
    }
}