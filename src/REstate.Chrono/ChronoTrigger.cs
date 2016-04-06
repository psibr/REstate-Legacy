using System;

namespace REstate.Chrono
{
    public class ChronoTrigger 
        : IChronoTrigger
    {
        public Guid ChronoTriggerId { get; set; }
        public string MachineInstanceId { get; set; }
        public string StateName { get; set; }
        public string TriggerName { get; set; }
        public string Payload { get; set; }
        public long Delay { get; set; }
    }
}