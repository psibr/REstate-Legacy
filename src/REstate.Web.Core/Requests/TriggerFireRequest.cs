namespace REstate.Web.Core.Requests
{
    public class TriggerFireRequest
    {
        public string MachineInstanceId { get; set; }

        public string TriggerName { get; set; }

        public string Payload { get; set; }
    }
}
