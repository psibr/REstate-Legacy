using REstate.Configuration;
using REstate.Engine.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Susanoo.SusanooCommander;

namespace REstate.Repositories.Core.Susanoo
{
    public class MachineInstancesRepository
        : ContextualRepository, IMachineInstancesRepository
    {
        private readonly StringSerializer _stringSerializer;

        public MachineInstancesRepository(EngineRepositoryContext root, StringSerializer stringSerializer)
            : base(root)
        {
            _stringSerializer = stringSerializer;
        }

        public async Task CreateInstance(string machineName, string instanceId, CancellationToken cancellationToken)
        {
            await CreateInstance(machineName, instanceId, null, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task CreateInstance(string machineName, string instanceId,
            IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var serializedMetadata = metadata == null ? null : _stringSerializer.Serialize(metadata);

            await DefineCommand(
                    @"
                    IF NOT EXISTS (SELECT TOP 1 InstanceId FROM Instances WHERE InstanceId = @InstanceId)
                    BEGIN

                        DECLARE @InitialState varchar(250);

                        SELECT TOP 1 @InitialState = InitialState FROM Machines WHERE MachineName = @MachineName;

                        INSERT INTO Instances (InstanceId, MachineName, StateName, Metadata)
                        VALUES (@InstanceId, @MachineName, @InitialState, @Metadata);
                    END")
                .SendNullValues()
                .Compile()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    MachineName = machineName,
                    Metadata = serializedMetadata
                }, null, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task DeleteInstance(string instanceId, CancellationToken cancellationToken)
        {
            await DefineCommand("DELETE FROM Instances WHERE InstanceId = @InstanceId")
                .Compile()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, null,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<InstanceRecord> GetInstanceState(string instanceId, CancellationToken cancellationToken)
        {
            return (await DefineCommand(
                    "SELECT TOP 1 * " +
                    "FROM Instances " +
                    "WHERE InstanceId = @InstanceId " +
                    "ORDER BY StateChangedDateTime DESC;")
                .WithResultsAs<InstanceRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, cancellationToken)
                .ConfigureAwait(false))
                .Single();
        }

        public async Task SetInstanceState(string instanceId, string stateName, string triggerName, string lastCommitTag, CancellationToken cancellationToken)
        {
            await SetInstanceState(instanceId, stateName, triggerName, null, lastCommitTag, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task SetInstanceState(string instanceId, string stateName, string triggerName, string parameterData,
            string lastCommitTag, CancellationToken cancellationToken)
        {
            var result = await DefineCommand(
                    @"
                    BEGIN TRANSACTION

                    EXEC sp_getapplock @Resource = @InstanceId, @LockMode = 'Update'

                    DECLARE @MachineName varchar(250);
                    DECLARE @CommitTag varchar(250);

                    SELECT TOP 1 @MachineName = MachineName, @CommitTag = CommitTag
                    FROM Instances
                    WHERE InstanceId = @InstanceId
                    ORDER BY StateChangedDateTime DESC

                    IF(@CommitTag = @LastCommitTag)
                    BEGIN
                        INSERT INTO Instances (InstanceId, MachineName, StateName, TriggerName, ParameterData)
                        VALUES (@InstanceId, @MachineName, @StateName, @TriggerName, @ParameterData);

                        SELECT Success = 1;
                    END
                    ELSE
                        SELECT Success = 0;

                    COMMIT")
                .SendNullValues()
                .Compile()
                .ExecuteScalarAsync<int>(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    StateName = stateName,
                    TriggerName = triggerName,
                    ParameterData = parameterData,
                    LastCommitTag = lastCommitTag
                }, null, cancellationToken)
                .ConfigureAwait(false);

            if (result <= 0)
                throw new StateConflictException("State did not reflect original state when attempting to transition.");
        }

        public async Task<string> GetInstanceMetadata(string instanceId, CancellationToken cancellationToken)
        {
            var result = await DefineCommand(
                    "SELECT Metadata " +
                    "FROM Instances " +
                    "WHERE InstanceId = @InstanceId AND Metadata IS NOT NULL;")
                .Compile()
                .ExecuteScalarAsync<string>(DatabaseManagerPool.DatabaseManager,
                new
                {
                    InstanceId = instanceId
                }, cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
    }
}
