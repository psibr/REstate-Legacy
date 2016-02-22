namespace REstate.Configuration
{
    public interface ITransition : IMachineDefinitionDependent
    {
        bool IsActive { get; set; }
        string GuardName { get; set; }
        string ResultantStateName { get; set; }
        string StateName { get; set; }
        string TriggerName { get; set; }
    }
}