using ConsoleTables;
using DatabaseGpt.Exceptions;
using DatabaseGpt.Extensions;
using DatabaseGpt.Services;
using DatabaseGpt.Settings;
using Microsoft.Extensions.Options;

namespace DatabaseGpt;

internal class Application
{
    private readonly IDatabaseGptClient databaseGptClient;
    private readonly DatabaseSettings databaseSettings;

    public Application(IDatabaseGptClient databaseGptClient, IOptions<DatabaseSettings> databaseSettingsOptions)
    {
        this.databaseGptClient = databaseGptClient;
        databaseSettings = databaseSettingsOptions.Value;
    }

    public async Task ExecuteAsync()
    {
        var conversationId = Guid.NewGuid();

        Console.WriteLine($"""
            The following rules will be applied:
            {databaseSettings.SystemMessage}
            """);

        Console.WriteLine();

        var options = new NaturalLanguageQueryOptions
        {
            OnStarting = (_) =>
            {
                Console.WriteLine("I'm thinking...");

                return default;
            },
            OnCandidateTablesFound = (tables, _) =>
            {
                Console.WriteLine();
                Console.WriteLine($"I think the following tables might be useful: {string.Join(", ", tables)}.");

                return default;
            },
            OnQueryGenerated = (sql, _) =>
            {
                Console.WriteLine();
                Console.WriteLine("The query to answer the question should be the following:");
                Console.WriteLine(sql);
                Console.WriteLine();

                return default;
            }
        };

        string? question = null;
        do
        {
            try
            {
                Console.Write("Ask me anything: ");
                question = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(question))
                {
                    using var reader = await databaseGptClient.ExecuteNaturalLanguageQueryAsync(conversationId, question, options);

                    var table = new ConsoleTable(reader.GetColumnNames().ToArray());
                    table.Options.NumberAlignment = Alignment.Right;
                    table.Options.EnableCount = false;

                    while (reader.Read())
                    {
                        var values = new List<string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            values.Add(reader[i]?.ToString() ?? string.Empty);
                        }

                        table.AddRow(values.ToArray());
                    }

                    table.Write();
                }
            }
            catch (NoTableFoundException)
            {
                Console.WriteLine();
                Console.WriteLine($"I'm sorry, but there's no available information in the provided tables that can be useful for the question '{question}'.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Message);

                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine();
            }
        } while (!string.IsNullOrWhiteSpace(question));
    }
}
