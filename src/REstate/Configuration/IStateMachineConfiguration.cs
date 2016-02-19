using System.Collections.Generic;

namespace REstate.Configuration
{
    public interface IStateMachineConfiguration
    {
        ICollection<IGuard> Guards { get; set; }
        IMachineDefinition MachineDefinition { get; set; }
        ICollection<IStateConfiguration> StateConfigurations { get; set; }
        ICollection<ITrigger> Triggers { get; set; }
        ICollection<ICodeWithDatabaseConfiguration> CodeElements { get; set; }
    }
}