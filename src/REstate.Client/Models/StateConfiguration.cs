using REstate.Configuration;
using System.Collections.Generic;

namespace REstate.Client.Models
{
    public class StateConfiguration 
    {
        public Configuration.State State { get; set; }

        public ICollection<Transition> Transitions { get; set; }

        public ICollection<IgnoreRule> IgnoreRules { get; set; }

        public StateAction OnEntryAction { get; set; }

        public StateAction OnEntryFromAction { get;set;}

        public StateAction OnExitAction { get; set; }
    }
}
