namespace DatabaseGpt.Models;

public class NaturalLanguageQueryOptions
{
    public Func<IServiceProvider, ValueTask>? OnStarting { get; set; }

    public Func<OnCandidateTablesFoundArguments, IServiceProvider, ValueTask>? OnCandidateTablesFound { get; set; }

    public Func<OnQueryGeneratedArguments, IServiceProvider, ValueTask>? OnQueryGenerated { get; set; }
}