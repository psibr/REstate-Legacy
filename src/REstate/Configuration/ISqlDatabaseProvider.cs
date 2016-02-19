namespace REstate.Configuration
{
    public interface ISqlDatabaseProvider
    { 
        string ProviderName { get; }

        string ProviderDescription { get; }

        string ProviderValue { get; }
    }
}