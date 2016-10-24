namespace REstate.Configuration
{
    public class Machine
    {
        public string MachineName { get; set; }
        public string InitialState { get; set; }
        public bool AutoIgnoreTriggers { get; set; }
        public StateConfiguration[] StateConfigurations { get; set; }
    }
}
