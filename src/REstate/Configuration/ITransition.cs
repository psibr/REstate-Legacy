namespace REstate.Configuration
{
    public interface ITransition
    {
        bool IsActive { get; set; }
        int? GuardId { get; set; }
        int MachineDefinitionId { get; set; }
        string ResultantStateName { get; set; }
        string StateName { get; set; }
        string TriggerName { get; set; }
    }
}