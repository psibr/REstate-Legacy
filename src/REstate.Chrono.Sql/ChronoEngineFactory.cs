using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Chrono.Susanoo
{
    public class ChronoEngineFactory : IChronoEngineFactory
    {
        public IChronoEngine CreateEngine()
        {
            return new ChronoEngine(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")));
        }
    }
}
