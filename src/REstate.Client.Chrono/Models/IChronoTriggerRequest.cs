using System;

namespace REstate.Client.Chrono.Models
{
    public interface IChronoTriggerRequest
    {
        Guid ChronoTriggerId { get; set; }

        Guid MachineInstanceId { get; set; }

        string StateName { get; set; }

        string TriggerName { get; set; }

        string Payload { get; set; }
        long Delay { get; set; }
    }
}