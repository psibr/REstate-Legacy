using Nancy;
using Nancy.ModelBinding;
using Nancy.Owin;
using REstate.Web.Auth.Requests;
using System.Collections.Generic;
using REstate.Web.Auth.Responses;
using SignInDelegate = System.Func<System.Func<System.Guid, System.Collections.Generic.IDictionary<string, object>>, bool, string>;
using REstate.Auth;

namespace REstate.Web.Auth.Modules
{
    public sealed class AuthenticationModule
        : NancyModule
    {
        public AuthenticationModule(REstateConfiguration configuration,
            IAuthRepositoryFactory authRepositoryFactory)
            : base("/auth")
        {

            //Get("/login", (parameters) => View["login.html"]);


            //Post("/login", async (parameters, ct) =>
            //{
            //    var credentials = this.Bind<CredentialAuthenticationRequest>();

            //    if (string.IsNullOrWhiteSpace(credentials?.Username) || string.IsNullOrWhiteSpace(credentials.Password))
            //        return 401

            //    var environment = Context.GetOwinEnvironment();
            //    var signInDelegate = (SignInDelegate)environment["jwtandcookie.signin"];

            //    var passwordHash = Convert.ToBase64String(crypto.HmacProvider
            //        .GenerateHmac(credentials.Password));

            //    IPrincipal principal;
            //    using (var repository = authRepositoryContextFactory.OpenAuthRepositoryContext())
            //    {
            //        principal = await repository
            //            .LoadPrincipalByCredentials(credentials.Username, passwordHash, ct);
            //    }

            //    if (principal == null) return 401;

            //    signInDelegate((jti) => new Dictionary<string, object>
            //    {
            //        { "sub", principal.UserOrApplicationName},
            //        { "apikey", principal.ApiKey},
            //        { "claims", principal.Claims }
            //    }, true);

            //    return 201;
            //});

            //We only have routes if authentication is turned on.
            if (configuration.AuthenticationSettings.UseAuthentication)
            {
                Post("/apikey", async (parameters, ct) =>
                {
                    var apiKeyRequest = this.Bind<ApiKeyAuthenticationRequest>();

                    if (string.IsNullOrWhiteSpace(apiKeyRequest?.ApiKey))
                        return new Response
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            ReasonPhrase = "Unable to detect ApiKey, check content-type headers."
                        };

                    var apiKey = apiKeyRequest.ApiKey;

                    var environment = Context.GetOwinEnvironment();
                    var signInDelegate = (SignInDelegate)environment["jwtandcookie.signin"];

                    IPrincipal principal;
                    using (var repository = authRepositoryFactory.OpenRepository())
                    {
                        principal = await repository.LoadPrincipalByApiKey(apiKey, ct);
                    }

                    if (principal == null) return 401;

                    var jwt = signInDelegate((jti) => new Dictionary<string, object>
                    {
                        { "sub", principal.UserOrApplicationName},
                        { "apikey", principal.ApiKey},
                        { "claims", principal.Claims }
                    }, false);

                    return Negotiate
                        .WithModel(new JwtResponse { Jwt = jwt })
                        .WithAllowedMediaRange("application/json");
                });
            }
        }
    }
}