using System;

namespace REstate
{
    /// <summary>
    /// Indicates a request for a machine definition was made with an unknown name.
    /// </summary>
    public class DefinitionDoesNotExistException
        : Exception
    {
        public DefinitionDoesNotExistException()
        {
        }

        public DefinitionDoesNotExistException(string message) : base(message)
        {
        }

        public DefinitionDoesNotExistException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
