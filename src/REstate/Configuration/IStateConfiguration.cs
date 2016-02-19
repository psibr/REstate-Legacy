using System.Collections.Generic;

namespace REstate.Configuration
{
    public interface IStateConfiguration
    {
        ICollection<IIgnoreRule> IgnoreRules { get; set; }
        IStateAction OnEntryAction { get; set; }

        IStateAction OnEntryFromAction { get; set; }

        IStateAction OnExitAction { get; set; }
        IState State { get; set; }
        ICollection<ITransition> Transitions { get; set; }
    }
}