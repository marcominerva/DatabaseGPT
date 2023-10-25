using System.Data.Common;
using DatabaseGpt.Models;

namespace DatabaseGpt;

public interface IDatabaseGptClient
{
    Task<DbDataReader> ExecuteNaturalLanguageQueryAsync(Guid sessionId, string question, NaturalLanguageQueryOptions? options = null, CancellationToken cancellationToken = default);
}