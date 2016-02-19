using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Susanoo;

namespace REstate.Susanoo
{
    public class DatabaseManagerPool
        : IDatabaseManagerPool
    {
        private readonly List<ManagerInstance> _databaseManagers = new List<ManagerInstance>();
        private readonly List<WeakReference<IDatabaseManager>> _weakReferences = new List<WeakReference<IDatabaseManager>>();
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
            : this(databaseManagerFactory, builder, ContextDbConnectionMode.AsynchronousAndTransactionSafe)
        {

        }

        /// <summary>
        /// Gets the connection mode.
        /// </summary>
        /// <value>The connection mode.</value>
        protected ContextDbConnectionMode ConnectionMode { get; private set; }

        /// <summary>
        /// Retrieves the or add database manager per transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns>DatabaseManager.</returns>
        private IDatabaseManager RetrieveOrAddDatabaseManagerPerTransaction(Transaction transaction)
        {
            string transactionId = null;

            if (transaction != null)
                transactionId = transaction.TransactionInformation.LocalIdentifier;

            var container = _databaseManagers.FirstOrDefault(manager => manager.TransactionId == transactionId);

            var result = container?.DatabaseManager;

            if (result == null)
            {
                result = _builder(_databaseManagerFactory);
                _databaseManagers.Add(new ManagerInstance { DatabaseManager = result, TransactionId = transactionId });
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
                    case ContextDbConnectionMode.PerTransaction:
                        result = RetrieveOrAddDatabaseManagerPerTransaction(Transaction.Current);
                        break;
                    case ContextDbConnectionMode.AsynchronousAndTransactionSafe:
                        if (Transaction.Current == null)
                        {
                            //IDatabaseManager dataReference = null;

                            //foreach (var weakReference in _weakReferences)
                            //{
                            //    if (weakReference.TryGetTarget(out dataReference) &&
                            //        dataReference.State == ConnectionState.Closed)
                            //        break;
                            //}

                            //result = dataReference;

                            //if (dataReference == null)
                            //{
                            result = _builder(_databaseManagerFactory);

                            _weakReferences.Add(new WeakReference<IDatabaseManager>(result));
                            //}
                        }
                        else
                        {
                            result = RetrieveOrAddDatabaseManagerPerTransaction(Transaction.Current);
                            result.OpenConnection();
                        }
                        break;
                }

                return result;
            }
        }

        #region Resource Management

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

        #endregion Resource Management

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