using REstate.Configuration;
using Susanoo;
using Susanoo.Processing;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace REstate.Repositories.Instances.Susanoo
{
    public class MachineInstancesRepository
        : InstanceContextualRepository, IMachineInstancesRepository
    {
        private static readonly ISingleResultSetCommandProcessor<dynamic,REstate.Configuration.State> GetStateCommandProcessor = 
            CommandManager.Instance
                .DefineCommand("dbo.GetInstanceState",
                    CommandType.StoredProcedure)
                .DefineResults<REstate.Configuration.State>()
                .Realize();

        public MachineInstancesRepository(InstanceRepository root)
            : base(root)
        {
        }

        public async Task EnsureInstanceExists(IStateMachineConfiguration configuration, Guid machineInstanceGuid,
            CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand("IF NOT EXISTS(SELECT TOP 1 MachineInstances.MachineInstanceId FROM MachineInstances WHERE MachineInstanceId = @MachineInstanceGuid)" +
                               "\n\tINSERT INTO MachineInstances VALUES(@machineInstanceGuid, @MachineDefinitionId, @InitialStateName); ", CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    machineInstanceGuid,
                    configuration.MachineDefinition.MachineDefinitionId,
                    configuration.MachineDefinition.InitialStateName
                }, null, cancellationToken);
        }

        public async Task DeleteInstance(Guid machineInstanceGuid, CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand("DELETE FROM MachineInstances WHERE MachineInstanceId = @machineInstanceGuid",
                    CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { machineInstanceGuid }, null,
                    cancellationToken);
        }

        public State GetInstanceState(Guid machineInstanceGuid)
        {
            var results = GetStateCommandProcessor
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { MachineInstanceGuid = machineInstanceGuid },
                    CancellationToken.None).Result;

            var result = results
                .SingleOrDefault();

            return result == null 
                ? null 
                : new State(result.MachineDefinitionId, result.StateName);
        }

        public void SetInstanceState(Guid machineInstanceGuid, State state, State lastState)
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
                        "EXEC sp_getapplock @Resource = @machineInstanceGuid, @LockMode='Update', @LockTimeout='2000'" +
                        "\n\n" +
                        "UPDATE MachineInstances SET StateName = @StateName \n" +
                        "WHERE MachineInstanceId = @machineInstanceGuid AND StateName = @LastStateName;" +
                        "\n\n" +
                        "EXEC sp_releaseapplock @Resource = @machineInstanceGuid",

                        CommandType.Text)
                    .Realize()
                    .SetTimeout(TimeSpan.FromSeconds(3))
                    .ExecuteNonQuery(manager,
                        new { machineInstanceGuid, state.StateName, LastStateName = lastState.StateName });

                if(results <= 0)
                    throw new StateConflictException("State did not reflect original state when attempting to transition.");


                transaction.Complete();
            }
            manager.CloseConnection();
        }
    }
}