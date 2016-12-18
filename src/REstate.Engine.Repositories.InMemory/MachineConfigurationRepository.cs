using REstate.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class MachineConfigurationRepository
        : ContextualRepository, IMachineConfigurationRepository, IMachineInstancesRepository
    {
        private readonly StringSerializer _stringSerializer;

        private static IDictionary<string, MachineRecord> Machines { get; } = new Dictionary<string, MachineRecord>();
        private static IDictionary<string, Tuple<InstanceRecord, IDictionary<string, string>>> Instances { get; } = new Dictionary<string, Tuple<InstanceRecord, IDictionary<string, string>>>();

        public MachineConfigurationRepository(EngineRepositoryContext root, StringSerializer stringSerializer)
            : base(root)
        {
            _stringSerializer = stringSerializer;
        }

        public Task<IEnumerable<MachineRecord>> ListMachinesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Machines.Values as IEnumerable<MachineRecord>);
        }

        public Task<Machine> RetrieveMachineConfigurationAsync(string machineName, CancellationToken cancellationToken)
        {
            var machineRecord = Machines[machineName];

            var machine = _stringSerializer.Deserialize<Machine>(machineRecord.Definition);

            return Task.FromResult(machine);
        }

        public Task<Machine> RetrieveMachineConfigurationForInstanceAsync(string instanceId, CancellationToken cancellationToken)
        {
            var instanceRecord = Instances[instanceId];

            var machineRecord = Machines[instanceRecord.Item1.MachineName];

            var machine = _stringSerializer.Deserialize<Machine>(machineRecord.Definition);

            return Task.FromResult(machine);
        }

        public Task<Machine> DefineStateMachineAsync(Machine machine, string forkedFrom, CancellationToken cancellationToken)
        {
            var definition = _stringSerializer.Serialize(machine);

            Machines.Add(machine.MachineName, new MachineRecord
            {
                MachineName = machine.MachineName,
                Definition = definition,
                CreatedDateTime = DateTime.UtcNow,
                ForkedFrom = forkedFrom,
                InitialState = machine.InitialState
            });

            var machineRecord = Machines[machine.MachineName];

            var machineResult = _stringSerializer.Deserialize<Machine>(machineRecord.Definition);

            return Task.FromResult(machineResult);
        }

        public Task<Machine> DefineStateMachineAsync(Machine machine, CancellationToken cancellationToken)
        {
            return DefineStateMachineAsync(machine, null, cancellationToken);
        }

        public Task CreateInstanceAsync(string machineName, string instanceId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineRecord = Machines[machineName];

            Instances.Add(instanceId, Tuple.Create(new InstanceRecord
            {
                MachineName = machineName,
                StateName = machineRecord.InitialState,
                CommitTag = Guid.NewGuid().ToString(),
                StateChangedDateTime = DateTime.UtcNow
            }, metadata));

            return Task.CompletedTask;
        }

        public Task CreateInstanceAsync(string machineName, string instanceId, CancellationToken cancellationToken)
        {
            return CreateInstanceAsync(machineName, instanceId, new Dictionary<string, string>(), cancellationToken);
        }

        public Task DeleteInstanceAsync(string instanceId, CancellationToken cancellationToken)
        {
            Instances.Remove(instanceId);

            return Task.CompletedTask;
        }

        public Task<InstanceRecord> GetInstanceStateAsync(string instanceId, CancellationToken cancellationToken)
        {
            var instanceRecord = Instances[instanceId].Item1;

            return Task.FromResult(instanceRecord);
        }

        public Task<InstanceRecord> SetInstanceStateAsync(string instanceId, string stateName, string triggerName, string lastCommitTag, CancellationToken cancellationToken)
        {
            return SetInstanceStateAsync(instanceId, stateName, triggerName, null, lastCommitTag, cancellationToken);
        }

        public Task<InstanceRecord> SetInstanceStateAsync(string instanceId, string stateName, string triggerName, string parameterData, string lastCommitTag, CancellationToken cancellationToken)
        {
            var instance = Instances[instanceId];

            lock(instance.Item1)
            {
                if (lastCommitTag == null || instance.Item1.CommitTag == lastCommitTag)
                {
                    instance.Item1.StateName = stateName;
                    instance.Item1.TriggerName = triggerName;
                    instance.Item1.Payload = parameterData;
                    instance.Item1.CommitTag = Guid.NewGuid().ToString();
                }
                else
                {
                    throw new ArgumentException("CommitTag did not match.", nameof(lastCommitTag));
                }
            }

            return Task.FromResult(instance.Item1);
        }

        public Task<string> GetInstanceMetadataAsync(string instanceId, CancellationToken cancellationToken)
        {
            var metadata = Instances[instanceId].Item2;

            return Task.FromResult(_stringSerializer.Serialize(metadata));
        }
    }
}
