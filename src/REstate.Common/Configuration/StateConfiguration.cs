namespace REstate.Configuration
{
    public class StateConfiguration
    {
        public string StateName { get; set; }
        public string ParentStateName { get; set; }
        public string StateDescription { get; set; }
        public Transition[] Transitions { get; set; }
        public Code OnEntry { get; set; }
        public string[] IgnoreRules { get; set; }
        public Code OnExit { get; set; }
        public OnEntryFrom[] OnEntryFrom { get; set; }
    }
}
