using REstate.Configuration;
using REstate.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Susanoo.SusanooCommander;
using System;

namespace REstate.Engine.Repositories.MSSQL
{
    public class MachineRepository
        : ContextualRepository, IMachineRepository
    {
        private readonly StringSerializer _stringSerializer;
        private readonly IPlatformLogger _logger;

        public MachineRepository(EngineRepositoryContext root, StringSerializer stringSerializer, IPlatformLogger logger)
            : base(root)
        {
            _stringSerializer = stringSerializer;
            _logger = logger;
        }

        public Task CreateMachineAsync(string schematicName, string machineId, CancellationToken cancellationToken)
        {
            return CreateMachineAsync(schematicName, machineId, null, cancellationToken);
        }

        public Task CreateMachineAsync(string schematicName, string machineId,
            IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var serializedMetadata = metadata == null ? null : _stringSerializer.Serialize(metadata);
            
            return DefineCommand(
                    @"
                    DECLARE @InitialState varchar(250);
                    DECLARE @StateChangedDateTime DATETIME2(3);
                    DECLARE @CommitTag varchar(250);
                    SELECT TOP 1 @InitialState = InitialState FROM Schematics WHERE SchematicName = @SchematicName;

                    SELECT @StateChangedDateTime = GETUTCDATE(), @CommitTag = NEWID()

                    INSERT INTO Machines (MachineId, SchematicName, StateName, CommitTag, StateChangedDateTime, Metadata)
                    VALUES (@MachineId, @SchematicName, @InitialState, @CommitTag, @StateChangedDateTime, @Metadata);")
                .SendNullValues()
                .Compile()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    MachineId = machineId,
                    SchematicName = schematicName,
                    Metadata = serializedMetadata
                }, null, cancellationToken);
        }

        public Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            return DefineCommand("DELETE FROM Machines WHERE MachineId = @MachineId")
                .Compile()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new { MachineId = machineId }, null,
                    cancellationToken);
        }

        public async Task<InstanceRecord> GetMachineStateAsync(string machineId, CancellationToken cancellationToken)
        {
            return (await DefineCommand(
                    "SELECT TOP 1 * " +
                    "FROM Machines " +
                    "WHERE MachineId = @MachineId; ")
                .WithResultsAs<InstanceRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { MachineId = machineId }, cancellationToken)
                .ConfigureAwait(false))
                .Single();
        }

        public Task<InstanceRecord> SetMachineStateAsync(string machineId, string stateName, string triggerName, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            return SetMachineStateAsync(machineId, stateName, triggerName, null, lastCommitTag, cancellationToken);
        }

        public async Task<InstanceRecord> SetMachineStateAsync(string machineId, string stateName, string triggerName, string parameterData,
            Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            var result = await DefineCommand(
                    @"                    
                    BEGIN TRANSACTION
                    
                    BEGIN TRY
                    
                        DECLARE @StateChangedDateTime DATETIME2(3);
                        DECLARE @CommitTag varchar(250);

                        DECLARE @result int;
                        EXEC @result = sp_getapplock @Resource = @MachineId, @LockMode = 'Exclusive'

                        IF (@result = -3)
                        BEGIN
                            RAISERROR('Somehow a deadlock occured?!',16,1)
                            ROLLBACK TRANSACTION;
                        END

                        DECLARE @SchematicName varchar(250);
                        DECLARE @ActualCommitTag varchar(250);

                        SELECT TOP 1 @SchematicName = SchematicName, @ActualCommitTag = CommitTag
                        FROM Machines
                        WHERE MachineId = @MachineId;

                        IF(@ActualCommitTag = @LastCommitTag)
                        BEGIN
                            SELECT @StateChangedDateTime = GETUTCDATE(), @CommitTag = NEWID()

                            UPDATE Machines SET StateName = @StateName, CommitTag = @CommitTag, StateChangedDateTime = @StateChangedDateTime WHERE MachineId = @MachineId

                            SELECT MachineId = @MachineId, SchematicName = @SchematicName, StateName = @StateName, CommitTag = @CommitTag, StateChangedDateTime = @StateChangedDateTime, TriggerName = @TriggerName, ParameterData = @ParameterData;
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
                    MachineId = machineId,
                    StateName = stateName,
                    TriggerName = triggerName,
                    ParameterData = parameterData,
                    LastCommitTag = lastCommitTag
                }, null).ConfigureAwait(false);

            if (!result.Any())
                throw new StateConflictException("State did not reflect original state when attempting to transition.");

            return result.Single();
        }

        public Task<string> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken)
        {
            return DefineCommand(
                    "SELECT Metadata " +
                    "FROM Machines " +
                    "WHERE MachineId = @MachineId;")
                .Compile()
                .ExecuteScalarAsync<string>(DatabaseManagerPool.DatabaseManager,
                new
                {
                    MachineId = machineId
                }, cancellationToken);
        }
    }
}
