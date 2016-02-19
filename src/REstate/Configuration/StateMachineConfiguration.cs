using System.Collections.Generic;

namespace REstate.Configuration
{
    public class StateMachineConfiguration : IStateMachineConfiguration
    {
        public IMachineDefinition MachineDefinition { get; set; }

        public ICollection<IStateConfiguration> StateConfigurations { get; set; }

        public ICollection<ITrigger> Triggers { get; set; }
        public ICollection<ICodeWithDatabaseConfiguration> CodeElements { get; set; }

        public ICollection<IGuard> Guards { get; set; }
    }
}