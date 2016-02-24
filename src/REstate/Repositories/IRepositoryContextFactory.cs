namespace REstate.Repositories
{
    public interface IConfigurationRepositoryContextFactory
    {
        IConfigurationRepository OpenConfigurationRepositoryContext(string apiKey);
    }
}
