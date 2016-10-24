namespace REstateClient.Models
{
    internal class TriggerModel
    {
        public string MachineName { get; set; }
        public string TriggerName { get; set; }

        public static implicit operator REstate.Trigger(TriggerModel model)
        {
            return new REstate.Trigger(model.MachineName, model.TriggerName);
        }
    }
}
