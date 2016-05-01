using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using Susanoo;

namespace REstate.Repositories.Core.Susanoo
{
    public class MachineInstancesRepository
        : ConfigurationContextualRepository, IMachineInstancesRepository
    {
        public MachineInstancesRepository(ConfigurationRepository root)
            : base(root)
        {
        }

        public async Task EnsureInstanceExists(string machineName, string instanceId, CancellationToken cancellationToken)
        {
            await CommandManager.Instance
                .DefineCommand(
@"
IF NOT EXISTS (SELECT TOP 1 InstanceId FROM Instances WHERE InstanceId = @InstanceId)
BEGIN

    DECLARE @InitialState varchar(250);

    SELECT TOP 1 @InitialState = InitialState FROM Machines WHERE MachineName = @MachineName;

    INSERT INTO Instances (InstanceId, MachineName, StateName)
    VALUES (@InstanceId, @MachineName, @InitialState);
END", CommandType.Text)
                .Realize()
                .ExecuteNonQueryAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId,
                    MachineName = machineName
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

        public void SetInstanceState(string instanceId, string stateName, string lastCommitTag)
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
    INSERT INTO Instances (InstanceId, MachineName, StateName)
    VALUES (@InstanceId, @MachineName, @StateName);

    SELECT Success = 1;
END
ELSE
    SELECT Success = 0;

COMMIT
", CommandType.Text)
                .Realize()
                .ExecuteScalarAsync<int>(DatabaseManagerPool.DatabaseManager, new
                {
                    InstanceId = instanceId, StateName = stateName, LastCommitTag = lastCommitTag
                }, null, CancellationToken.None).Result;

            if(result <= 0)
                throw new StateConflictException("State did not reflect original state when attempting to transition.");
        }
    }
}