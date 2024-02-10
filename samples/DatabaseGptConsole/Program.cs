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
                new KeyValuePair<string, string?>("DatabaseGptSettings:SystemMessage", systemMessage)
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
    },
    chatGpt =>
    {
        chatGpt.UseConfiguration(context.Configuration);
    });
}
