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
        /// New DatabaseManager for each call.
        /// </summary>
        Each = 2
    }
}
