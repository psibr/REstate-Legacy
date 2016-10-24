namespace REstate.Configuration
{
    public class Transition
    {
        public string TriggerName { get; set; }
        public string ResultantStateName { get; set; }
        public Code Guard { get; set; }
    }
}
