using System;

namespace REstate.Repositories
{
    public interface IInstanceRepository
        : IInstanceContextualRepository, IDisposable
    {
        IMachineInstancesRepository MachineInstances { get; }
    }
}