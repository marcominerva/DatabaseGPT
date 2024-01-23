using System.Data.Common;

namespace DatabaseGpt.Models;

public class DatabaseGptQueryResult(string query, DbDataReader reader) : IDisposable
{
    private bool disposedValue;

    public string Query { get; } = query;

    public DbDataReader DataReader { get; } = reader;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                DataReader.Dispose();
            }

            disposedValue = true;
        }
    }
}
