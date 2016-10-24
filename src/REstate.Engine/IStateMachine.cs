using System.Collections.Generic;

namespace REstate.Engine
{
    public interface IStateMachine
    {
        string MachineInstanceId { get; }

        string MachineDefinitionId { get; }

        void Fire(Trigger trigger);

        void Fire(Trigger trigger, string payload);

        bool IsInState(State state);

        State GetCurrentState();

        ICollection<Trigger> PermittedTriggers { get; }

        string ToString();
    }
}
