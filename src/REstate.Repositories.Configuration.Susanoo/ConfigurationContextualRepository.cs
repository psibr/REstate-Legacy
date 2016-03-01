using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Configuration.Susanoo
{
    public abstract class ConfigurationContextualRepository
        : IConfigurationContextualRepository
    {
        protected ConfigurationContextualRepository(ConfigurationRepository root)
        {
            Root = root;
        }

        public string ApiKey
            => Root.ApiKey;

        IConfigurationRepository IConfigurationContextualRepository.Root 
            => this.Root;

        public ConfigurationRepository Root { get; }

        public virtual IDatabaseManagerPool DatabaseManagerPool
            => Root.DatabaseManagerPool;
    }
}
