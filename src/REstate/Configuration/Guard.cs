namespace REstate.Configuration
{
    public class Guard 
        : IGuard
    {
        public string MachineDefinitionId { get; set; }

        public string GuardName { get; set; }

        public string GuardDescription { get; set; }

        public int? CodeElementId { get; set; }
    }
}