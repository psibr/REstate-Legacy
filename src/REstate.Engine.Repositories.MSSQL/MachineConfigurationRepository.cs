using REstate.Configuration;
using REstate.Engine.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Susanoo.SusanooCommander;

namespace REstate.Repositories.Core.Susanoo
{
    public class MachineConfigurationRepository
        : ContextualRepository, IMachineConfigurationRepository
    {
        private readonly StringSerializer _StringSerializer;

        public MachineConfigurationRepository(EngineRepositoryContext root, StringSerializer stringSerializer)
            : base(root)
        {
            _StringSerializer = stringSerializer;
        }

        public async Task<IEnumerable<MachineRecord>> ListMachines(CancellationToken cancellationToken)
        {
            return (await DefineCommand(
                    "SELECT MachineName, InitialState, CreatedDateTime " +
                    "FROM Machines;")
                .WithResultsAs<MachineRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken));
        }

        public async Task<Machine> RetrieveMachineConfiguration(string machineName, CancellationToken cancellationToken)
        {
            var machineRecord = (await DefineCommand(
                    "SELECT * FROM Machines WHERE MachineName = @MachineName;")
                .WithResultsAs<MachineRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { MachineName = machineName }, cancellationToken))
                .Single();

            var machine = _StringSerializer.Deserialize<Machine>(machineRecord.Definition);

            return machine;
        }

        public async Task<Machine> RetrieveMachineConfigurationForInstance(string instanceId, CancellationToken cancellationToken)
        {
            var machineRecord = (await DefineCommand(
                    "SELECT TOP 1 Machines.* " +
                    "FROM Machines " +
                    "INNER JOIN Instances ON Instances.MachineName = Machines.MachineName " +
                    "WHERE InstanceId = @InstanceId;")
                .WithResultsAs<MachineRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { InstanceId = instanceId }, cancellationToken))
                .Single();

            var machine = _StringSerializer.Deserialize<Machine>(machineRecord.Definition);

            return machine;
        }

        public async Task<Machine> DefineStateMachine(Machine machine, string forkedFrom, CancellationToken cancellationToken)
        {
            var definition = _StringSerializer.Serialize(machine);

            var results = await DefineCommand(
                    "INSERT INTO Machines (MachineName, ForkedFrom, InitialState, Definition) " +
                    "VALUES (@MachineName, @ForkedFrom, @InitialState, @Definition);" +
                    "\r\n\r\n" +
                    "SELECT * FROM Machines WHERE MachineName = @MachineName")
                .SendNullValues()
                .WithResultsAs<MachineRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    machine.MachineName,
                    ForkedFrom = forkedFrom,
                    machine.InitialState,
                    Definition = definition
                }, cancellationToken);

            var machineRecord = results.Single();

            var machineResult = _StringSerializer.Deserialize<Machine>(machineRecord.Definition);

            return machineResult;
        }

        public async Task<Machine> DefineStateMachine(Machine machine, CancellationToken cancellationToken)
        {
            return await DefineStateMachine(machine, null, cancellationToken);
        }
    }
}
