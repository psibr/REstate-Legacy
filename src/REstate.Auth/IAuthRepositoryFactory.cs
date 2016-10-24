namespace REstate.Auth
{
    public interface IAuthRepositoryFactory
    {
        IAuthRepository OpenRepository();
    }
}