namespace REstate.Configuration
{
    public interface IStateAction
    {
        int MachineDefinitionId { get; set; }
        string StateName { get; set; }
        string PurposeName { get; set; }
        string TriggerName { get; set; }
        string StateActionDescription { get; set; }
        int? CodeElementId { get; set; }
    }
}