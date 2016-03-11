namespace REstate.Configuration
{
    public class CodeElement : ICodeElement
    {
        public int CodeElementId { get; set; }

        public string ConnectorKey { get; set; }

        public string CodeElementName { get; set; }

        public string SemanticVersion { get; set; }

        public string CodeElementDescription { get; set; }

        public string CodeBody { get; set; }

        public int? SqlDatabaseDefinitionId { get; set; }
    }
}