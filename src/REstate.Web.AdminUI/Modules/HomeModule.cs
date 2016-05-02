using System.IO;
using Nancy;
using Nancy.Responses;
using REstate.Platform;
using Nancy.Owin;

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
            Get["/"] = _ => BuildPageOrRedirect(configuration);
            Get[@"/(.*)"] = _ => BuildPageOrRedirect(configuration);
            Get[@"/(.*)/(.*)"] = _ => BuildPageOrRedirect(configuration);
        }

        private dynamic BuildPageOrRedirect(REstatePlatformConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(Context.GetOwinEnvironment()["owin.RequestPath"] as string))
            {
                return Response.AsRedirect(configuration.AdminHttpService.Address,
                    RedirectResponse.RedirectType.Permanent);
            }

            if (Context.CurrentUser == null)
                return Response.AsRedirect($"{configuration.AuthHttpService.Address}login");

            using (var fread = new FileStream(
                $"{configuration.AdminHttpService.StaticContentRootRoutePath}\\index.html",
                FileMode.Open))
            using (var streamReader = new StreamReader(fread))
            {
                var indexHtml = streamReader.ReadToEnd();

                //not sure if this is needed now
                indexHtml = indexHtml.Replace(@"<base href=""/"">", @"<base href=""/restate/admin/"">");

                return Response.AsText(indexHtml, "text/html");
            }
        }
    }
}