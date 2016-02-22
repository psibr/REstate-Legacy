namespace REstate.Configuration
{
    public interface ITrigger : IMachineDefinitionDependent
    {
        bool IsActive { get; set; }
        string TriggerDescription { get; set; }
        string TriggerName { get; set; }
    }
}