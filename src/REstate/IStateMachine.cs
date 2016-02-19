using System.Collections.Generic;

namespace REstate
{
    public interface IStateMachine
    {
        void Fire(Trigger trigger);

        void Fire(Trigger trigger, string payload);

        bool IsInState(State state);

        State GetCurrentState();

        ICollection<Trigger> PermittedTriggers { get; }

        string ToString();
    }
}
