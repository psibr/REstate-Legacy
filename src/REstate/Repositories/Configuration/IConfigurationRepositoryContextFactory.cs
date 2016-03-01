namespace REstate.Repositories.Configuration
{
    public interface IConfigurationRepositoryContextFactory
    {
        IConfigurationRepository OpenConfigurationRepositoryContext(string apiKey);
    }
}
