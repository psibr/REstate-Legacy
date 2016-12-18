using REstate.Logging;

namespace REstate.Engine.Repositories.InMemory
{
    public class RepositoryContextFactory
        : IRepositoryContextFactory
    {
        private readonly StringSerializer _stringSerializer;
        private readonly IPlatformLogger _logger;

        public RepositoryContextFactory(StringSerializer stringSerializer, IPlatformLogger logger)
        {
            _stringSerializer = stringSerializer;
            _logger = logger;
        }

        public IEngineRepositoryContext OpenContext(string apiKey)
        {
            return new EngineRepositoryContext(_stringSerializer, _logger, apiKey);
        }
    }
}
