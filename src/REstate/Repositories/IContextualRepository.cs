namespace REstate.Repositories
{
    public interface IContextualRepository
    {
        IRepository Root { get; }

        string ApiKey { get; }
    }
}