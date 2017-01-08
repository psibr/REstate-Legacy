using System;

namespace REstate
{
    /// <summary>
    /// Indicates a request for an instance was made with an unknown or deleted InstanceId
    /// </summary>
    public class InstanceDoesNotExistException
        : Exception
    {
        public InstanceDoesNotExistException()
        {
        }

        public InstanceDoesNotExistException(string message) : base(message)
        {
        }

        public InstanceDoesNotExistException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
