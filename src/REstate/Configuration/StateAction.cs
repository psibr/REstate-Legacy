namespace REstate.Configuration
{
    public class StateAction
        : IStateAction
    {
        public string MachineDefinitionId { get; set; }
        public string StateName { get; set; }
        public string PurposeName { get; set; }
        public string TriggerName { get; set; }
        public string StateActionDescription { get; set; }
        public int? CodeElementId { get; set; }
    }
}