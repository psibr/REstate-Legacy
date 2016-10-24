namespace REstate.Engine.Repositories
{
    public interface IContextualRepository
    {
        IEngineRepositoryContext Root { get; }

        string ApiKey { get; }
    }
}
