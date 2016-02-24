using System;

namespace REstate.Repositories
{
    public interface IConfigurationRepository
        : IConfigurationContextualRepository, IDisposable
    {
        IMachineConfigurationRepository Machines { get; }

        ICodeConfigurationRepository Code { get; }
    }
}