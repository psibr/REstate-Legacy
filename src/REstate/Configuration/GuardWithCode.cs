namespace REstate.Configuration
{
    public class GuardWithCode
        : IGuardWithCode, IGuard
    {
        public int GuardId { get; set; }

        public string GuardName { get; set; }

        public string GuardDescription { get; set; }

        public int? CodeElementId { get; set; }

        public int CodeTypeId { get; set; }

        public string CodeElementName { get; set; }

        public string SemanticVersion { get; set; }

        public string CodeElementDescription { get; set; }

        public string CodeBody { get; set; }

        public int SqlDatabaseDefinitionId { get; set; }

        public string SqlDatabaseName { get; set; }

        public string SqlDatabaseDescription { get; set; }

        public string ProviderName { get; set; }

        public string ProviderDescription { get; set; }

        public string ProviderValue { get; set; }

        public string ConnectionString { get; set; }
    }
}