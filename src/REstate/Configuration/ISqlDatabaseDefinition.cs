namespace REstate.Configuration
{
    public interface ISqlDatabaseDefinition
    {
        int SqlDatabaseDefinitionId { get;}

        string SqlDatabaseName { get; }

        string SqlDatabaseDescription { get;}

        string ConnectionString { get; }

        int SqlDatabaseProviderId { get; }
    }
}