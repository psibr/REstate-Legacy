namespace REstate.Configuration
{
    public class MachineDefinition : IMachineDefinition
    {
        public int MachineDefinitionId { get; set; }

        public string MachineName { get; set; }

        public string MachineDescription { get; set; }

        public string InitialStateName{ get; set; }

        public bool AutoIgnoreNotConfiguredTriggers { get; set; }

        public bool IsActive { get; set; }
    }
}