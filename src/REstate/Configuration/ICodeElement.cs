namespace REstate.Configuration
{
    public interface ICodeElement
    {
        int CodeElementId { get; set; }

        string ConnectorKey { get; set; }

        string CodeElementName { get; set; }

        string SemanticVersion { get; set; }

        string CodeElementDescription { get; set; }

        string CodeBody { get; set; }
    }
}