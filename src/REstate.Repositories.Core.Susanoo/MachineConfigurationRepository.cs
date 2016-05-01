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
    public class MachineConfigurationRepository
        : ConfigurationContextualRepository, IMachineConfigurationRepository
    {
        public IStringSerializer StringSerilaizer { get; set; }

        public MachineConfigurationRepository(ConfigurationRepository root, IStringSerializer stringSerilaizer)
            : base(root)
        {
            StringSerilaizer = stringSerilaizer;
        }

        public async Task<ICollection<MachineRecord>> ListMachines(CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand("SELECT MachineName, InitialState, AutoIgnoreTriggers, CreatedDateTime " +
                               "FROM Machines;",
                    CommandType.Text)
                .DefineResults<MachineRecord>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken)).ToList();
        }

        public async Task<Machine> RetrieveMachineConfiguration(string machineName, CancellationToken cancellationToken)
        {
            var machineRecord = (await CommandManager.Instance
                .DefineCommand("SELECT * FROM Machines WHERE MachineName = @MachineName;", CommandType.Text)
                .DefineResults<MachineRecord>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new {MachineName = machineName}, cancellationToken))
                .Single();

            var machine = StringSerilaizer.Deserialize<Machine>(machineRecord.Definition);

            return machine;
        }

        public async Task<Machine> RetrieveMachineConfigurationForInstance(string instanceId, CancellationToken cancellationToken)
        {
            var machineRecord = (await CommandManager.Instance
                .DefineCommand("SELECT TOP 1 Machines.* " +
                               "FROM Machines " +
                               "INNER JOIN Instances ON Instances.MachineName = Machines.MachineName " +
                               "WHERE InstanceId = @InstanceId;", CommandType.Text)
                .DefineResults<MachineRecord>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, cancellationToken))
                .Single();

            var machine = StringSerilaizer.Deserialize<Machine>(machineRecord.Definition);

            return machine;
        }

        public async Task<Machine> DefineStateMachine(Machine machine, string forkedFrom, CancellationToken cancellationToken)
        {
            var definition = StringSerilaizer.SerializeToString(machine);

            var results = await CommandManager.Instance
                .DefineCommand("INSERT INTO Machines (MachineName, ForkedFrom, InitialState, AutoIgnoreTriggers, Definition) " +
                               "VALUES (@MachineName, @ForkedFrom, @InitialState, @AutoIgnoreTriggers, @Definition);" +
                               "\r\n\r\n" +
                               "SELECT * FROM Machines WHERE MachineName = @MachineName", CommandType.Text)
                .SendNullValues()
                .DefineResults<MachineRecord>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    machine.MachineName,
                    ForkedFrom = forkedFrom,
                    machine.InitialState,
                    machine.AutoIgnoreTriggers,
                    Definition = definition
                }, cancellationToken);

            var machineRecord = results.Single();

            var machineResult = StringSerilaizer.Deserialize<Machine>(machineRecord.Definition);

            return machineResult;
        }

        public async Task<Machine> DefineStateMachine(Machine machine, CancellationToken cancellationToken)
        {
            return await DefineStateMachine(machine, null, cancellationToken);
        }

    }
}