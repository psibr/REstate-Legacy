using System;
using System.Collections.Generic;

namespace REstate
{
    public interface IStateMachine
    {
        Guid MachineInstanceId { get; }

        int MachineDefinitionId { get; }

        void Fire(Trigger trigger);

        void Fire(Trigger trigger, string payload);

        bool IsInState(State state);

        State GetCurrentState();

        ICollection<Trigger> PermittedTriggers { get; }

        string ToString();
    }
}
