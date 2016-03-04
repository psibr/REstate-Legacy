using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Susanoo;
using Susanoo.ConnectionPooling;
using Susanoo.Processing;

namespace REstate.Chrono.Susanoo
{
    public class ChronoEngine
        : IChronoEngine
    {
        private readonly ISingleResultSetCommandProcessor<dynamic, ChronoTrigger> _command = CommandManager.Instance
                .DefineCommand("SELECT * FROM ChronoTriggers WHERE FireAfter <= GETUTCDATE()", CommandType.Text)
                .DefineResults<ChronoTrigger>()
                .Realize();

        private readonly IDatabaseManagerPool _databaseManagerPool;

        public ChronoEngine(IDatabaseManagerPool databaseManagerPool)
        {
            _databaseManagerPool = databaseManagerPool;
        }

        public IEnumerable<IChronoTrigger> GetChronoStream(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var results = _command.Execute(_databaseManagerPool.DatabaseManager)
                    .ToList();

                if (!results.Any())
                {
                    Thread.Sleep(5000);
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
                .DefineCommand<IChronoTrigger>(
                    "INSERT INTO ChronoTriggers (MachineDefinitionId, MachineInstanceId, StateName, TriggerName, Payload, FireAfter) \n" +
                    "VALUES(@MachineDefinitionId, @MachineInstanceId, @StateName, @TriggerName, @Payload, @FireAfter)",
                    CommandType.Text)
                .ExcludeProperty(o => o.Delay)
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

        ~ChronoEngine()
        {
            Dispose(false);
        }
    }
}
