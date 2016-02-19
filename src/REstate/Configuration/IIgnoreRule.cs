namespace REstate.Configuration
{
    public interface IIgnoreRule
    {
        bool IsActive { get; set; }
        int MachineDefinitionId { get; set; }
        string StateName { get; set; }
        string TriggerName { get; set; }
    }
}