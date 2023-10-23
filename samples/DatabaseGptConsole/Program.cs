using ChatGptNet;
using DatabaseGpt;
using DatabaseGptConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.local.json", optional: true);

        if (File.Exists("SystemMessage.txt"))
        {
            var systemMessage = File.ReadAllText("SystemMessage.txt");
            builder.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("DatabaseSettings:SystemMessage", systemMessage)
            });
        }
    })
    .ConfigureServices(ConfigureServices)
    .Build();

var application = host.Services.GetRequiredService<Application>();
await application.ExecuteAsync();

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddSingleton<Application>();
    services.AddDatabaseGpt(database => database
            .UseConfiguration(context.Configuration)
            .UseSqlServer(context.Configuration["ConnectionStrings:SqlConnection"]),
        chatgpt => chatgpt.UseConfiguration(context.Configuration)
    );

    // For using Postgres
    // services.AddNpgsqlDatabaseGptProvider(context.Configuration["ConnectionStrings:SqlConnection"]);

    // For using SQL Server
    // services.AddSqlServerDatabaseGptProvider(context.Configuration["ConnectionStrings:SqlConnection"]);
}
