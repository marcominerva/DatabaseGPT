using DatabaseGpt.Abstractions.Exceptions;

namespace DatabaseGpt.Exceptions;

public class InvalidSqlException : DatabaseGptException
{
    public InvalidSqlException()
    {
    }

    public InvalidSqlException(string message)
        : base(message)
    {
    }

    public InvalidSqlException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}