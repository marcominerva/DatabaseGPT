namespace DatabaseGpt.SqlServer.Models;

internal class ColumnEntity
{
    public string Schema { get; set; } = null!;

    public string Table { get; set; } = null!;

    public string Column { get; set; } = null!;

    public string Description { get; set; } = null!;
}
