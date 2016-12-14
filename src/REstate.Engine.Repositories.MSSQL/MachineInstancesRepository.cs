using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Logging;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IPlatformLogger _logger;

        public MachineInstancesRepository(EngineRepositoryContext root, StringSerializer stringSerializer, IPlatformLogger logger)
            : base(root)
        {
            _stringSerializer = stringSerializer;
            _logger = logger;
        }

        public Task CreateInstanceAsync(string machineName, string instanceId, CancellationToken cancellationToken)
        {
            return CreateInstanceAsync(machineName, instanceId, null, cancellationToken);
        }

        public Task CreateInstanceAsync(string machineName, string instanceId,
            IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var serializedMetadata = metadata == null ? null : _stringSerializer.Serialize(metadata);
            
            return DefineCommand(
                    @"
                    DECLARE @InitialState varchar(250);
                    DECLARE @StateChangedDateTime DATETIME2(3);
                    DECLARE @CommitTag varchar(250);
                    SELECT TOP 1 @InitialState = InitialState FROM Machines WHERE MachineName = @MachineName;

                    SELECT @StateChangedDateTime = GETUTCDATE(), @CommitTag = NEWID()

                    INSERT INTO Instances (InstanceId, MachineName, StateName, CommitTag, StateChangedDateTime, Metadata)
                    VALUES (@InstanceId, @MachineName, @InitialState, @CommitTag, @StateChangedDateTime, @Metadata);")
                .SendNullValues()
                .Compile()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    MachineName = machineName,
                    Metadata = serializedMetadata
                }, null, cancellationToken);
        }

        public Task DeleteInstanceAsync(string instanceId, CancellationToken cancellationToken)
        {
            return DefineCommand("DELETE FROM Instances WHERE InstanceId = @InstanceId")
                .Compile()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, null,
                    cancellationToken);
        }

        public async Task<InstanceRecord> GetInstanceStateAsync(string instanceId, CancellationToken cancellationToken)
        {
            return (await DefineCommand(
                    "SELECT TOP 1 * " +
                    "FROM Instances " +
                    "WHERE InstanceId = @InstanceId; ")
                .WithResultsAs<InstanceRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, cancellationToken)
                .ConfigureAwait(false))
                .Single();
        }

        public Task<InstanceRecord> SetInstanceStateAsync(string instanceId, string stateName, string triggerName, string lastCommitTag, CancellationToken cancellationToken)
        {
            return SetInstanceStateAsync(instanceId, stateName, triggerName, null, lastCommitTag, cancellationToken);
        }

        public async Task<InstanceRecord> SetInstanceStateAsync(string instanceId, string stateName, string triggerName, string parameterData,
            string lastCommitTag, CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            sw.Start();
            var result = await DefineCommand(
                    @"                    
                    BEGIN TRANSACTION
                    
                    BEGIN TRY
                    
                        DECLARE @StateChangedDateTime DATETIME2(3);
                        DECLARE @CommitTag varchar(250);

                        DECLARE @result int;
                        EXEC @result = sp_getapplock @Resource = @InstanceId, @LockMode = 'Exclusive'

                        IF (@result = -3)
                        BEGIN
                            RAISERROR('Somehow a deadlock occured?!',16,1)
                            ROLLBACK TRANSACTION;
                        END

                        DECLARE @MachineName varchar(250);
                        DECLARE @ActualCommitTag varchar(250);

                        SELECT TOP 1 @MachineName = MachineName, @ActualCommitTag = CommitTag
                        FROM Instances
                        WHERE InstanceId = @InstanceId;

                        IF(@ActualCommitTag = @LastCommitTag)
                        BEGIN
                            SELECT @StateChangedDateTime = GETUTCDATE(), @CommitTag = NEWID()

                            UPDATE Instances SET StateName = @StateName, CommitTag = @CommitTag, StateChangedDateTime = @StateChangedDateTime WHERE InstanceId = @InstanceId

                            SELECT InstanceId = @InstanceId, MachineName = @MachineName, StateName = @StateName, CommitTag = @CommitTag, StateChangedDateTime = @StateChangedDateTime, TriggerName = @TriggerName, ParameterData = @ParameterData;
                        END

                        COMMIT
                    END TRY
                    BEGIN CATCH
                        ROLLBACK TRANSACTION;
                    END CATCH")
                .SendNullValues()
                .WithResultsAs<InstanceRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    StateName = stateName,
                    TriggerName = triggerName,
                    ParameterData = parameterData,
                    LastCommitTag = lastCommitTag
                }, null).ConfigureAwait(false);

            sw.Stop();

            _logger.Verbose("Took {elapsed}ms", sw.ElapsedMilliseconds);

            if (!result.Any())
                throw new StateConflictException("State did not reflect original state when attempting to transition.");

            return result.Single();
        }

        public Task<string> GetInstanceMetadataAsync(string instanceId, CancellationToken cancellationToken)
        {
            return DefineCommand(
                    "SELECT Metadata " +
                    "FROM Instances " +
                    "WHERE InstanceId = @InstanceId;")
                .Compile()
                .ExecuteScalarAsync<string>(DatabaseManagerPool.DatabaseManager,
                new
                {
                    InstanceId = instanceId
                }, cancellationToken);
        }
    }
}
