using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using REstate.Configuration;
using REstate.Repositories.Instances;
using Susanoo;
using Susanoo.Processing;
using Susanoo.Proxies.Transforms;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace REstate.Repositories.Core.Susanoo
{
    public class MachineInstancesRepository
        : InstanceContextualRepository, IMachineInstancesRepository
    {
        private static readonly ISingleResultSetCommandProcessor<dynamic,REstate.Configuration.State> GetStateCommandProcessor = 
            CommandManager.Instance
                .DefineCommand("SELECT MachineDefinitionId, StateName FROM MachineInstances WHERE MachineInstanceId = @MachineInstanceId",
                    CommandType.Text)
                .DefineResults<REstate.Configuration.State>()
                .Realize();

        public MachineInstancesRepository(InstanceRepository root)
            : base(root)
        {
        }

        public async Task EnsureInstanceExists(IStateMachineConfiguration configuration, string machineInstanceId,
            CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand("IF NOT EXISTS(SELECT TOP 1 MachineInstances.MachineInstanceId FROM MachineInstances WHERE MachineInstanceId = @MachineInstanceId)" +
                               "\n\tINSERT INTO MachineInstances VALUES(@machineInstanceId, @MachineDefinitionId, @InitialStateName); ", CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    machineInstanceId,
                    configuration.MachineDefinition.MachineDefinitionId,
                    configuration.MachineDefinition.InitialStateName
                }, null, cancellationToken);
        }

        public async Task DeleteInstance(string machineInstanceId, CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand("DELETE FROM MachineInstances WHERE MachineInstanceId = @machineInstanceId",
                    CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { machineInstanceId }, null,
                    cancellationToken);
        }

        public State GetInstanceState(string machineInstanceId)
        {
            var results = GetStateCommandProcessor
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { MachineInstanceId = machineInstanceId },
                    CancellationToken.None).Result;

            var result = results
                .SingleOrDefault();

            return result == null 
                ? null 
                : new State(result.MachineDefinitionId, result.StateName);
        }

        public void SetInstanceState(string machineInstanceId, State state, State lastState)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var manager = DatabaseManagerPool.DatabaseManager;

            using (var transaction = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                manager.OpenConnection();
                var results = CommandManager.Instance
                    .DefineCommand(
                        "EXEC sp_getapplock @Resource = @machineInstanceId, @LockMode='Update', @LockTimeout='2000'" +
                        "\n\n" +
                        "UPDATE MachineInstances SET StateName = @StateName \n" +
                        "WHERE MachineInstanceId = @machineInstanceId AND StateName = @LastStateName;" +
                        "\n\n" +
                        "EXEC sp_releaseapplock @Resource = @machineInstanceId",
                        CommandType.Text)
                    .Realize()
                    .SetTimeout(TimeSpan.FromSeconds(3))
                    .ExecuteNonQuery(manager,
                        new { machineInstanceId, state.StateName, LastStateName = lastState.StateName });

                if(results <= 0)
                    throw new StateConflictException("State did not reflect original state when attempting to transition.");


                transaction.Complete();
            }
            manager.CloseConnection();
        }
    }
}