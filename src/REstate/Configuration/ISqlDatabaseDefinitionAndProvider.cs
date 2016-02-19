namespace REstate.Configuration
{
    public interface ISqlDatabaseDefinitionAndProvider
        : ISqlDatabaseDefinition, ISqlDatabaseProvider
    {
        new string ProviderName { get; }
    }
}