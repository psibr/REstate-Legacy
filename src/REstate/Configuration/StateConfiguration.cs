using System.Collections.Generic;

namespace REstate.Configuration
{
    public class StateConfiguration 
        : IStateConfiguration
    {
        public IState State { get; set; }

        public ICollection<ITransition> Transitions { get; set; }

        public ICollection<IIgnoreRule> IgnoreRules { get; set; }

        public IStateAction OnEntryAction { get; set; }

        public IStateAction OnEntryFromAction { get;set;}

        public IStateAction OnExitAction { get; set; }
    }
}
