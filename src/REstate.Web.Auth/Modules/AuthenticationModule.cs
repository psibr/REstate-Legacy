﻿using Nancy;
using Nancy.Cryptography;
using Nancy.ModelBinding;
using Nancy.Owin;
using REstate.Web.Auth.Requests;
using System;
using System.Collections.Generic;
using Psibr.Platform;
using Psibr.Platform.Repositories;
using REstate.Platform;
using REstate.Web.Auth.Responses;
using SignInDelegate = System.Func<System.Func<System.Guid, System.Collections.Generic.IDictionary<string, object>>, bool, string>;

namespace REstate.Web.Auth.Modules
{
    public sealed class AuthenticationModule
        : NancyModule
    {

        public AuthenticationModule(AuthRoutePrefix prefix, REstatePlatformConfiguration configuration,
            IAuthRepositoryContextFactory authRepositoryContextFactory, CryptographyConfiguration crypto)
            : base(prefix)
        {

            Get("/login", (parameters) => View["login.html"]);


            Post("/login", async (parameters, ct) =>
            {
                var credentials = this.Bind<CredentialAuthenticationRequest>();

                if (string.IsNullOrWhiteSpace(credentials?.Username) || string.IsNullOrWhiteSpace(credentials.Password))
                    return Response.AsRedirect(configuration.AuthHttpService.Address + "login");

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

                if (principal == null) return Response.AsRedirect(configuration.AuthHttpService.Address + "login");

                signInDelegate((jti) => new Dictionary<string, object>
                {
                    { "sub", principal.UserOrApplicationName},
                    { "apikey", crypto.EncryptionProvider.Encrypt(jti + principal.ApiKey)},
                    { "claims", principal.Claims }
                }, true);

                return Response.AsRedirect(configuration.AdminHttpService.Address);
            });

            Post("/apikey", async (parameters, ct) =>
            {
                var apiKeyRequest = this.Bind<ApiKeyAuthenticationRequest>();

                if(string.IsNullOrWhiteSpace(apiKeyRequest?.ApiKey))
                    return new Response
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        ReasonPhrase = "Unable to detect ApiKey, check content-type headers."
                    };

                var apiKey = apiKeyRequest.ApiKey;

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
                }, false);

                return new JwtResponse { Jwt = jwt };
            });
        }
    }
}