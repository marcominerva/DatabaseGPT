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
    public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, Action<DatabaseGptSettings> buildDatabaseGpt, Action<ChatGptOptionsBuilder> buildChatGpt, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var settings = new DatabaseGptSettings();
        buildDatabaseGpt(settings);

        services.AddSingleton(settings);
        services.Add(new ServiceDescriptor(typeof(IDatabaseGptClient), typeof(DatabaseGptClient), lifetime));
        services.AddChatGpt(buildChatGpt);
        services.AddResiliencePipeline(nameof(DatabaseGptClient), (builder) =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = settings!.MaxRetries,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder()
                    .Handle<ArgumentOutOfRangeException>()
                    .Handle<IndexOutOfRangeException>()
                    .Handle<DatabaseGptException>(),
                OnRetry = args => default
            });
        });

        return services;
    }

    public static DatabaseGptSettings UseConfiguration(this DatabaseGptSettings settings, IConfiguration configuration, string databaseSettings = "DatabaseSettings")
    {
        configuration.GetSection(databaseSettings).Bind(settings);
        return settings;
    }

    //public static IServiceCollection AddDatabaseGpt(this IServiceCollection services, IConfiguration configuration
    //    , ServiceLifetime lifetime = ServiceLifetime.Scoped, string connectionStringName = "SqlConnection"
    //    , string chatGptSectionName = "ChatGPT")
    //{
    //    var databaseSettings = ConfigureAndGet<DatabaseGptSettings>("DatabaseSettings");

    //    services.Add(new ServiceDescriptor(typeof(IDatabaseGptClient), typeof(DatabaseGptClient), lifetime));
    //    services.AddResiliencePipeline(nameof(DatabaseGptClient), (builder) =>
    //    {
    //        builder.AddRetry(new RetryStrategyOptions
    //        {
    //            MaxRetryAttempts = databaseSettings!.MaxRetries,
    //            Delay = TimeSpan.Zero,
    //            ShouldHandle = new PredicateBuilder()
    //                .Handle<ArgumentOutOfRangeException>()
    //                .Handle<IndexOutOfRangeException>()
    //                .Handle<DatabaseGptException>(),
    //            OnRetry = args => default
    //        });
    //    });

    //    return services;

    //    T? ConfigureAndGet<T>(string sectionName) where T : class
    //    {
    //        var section = configuration.GetSection(sectionName);
    //        var settings = section.Get<T>();
    //        services.Configure<T>(section);

    //        return settings;
    //    }
    //}
}