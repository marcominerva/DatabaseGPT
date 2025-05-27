using Azure;
using Azure.AI.OpenAI;
using DatabaseGpt;
using DatabaseGptConsole;
using Microsoft.Extensions.AI;
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
            builder.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("DatabaseGptSettings:SystemMessage", systemMessage)
            ]);
        }
    })
    .ConfigureServices(ConfigureServices)
    .Build();

var application = host.Services.GetRequiredService<Application>();
await application.ExecuteAsync();

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddSingleton<Application>();

    services.AddHybridCache();

    var apiKey = context.Configuration.GetValue<string>("ChatGPT:ApiKey")!;
    var deploymentName = context.Configuration.GetValue<string>("ChatGPT:DeploymentName")!;
    var endpoint = context.Configuration.GetValue<string>("ChatGpt:Endpoint")!;

    var azureOpenAIClient = new AzureOpenAIClient(new(endpoint), new AzureKeyCredential(apiKey));
    var chatClient = azureOpenAIClient.GetChatClient(deploymentName).AsIChatClient();

    services.AddChatClient(chatClient);

    services.AddDatabaseGpt(database =>
    {
        // For SQL Server.
        database.UseConfiguration(context.Configuration)
                .UseSqlServer(context.Configuration.GetConnectionString("SqlConnection"));

        // For PostgreSQL.
        //database.UseConfiguration(context.Configuration)
        //        .UseNpgsql(context.Configuration.GetConnectionString("NpgsqlConnection"));

        // For SQLite.
        //database.UseConfiguration(context.Configuration)
        //        .UseSqlite(context.Configuration.GetConnectionString("SqliteConnection"));
    });
}
