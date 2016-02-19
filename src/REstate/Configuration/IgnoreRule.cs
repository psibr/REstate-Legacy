namespace REstate.Configuration
{
    public class IgnoreRule : IIgnoreRule
    {
        public IgnoreRule()
        {
            IsActive = true;
        }

        public int MachineDefinitionId { get; set; }

        public string StateName { get; set; }

        public string TriggerName { get; set; }

        public bool IsActive { get; set; }
    }
}