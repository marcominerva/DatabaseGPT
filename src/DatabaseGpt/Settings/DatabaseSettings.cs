namespace DatabaseGpt.Settings;

public class DatabaseSettings
{
    public string SystemMessage { get; init; } = "You are an AI assistant that helps people find information.";

    public string[] IncludedTables { get; init; } = Array.Empty<string>();

    public string[] ExcludedTables { get; init; } = Array.Empty<string>();

    public string[] ExcludedColumns { get; init; } = Array.Empty<string>();

    public int MaxRetries { get; init; } = 3;
}
