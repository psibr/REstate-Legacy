namespace REstate.Configuration
{
    public class Guard 
        : IGuard
    {
        public int GuardId { get; set; }

        public string GuardName { get; set; }

        public string GuardDescription { get; set; }

        public int? CodeElementId { get; set; }
    }
}