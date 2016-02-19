namespace REstate.Configuration
{
    public interface ICodeWithDatabaseConfiguration
        : ICodeElement, ISqlDatabaseDefinition, ISqlDatabaseProvider
    {
        new int SqlDatabaseProviderId { get; set; }
    }

    public class CodeWithDatabaseConfiguration 
        : ICodeWithDatabaseConfiguration
    {
        public int CodeElementId { get; set; }
        public int CodeTypeId { get; set; }
        public string CodeElementName { get; set; }
        public string SemanticVersion { get; set; }
        public string CodeElementDescription { get; set; }
        public string CodeBody { get; set; }
        public int SqlDatabaseDefinitionId { get; set; }
        public string SqlDatabaseName { get; set; }
        public string SqlDatabaseDescription { get; set; }
        public string ConnectionString { get; set; }
        public int SqlDatabaseProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderValue { get; set; }
    }
}