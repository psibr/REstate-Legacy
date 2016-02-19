namespace REstate.Configuration
{
    public interface IGuardWithCode
        : IGuard
    {
        int CodeTypeId { get; set; }

        string CodeElementName { get; set; }

        string SemanticVersion { get; set; }

        string CodeElementDescription { get; set; }

        string CodeBody { get; set; }

        int SqlDatabaseDefinitionId { get; set; }

        string SqlDatabaseName { get; set; }

        string SqlDatabaseDescription { get; set; }

        int SqlDatabaseProviderId { get; set; }

        string ProviderName { get; set; }

        string ProviderDescription { get; set; }

        string ProviderValue { get; set; }

        string ConnectionString { get; set; }
    }
}