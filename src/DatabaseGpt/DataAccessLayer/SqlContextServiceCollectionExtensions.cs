using Microsoft.Extensions.DependencyInjection;

namespace DatabaseGpt.DataAccessLayer;

public static class SqlContextServiceCollectionExtensions
{
    public static IServiceCollection AddSqlContext(this IServiceCollection services, Action<SqlContextOptions> configuration, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new SqlContextOptions();
        configuration.Invoke(options);

        services.AddSingleton(options);
        services.Add(new ServiceDescriptor(typeof(ISqlContext), typeof(SqlContext), lifetime));

        return services;
    }
}
