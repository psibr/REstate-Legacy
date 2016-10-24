using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine
{
    public class StateEngine
    {
        private readonly IStateMachineFactory _StateMachineFactory;
        private readonly IRepositoryContextFactory _RepositoryContextFactory;

        private readonly StringSerializer _StringSerializer;

        private readonly string _ApiKey;

        public StateEngine(
            IStateMachineFactory stateMachineFactory,
            IRepositoryContextFactory repositoryContextFactory,
            StringSerializer stringSerializer,
            string apiKey)
        {
            _StateMachineFactory = stateMachineFactory;
            _RepositoryContextFactory = repositoryContextFactory;

            _StringSerializer = stringSerializer;

            _ApiKey = apiKey;
        }

        public async Task<Machine> GetMachineDefinition(string machineDefinitionId, CancellationToken cancellationToken)
        {
            Machine configuration;
            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
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

            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
            {
                configuration = await repository.Machines
                    .RetrieveMachineConfiguration(machineDefinitionId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _StateMachineFactory
                .ConstructFromConfiguration(_ApiKey, configuration);

            return machine.ToString();
        }

        public string PreviewDiagram(Machine machineDefinition)
        {
            var machine = _StateMachineFactory
                .ConstructFromConfiguration(_ApiKey,
                    machineDefinition);

            return machine.ToString();
        }

        public async Task<Machine> DefineStateMachine(Machine machineDefinition, CancellationToken cancellationToken)
        {
            Machine newMachineConfiguration;
            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
            {
                newMachineConfiguration = await repository.Machines
                    .DefineStateMachine(machineDefinition, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newMachineConfiguration;
        }

        public async Task<IEnumerable<MachineRecord>> ListMachines(CancellationToken cancellationToken)
        {
            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
            {
                return await repository.Machines
                    .ListMachines(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<string> InstantiateMachine(string machineDefinitionId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineInstanceId = Guid.NewGuid().ToString();

            using (var configruationRepository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
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

            return _StringSerializer.Deserialize<Dictionary<string, string>>(metadata);
        }

        public async Task<string> GetInstanceMetadataRaw(string machineInstanceId, CancellationToken cancellationToken)
        {
            string metadata;

            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
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

            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
            {
                //TODO: Andrea doesn't like this. Review.
                configuration = await repository.Machines.RetrieveMachineConfigurationForInstance(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _StateMachineFactory
                .ConstructFromConfiguration(_ApiKey,
                    machineInstanceId,
                    configuration);

            return machine;
        }

        public async Task DeleteInstance(string machineInstanceId, CancellationToken cancellationToken)
        {
            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
            {
                await repository.MachineInstances
                    .DeleteInstance(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<InstanceRecord> GetInstanceInfo(string machineInstanceId, CancellationToken cancellationToken)
        {
            InstanceRecord instanceInfo;

            using (var repository = _RepositoryContextFactory
                .OpenContext(_ApiKey))
            {
                instanceInfo = await repository.MachineInstances
                    .GetInstanceState(machineInstanceId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return instanceInfo;
        }
    }
}
