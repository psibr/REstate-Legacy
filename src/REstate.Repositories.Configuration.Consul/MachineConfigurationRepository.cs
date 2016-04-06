using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using REstate.Configuration;
using REstate.Repositories.Configuration;

namespace REstate.Repositories.Core.Consul
{
    public class MachineConfigurationRepository
        : CoreContextualRepository, IMachineConfigurationRepository
    {

        public MachineConfigurationRepository(CoreRepository parent)
            : base(parent)
        {
        }

        public async Task<IStateMachineConfiguration> RetrieveMachineConfiguration(string machineDefinitionId, bool loadCode, CancellationToken cancellationToken)
        {
            var result = await Client.KV.Get($"{MachinesPath}/{machineDefinitionId}");

            if (result.StatusCode != HttpStatusCode.OK) return null;

            var configuration = Serializer.Deserialize<StateMachineConfiguration>(result.Response.Value);

            return configuration;
        }

        public async Task<IStateMachineConfiguration> RetrieveMachineConfigurationForInstance(string machineInstanceId, bool loadCode, CancellationToken cancellationToken)
        {
            var result = await Client.KV.Get($"{InstancesPath}/{machineInstanceId}");

            if (result.StatusCode != HttpStatusCode.OK) return null;

            var instance = Serializer.Deserialize<MachineInstanceModel>(result.Response.Value);

            var configuration = await RetrieveMachineConfiguration(instance.MachineDefinitionId, loadCode, cancellationToken);

            return configuration;
        }

        public async Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration stateMachineConfiguration, string previousVersionId,
            CancellationToken cancellationToken)
        {
            var bytes = Serializer.SerializeToBytes(stateMachineConfiguration);

            var result = await Client.KV.Put(
                new KVPair($"{MachinesPath}/{stateMachineConfiguration.MachineDefinition.MachineDefinitionId}")
                {
                    Value = bytes
                });

            if (result.StatusCode != HttpStatusCode.OK)
                throw new Exception(); //TODO: Strong Exception for write failures.

            return await RetrieveMachineConfiguration(
                stateMachineConfiguration.MachineDefinition.MachineDefinitionId, true, cancellationToken);
        }

        public async Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration stateMachineConfiguration, CancellationToken cancellationToken)
        {
            return await DefineStateMachine(stateMachineConfiguration, null, cancellationToken);
        }
    }
}
