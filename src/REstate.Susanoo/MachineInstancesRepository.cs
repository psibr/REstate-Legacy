using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories;
using Susanoo;

namespace REstate.Susanoo
{
    public class MachineInstancesRepository
        : InstanceContextualRepository, IMachineInstancesRepository
    {
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

        public async Task<State> GetInstanceState(Guid machineInstanceGuid, CancellationToken cancellationToken)
        {
            var results = await CommandManager.Instance
                .DefineCommand(
                    "SELECT TOP 1 MachineDefinitionId, StateName FROM MachineInstances WHERE MachineInstanceId = @machineInstanceGuid",
                    CommandType.Text)
                .DefineResults<Configuration.State>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { machineInstanceGuid }, cancellationToken);

            var result = results
                .SingleOrDefault();

            return result != null ? new State(result.MachineDefinitionId, result.StateName) : null;
        }

        public async Task SetInstanceState(Guid machineInstanceGuid, State state, CancellationToken cancellationToken)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            await CommandManager.Instance
                .DefineCommand(
                    "UPDATE MachineInstances SET StateName = @StateName WHERE MachineInstanceId = @machineInstanceGuid",
                    CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { machineInstanceGuid, state.StateName },
                    null, cancellationToken);
        }
    }
}