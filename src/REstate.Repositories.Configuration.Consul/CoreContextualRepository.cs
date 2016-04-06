using Consul;
using Psibr.Platform;
using Psibr.Platform.Serialization;
using REstate.Repositories.Configuration;

namespace REstate.Repositories.Core.Consul
{
    public abstract class CoreContextualRepository
        : IConfigurationContextualRepository
    {
        protected CoreContextualRepository(CoreRepository root)
        {
            Root = root;
        }

        public string ApiKey
            => Root.ApiKey;

        IConfigurationRepository IConfigurationContextualRepository.Root
            => Root;

        public CoreRepository Root { get; }

        protected virtual ConsulClient Client =>
            Root.Client;

        protected IByteSerializer Serializer =>
            Root.Serializer;

        protected IPlatformConfiguration PlatformConfiguration =>
            Root.PlatformConfiguration;

        protected string REstateRootPath => 
            Root.REstateRootPath;

        protected string MachinesPath =>
            REstateRootPath + "/services/core/machines";

        protected string InstancesPath =>
            REstateRootPath + "/services/core/instances";
    }
}