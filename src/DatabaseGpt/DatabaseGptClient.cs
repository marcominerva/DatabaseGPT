using ChatGptNet;
using ChatGptNet.Extensions;
using DatabaseGpt.Abstractions;
using DatabaseGpt.Exceptions;
using DatabaseGpt.Models;
using DatabaseGpt.Settings;
using Polly;
using Polly.Registry;

namespace DatabaseGpt;

internal class DatabaseGptClient(IChatGptClient chatGptClient, ResiliencePipelineProvider<string> pipelineProvider, IServiceProvider serviceProvider, DatabaseGptSettings databaseGptSettings) : IDatabaseGptClient
{
    private readonly IDatabaseGptProvider provider = databaseGptSettings.CreateProvider();
    private readonly ResiliencePipeline pipeline = pipelineProvider.GetPipeline(nameof(DatabaseGptClient));

    private bool disposedValue;

    public async Task<string> GetNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteNaturalLanguageQueryInternalAsync(sessionId, question, options, cancellationToken);
        return result.Query;
    }

    public async Task<DatabaseGptQueryResult> ExecuteNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteNaturalLanguageQueryInternalAsync(sessionId, question, options, cancellationToken);
        return result;
    }

    private async Task<DatabaseGptQueryResult> ExecuteNaturalLanguageQueryInternalAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        sessionId = await CreateSessionAsync(sessionId, cancellationToken);

        var (query, reader) = await pipeline.ExecuteAsync(async cancellationToken =>
        {
            var tables = await GetTablesAsync(sessionId, question, options, cancellationToken);
            var query = await GetQueryAsync(sessionId, question, tables, options, cancellationToken);

            var reader = await provider.ExecuteQueryAsync(query, cancellationToken);
            return (query, reader);
        }, cancellationToken);

        return new(query, reader);
    }

    private async Task<Guid> CreateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var conversationExists = await chatGptClient.ConversationExistsAsync(sessionId, cancellationToken);
        if (!conversationExists)
        {
            var tables = await provider.GetTablesAsync(databaseGptSettings.IncludedTables, databaseGptSettings.ExcludedTables, cancellationToken);

            var systemMessage = $"""
                You are an assistant that answers questions using the information stored in a {provider.Name} database and the {provider.Language} language.
                Your answers can only reference one or more of the following tables: '{string.Join(',', tables)}'.
                You can create only {provider.Language} SELECT queries. Never create INSERT, UPDATE nor DELETE command.
                """;

            if (!string.IsNullOrWhiteSpace(databaseGptSettings.SystemMessage))
            {
                systemMessage += $"""
                    When you create a {provider.Language} query, consider the following information:
                    {databaseGptSettings.SystemMessage}
                    """;
            }

            sessionId = await chatGptClient.SetupAsync(sessionId, systemMessage, cancellationToken);
        }

        return sessionId;
    }

    private async Task<IEnumerable<string>> GetTablesAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options, CancellationToken cancellationToken)
    {
        var request = $"""
                You must answer the following question, '{question}', using a {provider.Language} query for a {provider.Name} database. Take into account the context of the chat to complete the question, adding the context from the chat.
                From the comma separated list of tables available in the database, select those tables that might be useful in the generated {provider.Language} query.
                The selected tables should be returned in a comma separated list. Your response should just contain the comma separated list of selected tables.
                The table name should always contain the schema name. For example, if the table name is 'Table1' and the schema name is 'dbo', then the table name should be 'dbo.Table1'.
                If there are no tables that might be useful, return only the string 'NONE', without any other words. You shouldn't never explain the reason why you haven't found any table.
                """;
        // If the question is unclear or you don't understand the question, or you need a clarification, then return only the string 'NONE', without any other words. 

        if (options?.OnStarting is not null)
        {
            await options.OnStarting.Invoke(serviceProvider);
        }

        var response = await chatGptClient.AskAsync(sessionId, request, cancellationToken: cancellationToken);

        var candidateTables = response.GetContent()!.Trim('\'');
        if (candidateTables == "NONE")
        {
            throw new NoTableFoundException($"No available information in the provided tables can be useful for the question '{question}'.");
        }

        var tables = candidateTables.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (options?.OnCandidateTablesFound is not null)
        {
            await options.OnCandidateTablesFound.Invoke(new(sessionId, question, tables), serviceProvider);
        }

        return tables;
    }

    private async Task<string> GetQueryAsync(Guid sessionId, string question, IEnumerable<string> tables, NaturalLanguageQueryOptions? options, CancellationToken cancellationToken)
    {
        var createTableScripts = await provider.GetCreateTablesScriptAsync(tables, databaseGptSettings.ExcludedColumns, cancellationToken);

        var request = $"""
                A {provider.Name} database contains the following tables and columns:
                {createTableScripts}
                Generate a {provider.Language} query for a {provider.Name} database to answer the question: '{question}' - The query must only reference table names and column names that appear in this request.
                For example, if the request contains the following CREATE TABLE statements:
                CREATE TABLE Table1 (Column1 VARCHAR(255), Column2 VARCHAR(255))
                CREATE TABLE Table2 (Column3 VARCHAR(255), Column4 VARCHAR(255))
                Then you should only reference tables Table1 and Table2 and the query should only reference columns Column1, Column2, Column3 and Column4.
                Your response should only contain the {provider.Language} query for a {provider.Name} database, no other information is required. For example, never explain the meaning of the query nor explain how to use the query.
                The query should only contain the SQL keywords that are available in a {provider.Language} SELECT query. For example, if the database is SQL SERVER, then the query should not contain the LIMIT keyword.
                You can create only SELECT queries. Never create INSERT, UPDATE nor DELETE commands.
                If the question of the user requires an INSERT, UPDATE or DELETE command, then return only the string 'NONE', without any other words. You shouldn't never explain the reason why you haven't created the query.
                If you use the AS keyword to rename the columns, then you should use the following format: 'AS ColumnName' and not 'AS Column Name'. The renamed name should not contain spaces and should be in the same language of the user question.                
                The response should not contain any markdown tags. For example, the response should not contain '```sql' or '```'.    
                """;

        // Add specific instructions, if any, for the current provider.
        var queryHints = await provider.GetQueryHintsAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(queryHints))
        {
            request += $"{Environment.NewLine}{queryHints}";
        }

        var response = await chatGptClient.AskAsync(sessionId, request, cancellationToken: cancellationToken);

        var query = response.GetContent()!;
        if (query == "NONE")
        {
            throw new InvalidSqlException($"The question '{question}' requires an INSERT, UPDATE or DELETE command, that isn't supported.");
        }

        query = query.Replace("```sql", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("```", string.Empty).Trim('\n');

        // Calls the provider to validate the query.
        query = await provider.NormalizeQueryAsync(query, cancellationToken);

        if (options?.OnQueryGenerated is not null)
        {
            await options.OnQueryGenerated.Invoke(new(sessionId, question, tables, query), serviceProvider);
        }

        return query;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                provider.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(disposedValue, this);
}