namespace REstate.Configuration
{
    public interface ISqlDatabaseDefinitionAndProvider
        : ISqlDatabaseDefinition, ISqlDatabaseProvider
    {
        new int SqlDatabaseProviderId { get; }
    }
}