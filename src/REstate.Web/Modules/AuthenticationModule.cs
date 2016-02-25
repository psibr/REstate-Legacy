using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy;
using Nancy.Cryptography;
using Nancy.ModelBinding;
using Nancy.Owin;
using Nancy.Responses.Negotiation;
using REstate.Auth;
using REstate.Auth.Susanoo;
using REstate.Repositories;
using REstate.Web.Requests;
using SignInDelegate = System.Func<System.Func<System.Guid, System.Collections.Generic.IDictionary<string, object>>, bool, string>;

namespace REstate.Web.Modules
{
    public class AuthenticationModule
        : NancyModule
    {
        protected static IHmacProvider HashProvider;

        public AuthenticationModule(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
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

                if (HashProvider == null)
                    HashProvider = new DefaultHmacProvider(
                        new PassphraseKeyGenerator(environment["REstate.passphrase"] as string,
                            new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 }));

                var passwordHash = Convert.ToBase64String(HashProvider.GenerateHmac(credentials.Password));


                IPrincipal principal;
                using (var repository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(null))
                {
                    principal = await repository.GetAuthRepository()
                        .LoadPrincipalByCredentials(credentials.Username, passwordHash, ct);
                }

                if (principal == null) return Response.AsRedirect("/REstate/login");

                var jwt = signInDelegate((jti) => new Dictionary<string, object>
                {
                        { "sub", principal.UserOrApplicationName},
                        { "apikey", new RijndaelEncryptionProvider(
                            new PassphraseKeyGenerator(environment["REstate.passphrase"] as string,
                                jti.ToByteArray()))
                            .Encrypt(principal.ApiKey)},
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
                using (var repository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(apiKey))
                {
                    principal = await repository.GetAuthRepository().LoadPrincipalByApiKey(apiKey, ct);
                }

                if (principal == null) return 401;

                var jwt = signInDelegate((jti) => new Dictionary<string, object>
                {
                    { "sub", principal.UserOrApplicationName},
                    { "apikey", new RijndaelEncryptionProvider(
                        new PassphraseKeyGenerator(environment["REstate.passphrase"] as string, jti.ToByteArray()))
                            .Encrypt(principal.ApiKey)},
                    { "claims", principal.Claims }
                }, true);

                return Negotiate
                    .WithModel(await Task.FromResult(new { jwt }))
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }
    }
}