using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Machine
    {
        [Required]
        public string MachineName { get; set; }

        [Required]
        public string InitialState { get; set; }

        public StateConfiguration[] StateConfigurations { get; set; }
        
        public ServiceState[] ServiceStates { get; set; }
    }
}
