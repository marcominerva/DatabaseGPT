using System.Data;

namespace DatabaseGpt.Services;

public interface IDatabaseGptClient
{
    Task<IDataReader> ExecuteNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default);
}