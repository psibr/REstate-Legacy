using System;

namespace REstateClient.Models
{
    public interface IChronoTriggerRequest
    {
        Guid ChronoTriggerId { get; set; }

        string MachineInstanceId { get; set; }

        string StateName { get; set; }

        string TriggerName { get; set; }

        string Payload { get; set; }
        long Delay { get; set; }
    }
}
