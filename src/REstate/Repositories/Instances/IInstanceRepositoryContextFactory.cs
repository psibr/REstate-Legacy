namespace REstate.Repositories.Instances
{
    public interface IInstanceRepositoryContextFactory
    {

        IInstanceRepository OpenInstanceRepositoryContext(string apiKey);
    }
}