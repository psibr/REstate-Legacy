namespace REstate.Configuration
{
    public class SqlDatabaseDefinitionAndProvider 
        : ISqlDatabaseDefinitionAndProvider
    {
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