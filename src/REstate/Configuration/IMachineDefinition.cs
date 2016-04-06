namespace REstate.Configuration
{
    public interface IMachineDefinition
    {
        bool AutoIgnoreNotConfiguredTriggers { get; set; }
        string InitialStateName { get; set; }
        bool IsActive { get; set; }
        string MachineDefinitionId { get; set; }
        string MachineDescription { get; set; }
        string MachineName { get; set; }
    }
}