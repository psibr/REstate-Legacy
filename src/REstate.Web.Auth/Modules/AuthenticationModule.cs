using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy;
using Nancy.Cryptography;
using Nancy.ModelBinding;
using Nancy.Owin;
using Nancy.Responses.Negotiation;
using REstate.Auth;
using REstate.Auth.Repositories;
using REstate.Web.Auth.Requests;
using SignInDelegate = System.Func<System.Func<System.Guid, System.Collections.Generic.IDictionary<string, object>>, bool, string>;

namespace REstate.Web.Auth.Modules
{
    public class AuthenticationModule
        : NancyModule
    {

        public AuthenticationModule(AuthRoutePrefix prefix,
            IAuthRepositoryContextFactory authRepositoryContextFactory, CryptographyConfiguration crypto)
            : base(prefix)
        {

            Get["/login", true] = async (parameters, ct) =>
                await Task.FromResult(View["login.html"]);


            Post["/login", true] = async (parameters, ct) =>
            {
                var credentials = this.Bind<CredentialAuthenticationRequest>();

                if (string.IsNullOrWhiteSpace(credentials?.Username) || string.IsNullOrWhiteSpace(credentials.Password))
                    return Response.AsRedirect("/REstate/login");

                var environment = Context.GetOwinEnvironment();
                var signInDelegate = (SignInDelegate)environment["jwtandcookie.signin"];

                var passwordHash = Convert.ToBase64String(crypto.HmacProvider
                    .GenerateHmac(credentials.Password));


                IPrincipal principal;
                using (var repository = authRepositoryContextFactory.OpenAuthRepositoryContext())
                {
                    principal = await repository
                        .LoadPrincipalByCredentials(credentials.Username, passwordHash, ct);
                }

                if (principal == null) return Response.AsRedirect("/REstate/login");

                var jwt = signInDelegate((jti) => new Dictionary<string, object>
                {
                        { "sub", principal.UserOrApplicationName},
                        { "apikey", crypto.EncryptionProvider.Encrypt(principal.ApiKey)},
                        { "claims", principal.Claims }
                }, true);

                return Response.AsRedirect("/REstate");
            };

            Post["/apikey", true] = async (parameters, ct) =>
            {
                var apiKey = this.Bind<ApiKeyAuthenticationRequest>().ApiKey;

                var environment = Context.GetOwinEnvironment();
                var signInDelegate = (SignInDelegate)environment["jwtandcookie.signin"];

                IPrincipal principal;
                using (var repository = authRepositoryContextFactory.OpenAuthRepositoryContext())
                {
                    principal = await repository.LoadPrincipalByApiKey(apiKey, ct);
                }

                if (principal == null) return 401;

                var jwt = signInDelegate((jti) => new Dictionary<string, object>
                {
                    { "sub", principal.UserOrApplicationName},
                    { "apikey", crypto.EncryptionProvider.Encrypt(jti + principal.ApiKey)},
                    { "claims", principal.Claims }
                }, true);

                return Negotiate
                    .WithModel(await Task.FromResult(new { jwt }))
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }
    }
}