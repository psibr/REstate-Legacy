namespace REstate.Client.Models
{
    internal class TriggerModel
    {
        public string MachineDefinitionId { get; set; }
        public string TriggerName { get; set; }

        public static implicit operator REstate.Trigger(TriggerModel model)
        {
            return new REstate.Trigger(model.MachineDefinitionId, model.TriggerName);
        }
    }
}
