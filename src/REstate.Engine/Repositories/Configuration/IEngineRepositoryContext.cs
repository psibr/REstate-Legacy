using System;

namespace REstate.Engine.Repositories
{
    public interface IEngineRepositoryContext
        : IContextualRepository, IDisposable
    {
        ISchematicRepository Schematics { get; }

        IMachineRepository Machines { get; }
    }
}
