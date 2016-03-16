using Nancy;
using REstate.Platform;

namespace REstate.Web.AdminUI.Modules
{
    /// <summary>
    /// UI application module.
    /// </summary>
    public class HomeModule
        : NancyModule
    {
        /// <summary>
        /// Registers the UI routes for the application.
        /// </summary>
        public HomeModule(REstateConfiguration configuration)
        {
                Get["/"] = _ => Context.CurrentUser == null 
                    ? Response.AsRedirect(configuration.AdminAddress)
                    : 404;
        }
    }
}
