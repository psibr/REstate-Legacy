using System;

namespace REstate.Configuration
{
    public class InstanceRecord
    {
        public string SchematicName { get; set; }

        public string StateName { get; set; }

        public string CommitTag { get; set; }

        public DateTime StateChangedDateTime { get; set; }

        public static implicit operator State(InstanceRecord record)
        {
            return new State(record.StateName, Guid.Parse(record.CommitTag));
        }
    }
}
