using REstate.Repositories;

namespace REstate.Susanoo
{
    public abstract class REstateContextualRepository
        : IContextualRepository
    {
        protected REstateContextualRepository(Repository root)
        {
            Root = root;
        }

        public string ApiKey
            => Root.ApiKey;

        IRepository IContextualRepository.Root 
            => this.Root;

        public Repository Root { get; }

        public virtual IDatabaseManagerPool DatabaseManagerPool
            => Root.DatabaseManagerPool;
    }
}
