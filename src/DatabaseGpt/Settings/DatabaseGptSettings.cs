using DatabaseGpt.Abstractions;

namespace DatabaseGpt.Settings;

public class DatabaseGptSettings : IDatabaseGptSettings
{
    private Func<IDatabaseGptProvider> providerFactory = null!;

    public string? SystemMessage { get; set; }

    public string[] IncludedTables { get; set; } = Array.Empty<string>();

    public string[] ExcludedTables { get; set; } = Array.Empty<string>();

    public string[] ExcludedColumns { get; set; } = Array.Empty<string>();

    public int MaxRetries { get; set; } = 3;

    public void SetDatabaseGptProviderFactory(Func<IDatabaseGptProvider> providerFactory)
    {
        ArgumentNullException.ThrowIfNull(providerFactory);

        this.providerFactory = providerFactory;
    }

    internal IDatabaseGptProvider CreateProvider() => providerFactory.Invoke();
}
