namespace REstate.Configuration
{
    public interface IState
    {
        int MachineDefinitionId { get; set; }
        string ParentStateName { get; set; }
        string StateDescription { get; set; }
        string StateName { get; set; }
    }
}