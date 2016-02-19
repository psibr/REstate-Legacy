namespace REstate.Configuration
{
    public interface ICodeElement
    {
        int CodeElementId { get; set; }

        int CodeTypeId { get; set; }

        string CodeElementName { get; set; }

        string SemanticVersion { get; set; }

        string CodeElementDescription { get; set; }

        string CodeBody { get; set; }
    }
}