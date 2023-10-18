using System.Data;

namespace DatabaseGpt.Extensions;

public static class IDataReaderExtensions
{
    public static IEnumerable<string> GetColumnNames(this IDataReader reader)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            yield return reader.GetName(i);
        }
    }
}
