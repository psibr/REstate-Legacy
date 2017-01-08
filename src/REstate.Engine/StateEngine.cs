using REstate.Configuration;
using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Logging;

namespace REstate.Engine
{
    public class StateEngine
    {
        private readonly IStateMachineFactory _stateMachineFactory;
        private readonly IRepositoryContextFactory _repositoryContextFactory;

        private readonly StringSerializer _stringSerializer;

        private readonly IPlatformLogger _Logger;

        private readonly string _apiKey;

        public StateEngine(
            IStateMachineFactory stateMachineFactory,
            IRepositoryContextFactory repositoryContextFactory,
            StringSerializer stringSerializer,
            IPlatformLogger logger,
            string apiKey)
        {
            _stateMachineFactory = stateMachineFactory;
            _repositoryContextFactory = repositoryContextFactory;

            _stringSerializer = stringSerializer;

            _Logger = logger;

            _apiKey = apiKey;
        }

        public async Task<Schematic> GetSchematic(string schematicName, CancellationToken cancellationToken)
        {
            Schematic configuration;
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                configuration = await repository.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken)
                    .ConfigureAwait(false);
            }

            return configuration;
        }

        public async Task<string> GetSchematicDiagram(string schematicName, CancellationToken cancellationToken)
        {
            Schematic schematic;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                schematic = await repository.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(_apiKey, null, schematic);

            return machine.ToString();
        }

        public string PreviewDiagram(Schematic schematic)
        {
            var machine = _stateMachineFactory
                .ConstructFromConfiguration(_apiKey, null, schematic);

            return machine.ToString();
        }

        public async Task<Schematic> CreateSchematic(Schematic schematic, CancellationToken cancellationToken)
        {
            Schematic newSchematic;
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                newSchematic = await repository.Schematics
                    .CreateSchematicAsync(schematic, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newSchematic;
        }

        public async Task<IEnumerable<Schematic>> ListSchematics(CancellationToken cancellationToken)
        {
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                return await repository.Schematics
                    .ListSchematicsAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<string> InstantiateMachine(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineId = Guid.NewGuid().ToString();

            using (var configruationRepository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                await configruationRepository.Machines
                    .CreateMachineAsync(schematicName, machineId, metadata, cancellationToken)
                    .ConfigureAwait(false);
            }

            return machineId;
        }

        public async Task<IDictionary<string, string>> GetMachineMetadata(string machineId, CancellationToken cancellationToken)
        {
            string metadata = await GetMachineMetadataRaw(machineId, cancellationToken);

            return _stringSerializer.Deserialize<Dictionary<string, string>>(metadata);
        }

        public async Task<string> GetMachineMetadataRaw(string machineId, CancellationToken cancellationToken)
        {
            string metadata;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                metadata = await repository.Machines
                    .GetMachineMetadataAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return metadata;
        }

        public async Task<IStateMachine> GetMachine(string machineId, CancellationToken cancellationToken)
        {
            Schematic schematic;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                //TODO: Andrea doesn't like this. Review.
                schematic = await repository.Schematics
                    .RetrieveSchematicForMachineAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(_apiKey,
                    machineId,
                    schematic);

            return machine;
        }

        public async Task DeleteMachine(string machineId, CancellationToken cancellationToken)
        {
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                await repository.Machines
                    .DeleteMachineAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<InstanceRecord> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken)
        {
            InstanceRecord machineInfo;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                machineInfo = await repository.Machines
                    .GetMachineStateAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return machineInfo;
        }
    }
}
