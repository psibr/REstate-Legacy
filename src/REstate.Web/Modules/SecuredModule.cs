using Nancy;
using Nancy.Security;
using System;
using System.Security.Claims;

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
        protected SecuredModule(REstateConfiguration configuration)
            : this(configuration, null)
        {
        }

        /// <summary>
        /// Requires the inheriting module to have authentication only.
        /// </summary>
        /// <param name="modulePath">The module path prefix.</param>
        protected SecuredModule(REstateConfiguration configuration, string modulePath)
            : this(configuration, modulePath, null)
        {
        }

        /// <summary>
        /// Requires the inheriting module to have
        /// authentication and a collection of claims.
        /// </summary>
        /// <param name="modulePath">The module path prefix.</param>
        /// <param name="claimPredicates"></param>
        protected SecuredModule(REstateConfiguration configuration, string modulePath, params Predicate<Claim>[] claimPredicates)
            : base(modulePath)
        {
            if (configuration.Authentication.UseAuthentication)
            {
                if (claimPredicates != null)
                    this.RequiresClaims(claimPredicates);
                else
                    this.RequiresAuthentication();
            }
        }
    }
}
