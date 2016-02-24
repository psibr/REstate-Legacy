namespace REstate.Repositories
{
    public interface IInstanceRepositoryContextFactory
    {

        IInstanceRepository OpenInstanceRepositoryContext(string apiKey);
    }
}