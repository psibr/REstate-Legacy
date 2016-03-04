namespace REstate.Configuration
{
    public class State 
        : IState
    {
        public int MachineDefinitionId { get; set; }

        public string StateName { get; set; }

        public string ParentStateName { get; set; }

        public string StateDescription { get; set; }
    }
}
