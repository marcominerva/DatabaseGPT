using DatabaseGpt;
using DatabaseGpt.Exceptions;
using DatabaseGpt.Extensions;
using DatabaseGpt.Settings;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace DatabaseGptConsole;

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
        AnsiConsole.Write(new FigletText("DatabaseGPT").LeftJustified());

        AnsiConsole.WriteLine($"""
            The following rules will be applied:
            {databaseSettings.SystemMessage}
            """);

        AnsiConsole.WriteLine();

        var options = new NaturalLanguageQueryOptions
        {
            OnStarting = (_) =>
            {
                AnsiConsole.Write("I'm thinking...");

                return default;
            },
            OnCandidateTablesFound = (tables, _) =>
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                AnsiConsole.Write($"I think the following tables might be useful: {string.Join(", ", tables)}.");

                return default;
            },
            OnQueryGenerated = (sql, _) =>
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                AnsiConsole.WriteLine("The query to answer the question should be the following:");

                AnsiConsole.WriteLine(sql);
                AnsiConsole.WriteLine();

                return default;
            }
        };

        var conversationId = Guid.NewGuid();
        string? question = null;

        do
        {
            try
            {
                AnsiConsole.Markup("Ask me anything: ");
                question = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(question))
                {
                    using var reader = await databaseGptClient.ExecuteNaturalLanguageQueryAsync(conversationId, question, options);

                    var table = new Table();
                    table.AddColumns(reader.GetColumnNames().ToArray());

                    while (reader.Read())
                    {
                        var values = new List<Markup>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var value = new Markup(reader[i]?.ToString() ?? string.Empty);

                            var type = reader.GetFieldType(i);
                            if (type != typeof(string))
                            {
                                value.RightJustified();
                            }

                            values.Add(value);
                        }

                        table.AddRow(values.ToArray());
                    }

                    AnsiConsole.Write(table);
                }
            }
            catch (NoTableFoundException)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Markup($"I'm sorry, but there's no available information in the provided tables that can be useful for the question [green]{question}[/].");
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex,
                    ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
            }
            finally
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();
            }
        } while (!string.IsNullOrWhiteSpace(question));
    }
}
