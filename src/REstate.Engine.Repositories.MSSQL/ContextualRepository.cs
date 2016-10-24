using REstate.Engine.Repositories;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Core.Susanoo
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
