using System;

namespace REstate.Repositories
{
    public interface IRepository
        : IContextualRepository, IDisposable
    {
        IConfigurationRepository Configuration { get; }

        IMachineFunctionsRepository MachineFunctions { get; }

        IMachineInstancesRepository MachineInstances { get; }
    }
}