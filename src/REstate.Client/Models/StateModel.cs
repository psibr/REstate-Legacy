namespace REstate.Client.Models
{
    internal class StateModel
    {
        public int MachineDefinitionId { get; set; }
        public string StateName { get; set; }

        public static implicit operator REstate.State(StateModel model)
        {
            return new REstate.State(model.MachineDefinitionId, model.StateName);
        }
    }
}