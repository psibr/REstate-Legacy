using REstate.Engine.Repositories;

namespace REstate.Engine
{
    public class StateEngineFactory
    {
        private readonly IStateMachineFactory _StateMachineFactory;
        private readonly IRepositoryContextFactory _RepositoryContextFactory;

        private readonly StringSerializer _StringSerializer;

        public StateEngineFactory(
            IStateMachineFactory stateMachineFactory,
            IRepositoryContextFactory repositoryContextFactory,
            StringSerializer stringSerializer)
        {
            _StateMachineFactory = stateMachineFactory;
            _RepositoryContextFactory = repositoryContextFactory;

            _StringSerializer = stringSerializer;
        }

        public StateEngine GetStateEngine(string apiKey)
        {
            return new StateEngine(
                _StateMachineFactory,
                _RepositoryContextFactory,
                _StringSerializer,
                apiKey);
        }
    }
}
