namespace REstate.Configuration
{
    public interface IGuard : IMachineDefinitionDependent
    {
        string GuardName { get; set; }

        string GuardDescription { get; set; }

        int? CodeElementId { get; set; }
    }
}