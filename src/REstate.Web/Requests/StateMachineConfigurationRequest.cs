using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;

namespace REstate.Web.Requests
{
    public class StateMachineConfigurationRequest
    {
        public MachineDefinition MachineDefinition { get; set; }

        public ICollection<StateConfiguration> StateConfigurations { get; set; }

        public ICollection<Configuration.Trigger> Triggers { get; set; }
        public ICollection<CodeWithDatabaseConfiguration> CodeElements { get; set; }

        public ICollection<Guard> Guards { get; set; }

        public static implicit operator StateMachineConfiguration(StateMachineConfigurationRequest request)
        {
            return new StateMachineConfiguration
            {
                MachineDefinition = request.MachineDefinition,
                Triggers = request.Triggers.Cast<ITrigger>().ToList(),
                Guards = request.Guards.Cast<IGuard>().ToList(),
                StateConfigurations = request.StateConfigurations.Select(sc => new Configuration.StateConfiguration
                {
                    State = sc.State,
                    OnEntryAction = sc.OnEntryAction,
                    OnEntryFromAction = sc.OnEntryFromAction,
                    OnExitAction = sc.OnExitAction,
                    Transitions = sc.Transitions.Cast<ITransition>().ToList(),
                    IgnoreRules = sc.IgnoreRules.Cast<IIgnoreRule>().ToList()
                }).Cast<IStateConfiguration>().ToList()
            };


        }
    }
}