using System;

namespace REstate.Configuration
{
    public class MachineRecord
    {
        public string MachineName { get; set; }

        public string ForkedFrom { get; set; }

        public string InitialState { get; set; }

        public bool AutoIgnoreTriggers { get; set; }

        public string Definition { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
