namespace REstate.Repositories
{
    public interface IRepositoryContextFactory
    {
        IRepository OpenRepositoryContext(string apiKey);
    }
}
