using Susanoo.ConnectionPooling;

namespace REstate.Engine.Repositories.MSSQL
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

        public virtual IDatabaseManagerPool DatabaseManagerPool
            => Root.DatabaseManagerPool;
    }
}
