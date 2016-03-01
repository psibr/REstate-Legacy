using System;

namespace REstate.Repositories.Configuration
{
    public interface IConfigurationRepository
        : IConfigurationContextualRepository, IDisposable
    {
        IMachineConfigurationRepository Machines { get; }

        ICodeConfigurationRepository Code { get; }
    }
}