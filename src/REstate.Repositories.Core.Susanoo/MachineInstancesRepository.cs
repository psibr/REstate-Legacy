using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Psibr.Platform.Serialization;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using Susanoo;

namespace REstate.Repositories.Core.Susanoo
{
    public class MachineInstancesRepository
        : ConfigurationContextualRepository, IMachineInstancesRepository
    {
        private readonly IStringSerializer _stringSerializer;

        public MachineInstancesRepository(ConfigurationRepository root, IStringSerializer stringSerializer)
            : base(root)
        {
            _stringSerializer = stringSerializer;
        }

        public async Task CreateInstance(string machineName, string instanceId, CancellationToken cancellationToken)
        {
            await CreateInstance(machineName, instanceId, null, cancellationToken);
        }

        public async Task CreateInstance(string machineName, string instanceId,
            IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var serializedMetadata = metadata == null ? null : _stringSerializer.SerializeToString(metadata);

            await CommandManager.Instance
                .DefineCommand(
                    @"
                    IF NOT EXISTS (SELECT TOP 1 InstanceId FROM Instances WHERE InstanceId = @InstanceId)
                    BEGIN

                        DECLARE @InitialState varchar(250);

                        SELECT TOP 1 @InitialState = InitialState FROM Machines WHERE MachineName = @MachineName;

                        INSERT INTO Instances (InstanceId, MachineName, StateName, Metadata)
                        VALUES (@InstanceId, @MachineName, @InitialState, @Metadata);
                    END", CommandType.Text)
                .SendNullValues()
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    MachineName = machineName,
                    Metadata = serializedMetadata
                }, null, cancellationToken);
        }

        public async Task DeleteInstance(string instanceId, CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand("DELETE FROM Instances WHERE InstanceId = @InstanceId", CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, null,
                    cancellationToken);
        }

        public InstanceRecord GetInstanceState(string instanceId)
        {
            return CommandManager.Instance
                .DefineCommand("SELECT TOP 1 * " +
                               "FROM Instances " +
                               "WHERE InstanceId = @InstanceId " +
                               "ORDER BY StateChangedDateTime DESC;", CommandType.Text)
                .DefineResults<InstanceRecord>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, CancellationToken.None)
                .Result
                .Single();
        }

        public void SetInstanceState(string instanceId, string stateName, string triggerName, string lastCommitTag)
        {
            SetInstanceState(instanceId, stateName, triggerName, null, lastCommitTag);
        }

        public void SetInstanceState(string instanceId, string stateName, string triggerName, string parameterData,
            string lastCommitTag)
        {

            var result = CommandManager.Instance
                .DefineCommand(
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

                    COMMIT
                    ", CommandType.Text)
                .SendNullValues()
                .Realize()
                .ExecuteScalarAsync<int>(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    StateName = stateName,
                    TriggerName = triggerName,
                    ParameterData = parameterData,
                    LastCommitTag = lastCommitTag
                }, null, CancellationToken.None).Result;

            if (result <= 0)
                throw new StateConflictException("State did not reflect original state when attempting to transition.");
        }

        public async Task<string> GetInstanceMetadata(string instanceId, CancellationToken cancellationToken)
        {
            var result = await CommandManager.Instance
                .DefineCommand("SELECT Metadata " +
                               "FROM Instances " +
                               "WHERE InstanceId = @InstanceId AND Metadata IS NOT NULL;", CommandType.Text)
                .Realize()
                .ExecuteScalarAsync<string>(DatabaseManagerPool.DatabaseManager,
                new
                {
                    InstanceId = instanceId
                }, cancellationToken);

            return result;
        }
    }
}