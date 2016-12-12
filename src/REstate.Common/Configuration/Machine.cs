namespace REstate.Configuration
{
    public class Machine
    {
        public string MachineName { get; set; }
        public string InitialState { get; set; }
        public StateConfiguration[] StateConfigurations { get; set; }
        public ServiceState[] ServiceStates { get; set; }
    }
}
