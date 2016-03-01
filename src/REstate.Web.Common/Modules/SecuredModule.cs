using Nancy;
using Nancy.Security;

namespace REstate.Web.Modules
{
    /// <summary>
    /// An abstract module used to secure modules with authentication and optionally claims.
    /// </summary>
    public abstract class SecuredModule
        : NancyModule
    {
        /// <summary>
        /// Requires the inheriting module to have authentication only.
        /// </summary>
        protected SecuredModule()
            :this(null)
        {
        }

        /// <summary>
        /// Requires the inheriting module to have authentication only.
        /// </summary>
        /// <param name="modulePath">The module path prefix.</param>
        protected SecuredModule(string modulePath)
            : this(modulePath, null)
        {
        }

        /// <summary>
        /// Requires the inheriting module to have 
        /// authentication and a collection of claims.
        /// </summary>
        /// <param name="modulePath">The module path prefix.</param>
        /// <param name="requiredClaims">The required claims.</param>
        protected SecuredModule(string modulePath, params string[] requiredClaims)
            : base(modulePath)
        {
            if (requiredClaims != null)
                this.RequiresClaims(requiredClaims);
            else
                this.RequiresAuthentication();
        }
    }
}