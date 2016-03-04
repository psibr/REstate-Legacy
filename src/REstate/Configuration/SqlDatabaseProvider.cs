namespace REstate.Configuration
{
    public class SqlDatabaseProvider : ISqlDatabaseProvider
    { 
        public string ProviderName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderValue { get; set; }
    }
}