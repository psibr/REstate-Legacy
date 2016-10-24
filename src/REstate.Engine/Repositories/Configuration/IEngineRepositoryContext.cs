using System;

namespace REstate.Engine.Repositories
{
    public interface IEngineRepositoryContext
        : IContextualRepository, IDisposable
    {
        IMachineConfigurationRepository Machines { get; }

        IMachineInstancesRepository MachineInstances { get; }
    }
}
