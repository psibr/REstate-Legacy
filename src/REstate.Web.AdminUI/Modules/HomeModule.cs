using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;
using REstate.Platform;
using Nancy.Owin;

namespace REstate.Web.AdminUI.Modules
{
    /// <summary>
    /// UI application module.
    /// </summary>
    public sealed class HomeModule
        : NancyModule
    {
        /// <summary>
        /// Registers the UI routes for the application.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public HomeModule(REstatePlatformConfiguration configuration)
        {
            Get("/", (_, ct) => BuildPageOrRedirect(Context, configuration, ct));
            Get("/{uri*}", (_, ct) => BuildPageOrRedirect(Context, configuration, ct));
        }

        private async Task<dynamic> BuildPageOrRedirect(NancyContext ctx, REstatePlatformConfiguration configuration, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(Context.GetOwinEnvironment()["owin.RequestPath"] as string))
            {
                return await Response.AsRedirect(configuration.AdminHttpService.Address,
                    RedirectResponse.RedirectType.Permanent);
            }

            if (Context.CurrentUser == null)
                return await Response.AsRedirect($"{configuration.AuthHttpService.Address}login");

            using (var fread = new FileStream(
                $"{configuration.AdminHttpService.StaticContentRootRoutePath}\\index.html",
                FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fread))
            {
                var indexHtml = await streamReader.ReadToEndAsync();

                return await Response.AsText(indexHtml, "text/html");
            }
        }
    }
}