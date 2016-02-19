using System.Collections.Generic;

namespace REstate.Web.Modules
{
    /// <summary>
    /// An abstract module that requires authentication 
    /// and the configuration route prefix
    /// </summary>
    public abstract class ConfigurationModule
        : SecuredModule
    {
        /// <summary>
        /// Requires the inheriting module to have authentication, 
        /// the configuration route prefix, 
        /// and optionally provide a collection of required claims.
        /// </summary>
        /// <param name="modulePath">The module path prefix.</param>
        /// <param name="requiredClaims">The required claims.</param>
        protected ConfigurationModule(string modulePath, params string[] requiredClaims)
            : base("/configuration" + modulePath, requiredClaims)
        {
        }
    }
}