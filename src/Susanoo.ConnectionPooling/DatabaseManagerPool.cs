using System;
using System.Collections.Generic;
using System.Linq;

namespace Susanoo.ConnectionPooling
{
    public class DatabaseManagerPool
        : IDatabaseManagerPool
    {
        private readonly List<ManagerInstance> _databaseManagers = new List<ManagerInstance>();
        private readonly IDatabaseManagerFactory _databaseManagerFactory;
        private readonly Func<IDatabaseManagerFactory, IDatabaseManager> _builder;

        public DatabaseManagerPool(
            IDatabaseManagerFactory databaseManagerFactory,
            Func<IDatabaseManagerFactory, IDatabaseManager> builder,
            ContextDbConnectionMode connectionMode)
        {
            _databaseManagerFactory = databaseManagerFactory;
            _builder = builder;
            ConnectionMode = connectionMode;
        }

        public DatabaseManagerPool(
            IDatabaseManagerFactory databaseManagerFactory,
            Func<IDatabaseManagerFactory, IDatabaseManager> builder)
            : this(databaseManagerFactory, builder, ContextDbConnectionMode.Each)
        {
        }

        /// <summary>
        /// Gets the connection mode.
        /// </summary>
        /// <value>The connection mode.</value>
        protected ContextDbConnectionMode ConnectionMode { get; private set; }

        /// <summary>
        /// Adds a new database manager.
        /// </summary>
        /// <returns>DatabaseManager.</returns>
        private IDatabaseManager AddDatabaseManager()
        {
            IDatabaseManager result = null;

            if (result == null)
            {
                result = _builder(_databaseManagerFactory);
            }

            return result;
        }

        /// <summary>
        /// Gets the database manager.
        /// </summary>
        /// <value>The database manager.</value>
        public IDatabaseManager DatabaseManager
        {
            get
            {
                IDatabaseManager result = null;

                switch (ConnectionMode)
                {
                    case ContextDbConnectionMode.Shared:
                        if (_databaseManagers.Count == 0)
                            _databaseManagers.Add(new ManagerInstance { DatabaseManager = _builder(_databaseManagerFactory) });
                        result = _databaseManagers[0].DatabaseManager;
                        break;

                    case ContextDbConnectionMode.Each:
                        result = AddDatabaseManager();
                        break;
                }

                return result;
            }
        }

        private bool _disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="DatabaseManagerPool"/> class.
        /// </summary>
        ~DatabaseManagerPool()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes
        /// </summary>
        /// <param name="disposing">is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var container in _databaseManagers.Where(container => container?.DatabaseManager != null))
                        container.DatabaseManager.Dispose();
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Class ManagerInstance.
        /// </summary>
        protected class ManagerInstance
        {
            /// <summary>
            /// Gets or sets the database manager.
            /// </summary>
            /// <value>The database manager.</value>
            public IDatabaseManager DatabaseManager { get; set; }

            /// <summary>
            /// Gets or sets the transaction identifier.
            /// </summary>
            /// <value>The transaction identifier.</value>
            public string TransactionId { get; set; }
        }
    }
}
