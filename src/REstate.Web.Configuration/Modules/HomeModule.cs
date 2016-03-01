using Nancy;

namespace REstate.Web.Configuration.Modules
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
                    ? Response.AsRedirect($"{configuration.AuthBaseUrl}/login") 
                    : 200;
        }
    }
}
