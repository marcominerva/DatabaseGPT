using System.Data;
using System.Data.Common;
using System.Text;
using DatabaseGpt.Web.Models;
using DatabaseGpt.Web.Models.Enums;
using DatabaseGpt.Web.Services.Interfaces;
using OperationResults;

namespace DatabaseGpt.Web.Services;

public class ChatService(IDatabaseGptClient databaseGptClient) : IChatService
{
    public async Task<Result<ChatResponse>> AskAsync(ChatRequest request)
    {
        using var result = await databaseGptClient.ExecuteNaturalLanguageQueryAsync(request.ConversationId, request.Message);

        var query = request.ResponseType switch
        {
            ResponseType.Query or ResponseType.QueryAndTable => GetFormattedQuery(result.Query),
            _ => null
        };

        var table = request.ResponseType switch
        {
            ResponseType.Table or ResponseType.QueryAndTable => GetMarkdownTable(result.DataReader),
            _ => null
        };

        return new ChatResponse(query, table);
    }

    public static string GetFormattedQuery(string query)
        => $"<pre>{query.Replace("\r\n", "<br />").Replace("\n", "<br />")}</pre>";

    public static string GetMarkdownTable(DbDataReader reader)
    {
        var columns = GetColumnNames(reader).ToArray();
        var header = $"|{string.Join('|', columns)}|";

        var fieldTypes = GetFieldTypes(reader).ToArray();
        var rowSeparators = $"{string.Concat(fieldTypes.Select(t => t == typeof(string) || t == typeof(Guid) || t == typeof(Guid?) ? "|----" : "|----:"))}|";

        var values = new StringBuilder();
        while (reader.Read())
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = GetValue(reader, i);
                values.Append('|').Append(value);
            }

            values.AppendLine("|");
        }

        var table = $"{header}{Environment.NewLine}{rowSeparators}{Environment.NewLine}{values}";
        return table;

        static IEnumerable<string> GetColumnNames(IDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                yield return reader.GetName(i);
            }
        }

        static IEnumerable<Type> GetFieldTypes(IDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                yield return reader.GetFieldType(i);
            }
        }

        static string GetValue(DbDataReader reader, int index)
        {
            var dataType = reader.GetDataTypeName(index);
            var value = dataType switch
            {
                "date" when reader.GetValue(index) != DBNull.Value => reader.GetFieldValue<DateOnly>(index).ToString(),
                "time" when reader.GetValue(index) != DBNull.Value => reader.GetFieldValue<TimeOnly>(index).ToString(),
                _ => reader[index]?.ToString()
            };

            return value;
        }
    }
}