using System;

namespace REstate
{
    public class StateConflictException
        : Exception
    {
        public StateConflictException()
        {
        }

        public StateConflictException(string message) : base(message)
        {
        }

        public StateConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
