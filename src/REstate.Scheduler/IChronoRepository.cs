using REstate.Scheduling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Scheduler
{
    public interface IChronoRepository
        : IDisposable
    {
        IEnumerable<ChronoTrigger> GetChronoStream(CancellationToken cancellationToken);

        Task AddChronoTrigger(ChronoTrigger trigger, CancellationToken cancellationToken);

        Task RemoveChronoTrigger(ChronoTrigger trigger, CancellationToken cancellationToken);
    }
}
