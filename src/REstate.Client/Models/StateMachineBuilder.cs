using System;
using System.Collections.Generic;
using REstate.Configuration;

namespace REstate.Client.Models
{
    internal class StateMachineBuilder
    {
        private readonly IMachineDefinition _definition;
        private readonly List<State> _states = new List<State>();
        private readonly List<Trigger> _triggers = new List<Trigger>();
        private readonly List<IGuard> _guards = new List<IGuard>();

        public StateMachineBuilder(string name, string description, string initialStateName, bool autoIgnoreNotConfiguredTriggers = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(initialStateName)) throw new ArgumentNullException(nameof(initialStateName));

            _definition = new MachineDefinition
            {
                AutoIgnoreNotConfiguredTriggers =  autoIgnoreNotConfiguredTriggers,
                InitialStateName = initialStateName,
                IsActive = true,
                MachineDescription = description,
                MachineName = name
            };
        }

        public StateMachineBuilder DefineStates(params State[] states)
        {
            _states.AddRange(states);
            return this;
        }

        public StateMachineBuilder DefineTriggers(params Trigger[] triggers)
        {
            _triggers.AddRange(triggers);
            return this;
        }

        public StateMachineBuilder DefineGuards(params Guard[] guards)
        {
            _guards.AddRange(guards);
            return this;
        }


    }
}
