namespace REstate.Repositories.Instances
{
    public interface IInstanceContextualRepository
    {
        IInstanceRepository Root { get; }

        string ApiKey { get; }
    }
}