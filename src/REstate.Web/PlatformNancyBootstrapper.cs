using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using REstate.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace REstate.Web
{
    /// <summary>
    /// The bootstrapper enables you to reconfigure the composition of the framework,
    /// by overriding the various methods and properties.
    /// For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
    /// </summary>
    public class PlatformNancyBootstrapper
        : AutofacNancyBootstrapper
    {
        protected IPlatformLogger Logger { get; }
        protected REstateConfiguration Configuration { get; }

        public PlatformNancyBootstrapper(ILifetimeScope scopedKernel)
        {
            ScopedKernel = scopedKernel;
            RootPathProvider = new WwwrootPathProvider();
            Logger = scopedKernel.Resolve<IPlatformLogger>();
            Configuration = scopedKernel.Resolve<REstateConfiguration>();
        }

        protected ILifetimeScope ScopedKernel;

        protected virtual void ConfigureAuthentication(IPipelines pipelines)
        {
        }

        protected override void ApplicationStartup(ILifetimeScope kernel, IPipelines pipelines)
        {
            base.ApplicationStartup(kernel, pipelines);

            ConfigureAuthentication(pipelines);

            pipelines.AfterRequest += (ctx) =>
            {
                if (ctx.Response == null) return;

                var statusCode = (int)ctx.Response.StatusCode;

                if (statusCode <= 400) return; //Return early if no errors or only bad request.

                var requestLogger = Logger.ForContext("clientAddress", ctx.Request.UserHostAddress)
                    .ForContext("userName", ctx.CurrentUser?.Identity.Name);

                switch (statusCode)
                {
                    case 401:
                        requestLogger
                            .Debug("Attempt to access resource while unauthorized: {statusCode} " +
                                   "with reason-phrase: {reasonPhrase} at {method} {requestPath}",
                                statusCode, ctx.Response.ReasonPhrase, ctx.Request.Method, ctx.Request.Path);
                        break;

                    case 403:
                        requestLogger
                            .Warning("Attempt to access forbidden resource: {statusCode} with reason-phrase:" +
                                     " {reasonPhrase} at {method} {requestPath}",
                                statusCode, ctx.Response.ReasonPhrase, ctx.Request.Method, ctx.Request.Path);
                        break;

                    case 404:
                        requestLogger
                            .Debug("Not Found request: {statusCode} with reason-phrase: {reasonPhrase} at {method} {requestPath}",
                                statusCode, ctx.Response.ReasonPhrase, ctx.Request.Method, ctx.Request.Path);
                        break;

                    case 500:
                        requestLogger
                            .Warning("Internal Server Error: {statusCode} with reason-phrase: {reasonPhrase} at {method} {requestPath}",
                                statusCode, ctx.Response.ReasonPhrase, ctx.Request.Method, ctx.Request.Path);
                        break;

                    default:
                        requestLogger
                            .Warning("Unhandled status code: {statusCode} with reason-phrase: {reasonPhrase} at {method} {requestPath}",
                                statusCode, ctx.Response.ReasonPhrase, ctx.Request.Method, ctx.Request.Path);
                        break;
                }
            };

            pipelines.OnError += (ctx, ex) =>
            {
                var errorId = Guid.NewGuid().ToString();

                var requestLogger = Logger.ForContext("clientAddress", ctx.Request.UserHostAddress)
                    .ForContext("userName", ctx.CurrentUser?.Identity.Name)
                    .ForContext("requestErrorId", errorId);

                requestLogger.Error(ex, "An unhandled exception occured with message: " +
                                        "{message} at {method} {requestPath}",
                                    ex.Message, ctx.Request.Method, ctx.Request.Path);

                var reasonPhrase = ex.Message.Replace("/n", "").Replace("/r", "");
                var reasonPhraseBytes = Encoding.UTF8.GetBytes(reasonPhrase);

                if (!(ex is ArgumentException))
                    return new Response
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        ReasonPhrase = $"RequestErrorId: {errorId}",
                        ContentType = "text/plain",
                        Contents = stream =>
                        {
                            stream.Write(reasonPhraseBytes, 0, reasonPhraseBytes.Length);
                            stream.Flush();
                        },
                    };

                return new Response
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = reasonPhrase,
                    Contents = stream =>
                    {
                        stream.Write(reasonPhraseBytes, 0, reasonPhraseBytes.Length);
                        stream.Flush();
                    },
                    ContentType = "text/plain"
                };
            };
        }

        protected override IRootPathProvider RootPathProvider { get; }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Clear();

            foreach (var source in StaticContentsConventions.Reverse())
            {
                nancyConventions.StaticContentsConventions.AddDirectory(source.Key, source.Value);
            }
        }

        protected virtual IDictionary<string, string> StaticContentsConventions => new Dictionary<string, string>
        {
            {"/", "wwwroot"},
            {"js", "wwwroot/js"},
            {"css", "wwwroot/css"},
            {"dist", "wwwroot/dist"},
            { "app", "wwwroot/app"},
            { "libs", "wwwroot/node_modules" }
        };

        protected override ILifetimeScope GetApplicationContainer() => ScopedKernel;
    }

    public class WwwrootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            var location = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;

            return location.Parent.Parent.FullName;
        }
    }
}
