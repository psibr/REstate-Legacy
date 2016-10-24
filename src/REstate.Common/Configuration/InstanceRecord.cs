using System;

namespace REstate.Configuration
{
    public class InstanceRecord
    {
        public string MachineName { get; set; }

        public string StateName { get; set; }

        public string TriggerName { get; set; }

        public string ParameterData { get; set; }

        public string CommitTag { get; set; }

        public DateTime StateChangedDateTime { get; set; }
    }
}
