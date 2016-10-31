using System;
using System.Collections.Generic;

namespace REstate.Scheduling
{
    public class ChronoTrigger
    {
        public ChronoTrigger()
        {

        }

        public ChronoTrigger(IDictionary<string, string> configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string machineInstanceId;
            string triggerName;
            string delayString;
            long delay;
            string verifyCommitTagString;
            bool verifyCommitTag;
            string payload;

            if (configuration.TryGetValue("triggerName", out triggerName))
                TriggerName = triggerName;
            else
                throw new ArgumentException("Configuration did not contain TriggerName.", nameof(configuration));

            if (configuration.TryGetValue("delay", out delayString) && long.TryParse(delayString, out delay))
                Delay = delay;
            else
                throw new ArgumentException("Configuration did not contain Delay.", nameof(configuration));

            if (configuration.TryGetValue("machineInstanceId", out machineInstanceId))
                MachineInstanceId = machineInstanceId;

            if (configuration.TryGetValue("payload", out payload))
                Payload = payload;

            if (configuration.TryGetValue("verifyCommitTag", out verifyCommitTagString) && bool.TryParse(verifyCommitTagString, out verifyCommitTag))
                VerifyCommitTag = verifyCommitTag;
        }

        public Guid ChronoTriggerId { get; set; }

        public string MachineInstanceId { get; set; }

        public string StateName { get; set; }

        public string TriggerName { get; set; }

        public string Payload { get; set; }

        public long Delay { get; set; }

        public string LastCommitTag { get; set; }

        public bool VerifyCommitTag { get; set; } = true;
    }
}
