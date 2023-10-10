namespace DatabaseGpt.Exceptions;

public class NoTableFoundException : Exception
{
    public NoTableFoundException()
    {
    }

    public NoTableFoundException(string message)
        : base(message)
    {
    }

    public NoTableFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
