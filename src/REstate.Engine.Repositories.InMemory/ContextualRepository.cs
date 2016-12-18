namespace REstate.Engine.Repositories.InMemory
{
    public abstract class ContextualRepository
        : IContextualRepository
    {
        protected ContextualRepository(EngineRepositoryContext root)
        {
            Root = root;
        }

        public string ApiKey
            => Root.ApiKey;

        IEngineRepositoryContext IContextualRepository.Root
            => this.Root;

        public EngineRepositoryContext Root { get; }
    }
}
