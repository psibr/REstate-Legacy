namespace REstate.Web.Requests
{
    public class TriggerFireRequest
    {
        public string MachineInstanceId { get; set; }

        public string TriggerName { get; set; }

        public string Payload { get; set; }
    }
}
