using System;

namespace REstate.Scheduling
{
    public interface IChronoTrigger
    {
        Guid ChronoTriggerId { get; set; }

        string MachineInstanceId { get; set; }

        string StateName { get; set; }

        string TriggerName { get; set; }

        string Payload { get; set; }

        string LastCommitTag { get; set; }

        long Delay { get; set; }
    }
}
