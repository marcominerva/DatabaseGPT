namespace DatabaseGpt.Abstractions;

public interface IDatabaseGptSettings
{
    public void SetDatabaseGptProviderFactory(Func<IDatabaseGptProvider> providerFactory);
}