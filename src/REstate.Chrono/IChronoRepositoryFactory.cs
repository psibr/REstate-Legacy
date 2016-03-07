namespace REstate.Chrono
{
    public interface IChronoRepositoryFactory
    {
        IChronoRepository OpenRepository();
    }
}