using System;

namespace REstate.Repositories.Instances
{
    public interface IInstanceRepository
        : IInstanceContextualRepository, IDisposable
    {
        IMachineInstancesRepository MachineInstances { get; }
    }
}