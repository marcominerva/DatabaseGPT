using ChatGptNet;
using DatabaseGpt.Abstractions.Exceptions;
using DatabaseGpt.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
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
        services.AddChatGpt(configureChatGpt, httpClient => ConfigureHttpClientResiliency(httpClient));
        services.AddResiliencePipeline(settings.MaxRetries);

        return services;
    }

    public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, Action<IServiceProvider, DatabaseGptSettings> configureDatabaseGpt, Action<ChatGptOptionsBuilder> configureChatGpt, int maxQueryGenerationRetries = 3)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabaseGpt);
        ArgumentNullException.ThrowIfNull(configureChatGpt);

        services.AddScoped(provider =>
        {
            var settings = new DatabaseGptSettings();
            configureDatabaseGpt(provider, settings);
            return settings;
        });

        services.AddScoped<IDatabaseGptClient, DatabaseGptClient>();
        services.AddChatGpt(configureChatGpt, httpClient => ConfigureHttpClientResiliency(httpClient));
        services.AddResiliencePipeline(maxQueryGenerationRetries);

        return services;
    }

    public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, Action<DatabaseGptSettings> configureDatabaseGpt, Action<IServiceProvider, ChatGptOptionsBuilder> configureChatGpt, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabaseGpt);
        ArgumentNullException.ThrowIfNull(configureChatGpt);

        var settings = new DatabaseGptSettings();
        configureDatabaseGpt(settings);
        services.AddSingleton(settings);

        services.Add(new ServiceDescriptor(typeof(IDatabaseGptClient), typeof(DatabaseGptClient), lifetime));
        services.AddChatGpt(configureChatGpt, httpClient => ConfigureHttpClientResiliency(httpClient));
        services.AddResiliencePipeline(settings.MaxRetries);

        return services;
    }

    public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, Action<IServiceProvider, DatabaseGptSettings> configureDatabaseGpt, Action<IServiceProvider, ChatGptOptionsBuilder> configureChatGpt, int maxQueryGenerationRetries = 3)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabaseGpt);
        ArgumentNullException.ThrowIfNull(configureChatGpt);

        services.AddScoped(provider =>
        {
            var settings = new DatabaseGptSettings();
            configureDatabaseGpt(provider, settings);
            return settings;
        });

        services.AddScoped<IDatabaseGptClient, DatabaseGptClient>();
        services.AddChatGpt(configureChatGpt, httpClient => ConfigureHttpClientResiliency(httpClient));
        services.AddResiliencePipeline(maxQueryGenerationRetries);

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

    private static IHttpStandardResiliencePipelineBuilder ConfigureHttpClientResiliency(IHttpClientBuilder httpClient)
        => httpClient.AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(1);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(3);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
        });

    private static IServiceCollection AddResiliencePipeline(this IServiceCollection services, int maxQueryGenerationRetries = 3)
    {
        services.AddResiliencePipeline(nameof(DatabaseGptClient), builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxQueryGenerationRetries,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<ArgumentOutOfRangeException>().Handle<IndexOutOfRangeException>().Handle<DatabaseGptException>()
            });
        });

        return services;
    }
}