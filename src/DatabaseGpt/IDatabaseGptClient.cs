using System.Data.Common;
using DatabaseGpt.Models;

namespace DatabaseGpt;

public interface IDatabaseGptClient : IDisposable
{
    Task<string> GetNaturalLanguageQueryAsync(Guid sessionId, string question, CancellationToken cancellationToken = default);

    Task<DbDataReader> ExecuteNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default);
}