using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Stateless
{
    public class StatelessStateMachineAdapter
        : IStateMachine
    {
        private readonly StateMachine<State, Trigger> _stateMachine;

        public StatelessStateMachineAdapter(StateMachine<State, Trigger> stateMachine, string machineDefinitionId, string machineInstanceId)
        {
            _stateMachine = stateMachine;
            MachineInstanceId = machineInstanceId;
            MachineDefinitionId = machineDefinitionId;
        }

        public string MachineInstanceId { get; }
        public string MachineDefinitionId { get; }

        public void Fire(Trigger trigger)
        {
            Fire(trigger, null);
        }

        public void Fire(Trigger trigger, string payload)
        {
            _stateMachine.Fire(new StateMachine<State, Trigger>.TriggerWithParameters<string>(trigger), payload);
        }

        public bool IsInState(State state)
        {
            return _stateMachine.IsInState(state);
        }

        public State GetCurrentState()
        {
            return _stateMachine.State;
        }

        public ICollection<Trigger> PermittedTriggers =>
            _stateMachine.PermittedTriggers.ToList();

        public override string ToString()
        {
            return _stateMachine.ToDotGraph();
        }
    }
}