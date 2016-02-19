namespace REstate.Configuration
{
    public interface IGuard
    {
        int GuardId { get; set; }

        string GuardName { get; set; }

        string GuardDescription { get; set; }

        int? CodeElementId { get; set; }
    }
}