using Nancy;
using REstate.Platform;

namespace REstate.Web.Core.Modules
{
    /// <summary>
    /// Machine interactions module.
    /// </summary>
    public class SchemaModule
        : NancyModule
    {

        public SchemaModule(REstatePlatformConfiguration configuration)
        {
            Get["TriggerSchema", "/trigger"] = (parameters) =>
                Response.AsText(Schemas.Trigger
                    .Replace("{host}", configuration.CoreHttpService.Address)
                    .Replace(":/", "://"), "application/schema+json");

            Get["MachineSchema", "/machine"] = (parameters) =>
                Response.AsText(Schemas.Machine
                    .Replace("{host}", configuration.CoreHttpService.Address)
                    .Replace("//", "/")
                    .Replace(":/", "://"), "application/schema+json");

            Get["StateSchema", "/state"] = (parameters) =>
                Response.AsText(Schemas.State
                    .Replace("{host}", configuration.CoreHttpService.Address)
                    .Replace("//", "/")
                    .Replace(":/", "://"), "application/schema+json");

            Get["CodeSchema", "/code"] = (parameters) =>
                Response.AsText(Schemas.Code
                    .Replace("{host}", configuration.CoreHttpService.Address)
                    .Replace("//", "/")
                    .Replace(":/", "://"), "application/schema+json");

            Get["OnEntryFromSchema", "/on-entry-from"] = (parameters) =>
                Response.AsText(Schemas.OnEntryFrom
                    .Replace("{host}", configuration.CoreHttpService.Address)
                    .Replace("//", "/")
                    .Replace(":/", "://"), "application/schema+json");
        }
        
    }
}