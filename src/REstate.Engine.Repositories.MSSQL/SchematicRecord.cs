using System;

namespace REstate.Engine.Repositories.MSSQL
{
    public class SchematicRecord
    {
        public string SchematicName { get; set; }

        public string ForkedFrom { get; set; }

        public string InitialState { get; set; }

        public string Schematic { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
