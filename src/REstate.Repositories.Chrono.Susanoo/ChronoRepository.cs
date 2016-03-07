using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Chrono;
using Susanoo;
using Susanoo.ConnectionPooling;
using Susanoo.Processing;

namespace REstate.Repositories.Chrono.Susanoo
{
    public class ChronoRepository
        : IChronoRepository
    {
        private readonly ISingleResultSetCommandProcessor<dynamic, ChronoTrigger> _command = CommandManager.Instance
                .DefineCommand("SELECT * FROM ChronoTriggers WITH (NOLOCK) WHERE FireAfter <= GETUTCDATE()", CommandType.Text)
                .DefineResults<ChronoTrigger>()
                .Realize();

        private readonly IDatabaseManagerPool _databaseManagerPool;

        public ChronoRepository(IDatabaseManagerPool databaseManagerPool)
        {
            _databaseManagerPool = databaseManagerPool;
        }

        public IEnumerable<IChronoTrigger> GetChronoStream(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                List<ChronoTrigger> results = new List<ChronoTrigger>();
                try
                {
                    results = _command.Execute(_databaseManagerPool.DatabaseManager)
                        .ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }

                if (!results.Any())
                {
                    Thread.Sleep(1000);
                }

                foreach (var chronoTrigger in results)
                {
                    yield return chronoTrigger;
                }
            }
        }

        public async Task AddChronoTrigger(IChronoTrigger trigger, CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand<IChronoTrigger>("AddChronoTrigger", CommandType.StoredProcedure)
                .ExcludeProperty(o => o.Delay)
                .ExcludeProperty(o => o.ChronoTriggerId)
                .SendNullValues()
                .Realize()
                .ExecuteNonQueryAsync(_databaseManagerPool.DatabaseManager, trigger,
                    new { FireAfter = DateTime.UtcNow + TimeSpan.FromSeconds(trigger.Delay) }, cancellationToken);
        }

        public async Task RemoveChronoTrigger(IChronoTrigger trigger, CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand<IChronoTrigger>("DELETE FROM ChronoTriggers WHERE ChronoTriggerId = @ChronoTriggerId",
                    CommandType.Text)
                .UseExplicitPropertyInclusionMode()
                .IncludeProperty(o => o.ChronoTriggerId)
                .Realize()
                .ExecuteNonQueryAsync(_databaseManagerPool.DatabaseManager, trigger, null, cancellationToken);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _databaseManagerPool.Dispose();
            }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ~ChronoRepository()
        {
            Dispose(false);
        }
    }
}
