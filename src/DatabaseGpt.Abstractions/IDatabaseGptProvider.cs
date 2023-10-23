using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseGpt.Abstractions;

public interface IDatabaseGptProvider
{
    Task<IEnumerable<string>> GetTablesAsync(IEnumerable<string> excludedTables);
    Task<string> GetCreateTablesScriptAsync(IEnumerable<string> tables, IEnumerable<string> excludedColumns);
    Task<IDataReader> ExecuteQueryAsync(string query);

    string Name { get; }
    string Language { get; }
}
