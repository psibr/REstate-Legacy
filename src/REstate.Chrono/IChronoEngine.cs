using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Chrono
{
    public interface IChronoEngine
        : IDisposable
    {
        IEnumerable<IChronoTrigger> GetChronoStream(CancellationToken cancellationToken);

        Task AddChronoTrigger(IChronoTrigger trigger, CancellationToken cancellationToken);

        Task RemoveChronoTrigger(IChronoTrigger trigger, CancellationToken cancellationToken);
    }
}