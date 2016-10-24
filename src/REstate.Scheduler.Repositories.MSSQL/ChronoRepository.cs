using REstate.Scheduling;
using Susanoo.ConnectionPooling;
using Susanoo.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Susanoo.SusanooCommander;

namespace REstate.Scheduler.Repositories.MSSQL
{
    public class ChronoRepository
        : IChronoRepository
    {
        private readonly ISingleResultSetCommandProcessor<dynamic, ChronoTrigger> _Command =
            DefineCommand("SELECT * FROM ChronoTriggers WITH (NOLOCK) WHERE FireAfter <= GETUTCDATE()")
                .WithResultsAs<ChronoTrigger>()
                .Compile();

        private readonly IDatabaseManagerPool _databaseManagerPool;

        protected string ApiKey { get; }

        public ChronoRepository(IDatabaseManagerPool databaseManagerPool, string apiKey)
        {
            _databaseManagerPool = databaseManagerPool;

            ApiKey = apiKey;
        }

        public IEnumerable<IChronoTrigger> GetChronoStream(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var results = _Command.Execute(_databaseManagerPool.DatabaseManager)
                    .ToList();

                if (!results.Any())
                {
                    Task.Delay(100);
                }

                foreach (var chronoTrigger in results)
                {
                    yield return chronoTrigger;
                }
            }
        }

        public async Task AddChronoTrigger(IChronoTrigger trigger, CancellationToken cancellationToken)
        {
            await DefineCommand<IChronoTrigger>(
                    @"INSERT INTO ChronoTriggers ( ChronoTriggerId, MachineInstanceId, StateName, TriggerName, Payload, FireAfter)
                    VALUES(newId(), @MachineInstanceId, @StateName, @TriggerName, @Payload, @FireAfter)")
                .ExcludeProperty(o => o.Delay)
                .ExcludeProperty(o => o.ChronoTriggerId)
                .SendNullValues()
                .Compile()
                .ExecuteNonQueryAsync(_databaseManagerPool.DatabaseManager, trigger,
                    new { FireAfter = DateTime.UtcNow + TimeSpan.FromSeconds(trigger.Delay) }, cancellationToken);
        }

        public async Task RemoveChronoTrigger(IChronoTrigger trigger, CancellationToken cancellationToken)
        {
            await DefineCommand<IChronoTrigger>("DELETE FROM ChronoTriggers WHERE ChronoTriggerId = @ChronoTriggerId")
                .UseExplicitPropertyInclusionMode()
                .IncludeProperty(o => o.ChronoTriggerId)
                .Compile()
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
