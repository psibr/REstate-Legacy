namespace REstate.Configuration
{
    public class SqlDatabaseDefinition : ISqlDatabaseDefinition
    {
        public int SqlDatabaseDefinitionId { get; set; }
        public string SqlDatabaseName { get; set; }
        public string SqlDatabaseDescription { get; set; }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
    }
}