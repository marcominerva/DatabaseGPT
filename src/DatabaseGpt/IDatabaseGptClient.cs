using DatabaseGpt.Models;

namespace DatabaseGpt;

public interface IDatabaseGptClient : IDisposable
{
    Task<string> GetNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default);

    Task<DatabaseGptQueryResult> ExecuteNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default);
}