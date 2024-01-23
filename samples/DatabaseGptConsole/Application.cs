using DatabaseGpt;
using DatabaseGpt.Exceptions;
using DatabaseGpt.Extensions;
using DatabaseGpt.Models;
using DatabaseGpt.Settings;
using Spectre.Console;

namespace DatabaseGptConsole;

internal class Application(IDatabaseGptClient databaseGptClient, DatabaseGptSettings databaseSettings)
{
    public async Task ExecuteAsync()
    {
        AnsiConsole.Write(new FigletText("DatabaseGPT").LeftJustified());

        if (!string.IsNullOrWhiteSpace(databaseSettings.SystemMessage))
        {
            AnsiConsole.WriteLine($"""
                The following rules will be applied:
                {databaseSettings.SystemMessage}
                """);
        }

        AnsiConsole.WriteLine();

        var options = new NaturalLanguageQueryOptions
        {
            OnStarting = (_) =>
            {
                AnsiConsole.Write("I'm thinking...");

                return default;
            },
            OnCandidateTablesFound = (args, _) =>
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                AnsiConsole.Write($"I think the following tables might be useful: {string.Join(", ", args.Tables)}.");

                return default;
            },
            OnQueryGenerated = (args, _) =>
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                AnsiConsole.WriteLine("The query to answer the question should be the following:");

                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine(args.Query);
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
                    using var result = await databaseGptClient.ExecuteNaturalLanguageQueryAsync(conversationId, question, options);
                    var reader = result.DataReader;

                    var table = new Table();
                    var columns = reader.GetColumnNames().Select(c => $"[olive]{c}[/]").ToArray();
                    table.AddColumns(columns);

                    while (reader.Read())
                    {
                        var values = new List<Markup>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var dataType = reader.GetDataTypeName(i);
                            var value = dataType switch
                            {
                                "date" when reader.GetValue(i) != DBNull.Value => reader.GetFieldValue<DateOnly>(i).ToString(),
                                "time" when reader.GetValue(i) != DBNull.Value => reader.GetFieldValue<TimeOnly>(i).ToString(),
                                _ => reader[i]?.ToString()
                            };

                            var markup = new Markup(value?.EscapeMarkup() ?? string.Empty);

                            var type = reader!.GetFieldType(i);
                            if (type != typeof(string) && type != typeof(Guid) && type != typeof(Guid?))
                            {
                                markup.RightJustified();
                            }

                            values.Add(markup);
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
            catch (InvalidSqlException)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Markup($"I'm sorry, but the question [green]{question}[/] requires an INSERT, UPDATE or DELETE query, that aren't supported.");
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex,
                    ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenMethods);
            }
            finally
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();
            }
        } while (!string.IsNullOrWhiteSpace(question));
    }
}
