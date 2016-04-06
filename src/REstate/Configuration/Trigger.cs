namespace REstate.Configuration
{
    public class Trigger : ITrigger
    {
        public Trigger()
        {
            IsActive = true;
        }

        public string MachineDefinitionId { get; set; }

        public string TriggerName { get; set; }

        public string TriggerDescription { get; set; }

        public bool IsActive { get; set; }
    }
}