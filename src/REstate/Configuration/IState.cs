namespace REstate.Configuration
{
    public interface IState : IMachineDefinitionDependent
    {
        string ParentStateName { get; set; }
        string StateDescription { get; set; }
        string StateName { get; set; }
    }
}