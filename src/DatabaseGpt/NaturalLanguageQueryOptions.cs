namespace DatabaseGpt;

public class NaturalLanguageQueryOptions
{
    public Func<IServiceProvider, ValueTask>? OnStarting { get; set; }

    public Func<IEnumerable<string>, IServiceProvider, ValueTask>? OnCandidateTablesFound { get; set; }

    public Func<string, IServiceProvider, ValueTask>? OnQueryGenerated { get; set; }
}