namespace REstate.Repositories.Configuration
{
    public interface IConfigurationContextualRepository
    {
        IConfigurationRepository Root { get; }

        string ApiKey { get; }
    }
}