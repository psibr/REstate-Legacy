namespace REstateClient.Models
{
    public class AddChronoTriggerRequest
    {
        public string Payload { get; set; }

        public IChronoTriggerRequest ChronoTrigger { get; set; }
    }
}
