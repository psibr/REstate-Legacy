namespace REstate.Configuration
{
    public interface ITrigger
    {
        bool IsActive { get; set; }
        int MachineDefinitionId { get; set; }
        string TriggerDescription { get; set; }
        string TriggerName { get; set; }
    }
}