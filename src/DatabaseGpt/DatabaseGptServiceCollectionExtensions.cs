using ChatGptNet;
using DatabaseGpt.Abstractions.Exceptions;
using DatabaseGpt.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DatabaseGpt;

public static class DatabaseGptServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, Action<DatabaseGptSettings> configureDatabaseGpt, Action<ChatGptOptionsBuilder> configureChatGpt, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabaseGpt);
        ArgumentNullException.ThrowIfNull(configureChatGpt);

        var settings = new DatabaseGptSettings();
        configureDatabaseGpt(settings);
        services.AddSingleton(settings);

        services.Add(new ServiceDescriptor(typeof(IDatabaseGptClient), typeof(DatabaseGptClient), lifetime));

        services.AddChatGpt(configureChatGpt);

        services.AddResiliencePipeline(nameof(DatabaseGptClient), (builder) =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = settings!.MaxRetries,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<ArgumentOutOfRangeException>().Handle<IndexOutOfRangeException>().Handle<DatabaseGptException>()
            });
        });

        return services;
    }

    public static DatabaseGptSettings UseConfiguration(this DatabaseGptSettings settings, IConfiguration configuration, string databaseGptSettingsSectionName = "DatabaseGptSettings")
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(databaseGptSettingsSectionName);

        configuration.GetSection(databaseGptSettingsSectionName).Bind(settings);
        return settings;
    }
}