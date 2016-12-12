using REstate.Configuration;
using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine
{
    public class StateEngine
    {
        private readonly IStateMachineFactory _stateMachineFactory;
        private readonly IRepositoryContextFactory _repositoryContextFactory;

        private readonly StringSerializer _stringSerializer;

        private readonly string _apiKey;

        public StateEngine(
            IStateMachineFactory stateMachineFactory,
            IRepositoryContextFactory repositoryContextFactory,
            StringSerializer stringSerializer,
            string apiKey)
        {
            _stateMachineFactory = stateMachineFactory;
            _repositoryContextFactory = repositoryContextFactory;

            _stringSerializer = stringSerializer;

            _apiKey = apiKey;
        }

        public async Task<Machine> GetMachineDefinition(string machineDefinitionId, CancellationToken cancellationToken)
        {
            Machine configuration;
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                configuration = await repository.Machines
                    .RetrieveMachineConfiguration(machineDefinitionId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return configuration;
        }

        public async Task<string> GetDiagramForDefinition(string machineDefinitionId, CancellationToken cancellationToken)
        {
            Machine configuration;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                configuration = await repository.Machines
                    .RetrieveMachineConfiguration(machineDefinitionId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(_apiKey, null, configuration);

            return machine.ToString();
        }

        public string PreviewDiagram(Machine machineDefinition)
        {
            var machine = _stateMachineFactory
                .ConstructFromConfiguration(_apiKey, null, machineDefinition);

            return machine.ToString();
        }

        public async Task<Machine> DefineStateMachine(Machine machineDefinition, CancellationToken cancellationToken)
        {
            Machine newMachineConfiguration;
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                newMachineConfiguration = await repository.Machines
                    .DefineStateMachine(machineDefinition, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newMachineConfiguration;
        }

        public async Task<IEnumerable<MachineRecord>> ListMachines(CancellationToken cancellationToken)
        {
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                return await repository.Machines
                    .ListMachines(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<string> InstantiateMachine(string machineDefinitionId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineInstanceId = Guid.NewGuid().ToString();

            using (var configruationRepository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                await configruationRepository.MachineInstances
                    .CreateInstance(machineDefinitionId, machineInstanceId, metadata, cancellationToken)
                    .ConfigureAwait(false);
            }

            return machineInstanceId;
        }

        public async Task<IDictionary<string, string>> GetInstanceMetadata(string machineInstanceId, CancellationToken cancellationToken)
        {
            string metadata = await GetInstanceMetadataRaw(machineInstanceId, cancellationToken);

            return _stringSerializer.Deserialize<Dictionary<string, string>>(metadata);
        }

        public async Task<string> GetInstanceMetadataRaw(string machineInstanceId, CancellationToken cancellationToken)
        {
            string metadata;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                metadata = await repository.MachineInstances
                    .GetInstanceMetadata(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return metadata;
        }

        public async Task<IStateMachine> GetInstance(string machineInstanceId, CancellationToken cancellationToken)
        {
            Machine configuration;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                //TODO: Andrea doesn't like this. Review.
                configuration = await repository.Machines.RetrieveMachineConfigurationForInstance(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(_apiKey,
                    machineInstanceId,
                    configuration);

            return machine;
        }

        public async Task DeleteInstance(string machineInstanceId, CancellationToken cancellationToken)
        {
            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                await repository.MachineInstances
                    .DeleteInstance(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<InstanceRecord> GetInstanceInfoAsync(string machineInstanceId, CancellationToken cancellationToken)
        {
            InstanceRecord instanceInfo;

            using (var repository = _repositoryContextFactory
                .OpenContext(_apiKey))
            {
                instanceInfo = await repository.MachineInstances
                    .GetInstanceState(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return instanceInfo;
        }
    }
}
