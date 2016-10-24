namespace REstate.Scheduler
{
    public interface IChronoRepositoryFactory
    {
        IChronoRepository OpenRepository(string apiKey);
    }
}
