using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
