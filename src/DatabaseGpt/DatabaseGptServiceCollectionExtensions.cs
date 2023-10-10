using ChatGptNet;
using DatabaseGpt.DataAccessLayer;
using DatabaseGpt.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DatabaseGpt.Services;

public static class DatabaseGptServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, IConfiguration configuration, ServiceLifetime lifetime = ServiceLifetime.Scoped, string connectionStringName = "SqlConnection", string chatGptSectionName = "ChatGPT")
    {
        var databaseSettings = ConfigureAndGet<DatabaseSettings>(nameof(DatabaseSettings));
        services.Add(new ServiceDescriptor(typeof(IDatabaseGptClient), typeof(DatabaseGptClient), lifetime));

        services.AddSqlContext(options =>
        {
            options.ConnectionString = configuration.GetConnectionString(connectionStringName)!;
        }, lifetime);

        // Adds ChatGPT service using settings from IConfiguration.
        services.AddChatGpt(configuration);

        services.AddResiliencePipeline(nameof(DatabaseGptClient), (builder) =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = databaseSettings!.MaxRetries,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<ArgumentOutOfRangeException>().Handle<IndexOutOfRangeException>().Handle<SqlException>(),
                OnRetry = args =>
                {
                    Console.WriteLine($"Error ('{args.Outcome.Exception!.Message}'). Retrying (Attempt {args.AttemptNumber + 1} of {databaseSettings.MaxRetries})...");

                    return default;
                }
            });
        });

        return services;

        T? ConfigureAndGet<T>(string sectionName) where T : class
        {
            var section = configuration.GetSection(sectionName);
            var settings = section.Get<T>();
            services.Configure<T>(section);

            return settings;
        }
    }
}