namespace DatabaseGpt.Abstractions.Exceptions;

public class DatabaseGptException : Exception
{
    public DatabaseGptException()
    {
    }

    public DatabaseGptException(string? message) : base(message)
    {
    }

    public DatabaseGptException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
