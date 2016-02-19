using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy;
using Nancy.Owin;
using Nancy.Responses;

namespace REstate.Web.Modules
{
    /// <summary>
    /// UI application module.
    /// </summary>
    public class UiModule
        : NancyModule
    {
        /// <summary>
        /// Registers the UI routes for the application.
        /// </summary>
        public UiModule()
        {
                Get["/"] = _ => Response.AsRedirect("/REstate/login");
        }
    }
}
