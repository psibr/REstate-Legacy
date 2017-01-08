using REstate.Engine.Repositories;
using REstate.Logging;

namespace REstate.Engine
{
    public class StateEngineFactory
    {
        private readonly IStateMachineFactory _StateMachineFactory;
        private readonly IRepositoryContextFactory _RepositoryContextFactory;

        private readonly StringSerializer _StringSerializer;

        private readonly IPlatformLogger _Logger;

        public StateEngineFactory(
            IStateMachineFactory stateMachineFactory,
            IRepositoryContextFactory repositoryContextFactory,
            StringSerializer stringSerializer,
            IPlatformLogger logger)
        {
            _StateMachineFactory = stateMachineFactory;
            _RepositoryContextFactory = repositoryContextFactory;

            _StringSerializer = stringSerializer;

            _Logger = logger;
        }

        public StateEngine GetStateEngine(string apiKey)
        {
            return new StateEngine(
                _StateMachineFactory,
                _RepositoryContextFactory,
                _StringSerializer,
                _Logger,
                apiKey);
        }
    }
}
