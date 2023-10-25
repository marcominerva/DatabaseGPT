using DatabaseGpt.Abstractions.Exceptions;

namespace DatabaseGpt.Exceptions;

public class NoTableFoundException : DatabaseGptException
{
    public NoTableFoundException()
    {
    }

    public NoTableFoundException(string message)
        : base(message)
    {
    }

    public NoTableFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
