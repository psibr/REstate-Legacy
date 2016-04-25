using Nancy;
using Nancy.Responses;
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
        public HomeModule(REstatePlatformConfiguration configuration)
        {
            Get["/"] = _ =>
            {
                return this.Response.AsRedirect(configuration.AdminHttpService.Address + "ui" + this.Request.Url.Query, RedirectResponse.RedirectType.Permanent);
            };

            Get["/ui"] = _ =>
            {
                if (Context.CurrentUser == null)
                    return Response.AsRedirect($"{configuration.AuthHttpService.Address}login");

                using (var fread = new System.IO.FileStream($"{configuration.AdminHttpService.StaticContentRootRoutePath}\\index.html", System.IO.FileMode.Open))
                using (var streamReader = new System.IO.StreamReader(fread))
                {
                    var indexHtml = streamReader.ReadToEnd();

                    indexHtml = indexHtml.Replace(@"<base href=""/"">", @"<base href=""./ui"">");


                    return Response.AsText(indexHtml, "text/html");
                }


            };
        }
    }
}