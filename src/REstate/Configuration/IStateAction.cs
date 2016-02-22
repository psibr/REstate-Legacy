namespace REstate.Configuration
{
    public interface IStateAction : IMachineDefinitionDependent
    {
        string StateName { get; set; }
        string PurposeName { get; set; }
        string TriggerName { get; set; }
        string StateActionDescription { get; set; }
        int? CodeElementId { get; set; }
    }
}