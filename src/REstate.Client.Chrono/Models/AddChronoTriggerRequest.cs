namespace REstate.Client.Chrono.Models
{
    public class AddChronoTriggerRequest
    {
        public string Payload { get; set; }

        public IChronoTriggerRequest ChronoTrigger { get; set; }
    }
}
