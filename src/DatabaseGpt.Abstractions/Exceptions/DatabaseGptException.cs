using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

    protected DatabaseGptException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
