namespace REstate.Repositories
{
    public interface IConfigurationContextualRepository
    {
        IConfigurationRepository Root { get; }

        string ApiKey { get; }
    }

    public interface IInstanceContextualRepository
    {
        IInstanceRepository Root { get; }

        string ApiKey { get; }
    }
}