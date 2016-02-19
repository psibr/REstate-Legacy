using System;
using Susanoo;

namespace REstate.Susanoo
{
    public interface IDatabaseManagerPool
        : IDisposable
    {
        IDatabaseManager DatabaseManager { get; }
    }
}