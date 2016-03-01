using System;

namespace REstate.Web.Instances.Requests
{
    public class TriggerFireRequest
    {
        public Guid MachineInstanceGuid { get; set; }

        public string TriggerName { get; set; }

        public string Payload { get; set; }
    }
}
