namespace Platform.Repositories
{
    public interface IAuthRepositoryContextFactory
    {
        IAuthRepository OpenAuthRepositoryContext();
    }
}
