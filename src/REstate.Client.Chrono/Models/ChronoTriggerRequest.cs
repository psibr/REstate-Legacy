using System;

namespace REstate.Client.Chrono.Models
{
    public class ChronoTriggerRequest : IChronoTriggerRequest
    {
        public Guid ChronoTriggerId { get; set; }
        public Guid MachineInstanceId { get; set; }
        public string StateName { get; set; }
        public string TriggerName { get; set; }
        public string Payload { get; set; }
        public long Delay { get; set; }
    }
}