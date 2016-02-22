namespace REstate.Configuration
{
    public interface IIgnoreRule : IMachineDefinitionDependent
    {
        bool IsActive { get; set; }
        string StateName { get; set; }
        string TriggerName { get; set; }
    }
}