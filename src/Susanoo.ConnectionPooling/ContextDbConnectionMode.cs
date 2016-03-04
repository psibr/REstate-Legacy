namespace Susanoo.ConnectionPooling
{
    /// <summary>
    /// Enum ContextDbConnectionMode
    /// </summary>
    public enum ContextDbConnectionMode
    {
        /// <summary>
        /// Uses a single database manager and connection for all calls (synchronous and potentially shared state)
        /// </summary>
        Shared = 1,

        /// <summary>
        /// Maintains a shared database manager for most calls, but creates a new database manager for transaction calls.
        /// </summary>
        PerTransaction = 2,

        /// <summary>
        /// Creates a new database manager and connection for each non-transactional call if the previous is still executing or open. (round-robin)
        /// Transactional calls always get their own shared database manager and connection.
        /// </summary>
        AsynchronousAndTransactionSafe = 3
    }
}