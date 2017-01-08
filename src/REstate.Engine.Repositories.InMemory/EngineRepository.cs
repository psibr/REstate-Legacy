using REstate.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepository: ISchematicRepository, IMachineRepository
    {
        private readonly StringSerializer _stringSerializer;

        private static IDictionary<string, Schematic> Schematics { get; } = new Dictionary<string, Schematic>();
        private static IDictionary<string, Tuple<InstanceRecord, IDictionary<string, string>>> Machines { get; } = new Dictionary<string, Tuple<InstanceRecord, IDictionary<string, string>>>();

        public EngineRepository(EngineRepositoryContext root, StringSerializer stringSerializer)
        {
            _stringSerializer = stringSerializer;
            Root = root;
        }

        public string ApiKey
            => Root.ApiKey;

        public EngineRepositoryContext Root { get; }

        public Task<IEnumerable<Schematic>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Schematics.Values as IEnumerable<Schematic>);
        }

        public Task<Schematic> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            var machineRecord = Schematics[schematicName];

            return Task.FromResult(machineRecord);
        }

        public Task<Schematic> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            Tuple<InstanceRecord, IDictionary<string, string>> instanceData;
            try
            {
                instanceData = Machines[machineId];
            }
            catch
            {
                throw new InstanceDoesNotExistException();
            }

            try
            {
                var machineRecord = Schematics[instanceData.Item1.SchematicName];

                return Task.FromResult(machineRecord);
            }
            catch
            {
                throw new DefinitionDoesNotExistException();
            }


        }

        public Task<Schematic> CreateSchematicAsync(Schematic schematic, string forkedFrom, CancellationToken cancellationToken)
        {

            Schematics.Add(schematic.SchematicName, schematic);

            var machineRecord = Schematics[schematic.SchematicName];

            return Task.FromResult(machineRecord);
        }

        public Task<Schematic> CreateSchematicAsync(Schematic schematic, CancellationToken cancellationToken)
        {
            return CreateSchematicAsync(schematic, null, cancellationToken);
        }

        public Task CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineRecord = Schematics[schematicName];

            Machines.Add(machineId, Tuple.Create(new InstanceRecord
            {
                SchematicName = schematicName,
                StateName = machineRecord.InitialState,
                CommitTag = Guid.NewGuid().ToString(),
                StateChangedDateTime = DateTime.UtcNow
            }, metadata));

            return Task.CompletedTask;
        }

        public Task CreateMachineAsync(string schematicName, string machineId, CancellationToken cancellationToken)
        {
            return CreateMachineAsync(schematicName, machineId, new Dictionary<string, string>(), cancellationToken);
        }

        public Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            Machines.Remove(machineId);

            return Task.CompletedTask;
        }

        public Task<InstanceRecord> GetMachineStateAsync(string machineId, CancellationToken cancellationToken)
        {
            var instanceRecord = Machines[machineId].Item1;

            return Task.FromResult(instanceRecord);
        }

        public Task<InstanceRecord> SetMachineStateAsync(string machineId, string stateName, string triggerName, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            return SetMachineStateAsync(machineId, stateName, triggerName, null, lastCommitTag, cancellationToken);
        }

        public Task<InstanceRecord> SetMachineStateAsync(string machineId, string stateName, string triggerName, string parameterData, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            var instance = Machines[machineId];

            lock(instance.Item1)
            {
                if (lastCommitTag == null || Guid.Parse(instance.Item1.CommitTag) == lastCommitTag)
                {
                    instance.Item1.StateName = stateName;
                    instance.Item1.CommitTag = Guid.NewGuid().ToString();
                }
                else
                {
                    throw new StateConflictException("CommitTag did not match.");
                }
            }

            return Task.FromResult(instance.Item1);
        }

        public Task<string> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken)
        {
            var metadata = Machines[machineId].Item2;

            return Task.FromResult(_stringSerializer.Serialize(metadata));
        }
    }
}
