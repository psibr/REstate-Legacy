using System;

namespace Susanoo.ConnectionPooling
{
    public interface IDatabaseManagerPool
        : IDisposable
    {
        IDatabaseManager DatabaseManager { get; }
    }
}