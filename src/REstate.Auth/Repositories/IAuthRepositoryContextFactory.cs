namespace REstate.Auth.Repositories
{
    public interface IAuthRepositoryContextFactory
    {
        IAuthRepository OpenAuthRepositoryContext();
    }
}
