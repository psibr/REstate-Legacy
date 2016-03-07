using System;
using REstate.Chrono;

namespace REstate.Client.Chrono.Models
{
    public class AddChronoTriggerRequest
    {
        public string Payload { get; set; }

        public IChronoTrigger ChronoTrigger { get; set; }
    }
}
