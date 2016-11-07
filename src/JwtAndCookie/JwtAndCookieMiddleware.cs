using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OwinJwtAndCookie
{
    using Newtonsoft.Json;
    using System.Security.Cryptography.X509Certificates;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class JwtAndCookieMiddleware
    {
        private readonly AppFunc _next;
        private readonly Options _options;
        private readonly Jose.JweEncryption encryptionAlg = Jose.JweEncryption.A128CBC_HS256; //Will switch to AES###GCM when CoreFx supports it. (Windows Only ATM)

        public JwtAndCookieMiddleware(AppFunc next, Options options)
        {
            _next = next;
            _options = options;

            Jose.JWT.JsonMapper = new Jose.NewtonsoftMapper();
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            DefineJwtGenerator(environment, _options);

            var headers = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];

            ClaimsPrincipal principal = null;
            if (headers.ContainsKey("Authorization"))
            {
                var token = headers["Authorization"]?.FirstOrDefault()?.Replace("Bearer", "").Trim();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    //We have a token, go find the API key we assigned.
                    var payload = DecodeAndValidateToken(token);

                    principal = _options.CreatePrincipal(payload);
                }
            }
            else if (headers.ContainsKey("Cookie") && (headers["Cookie"].FirstOrDefault()?.Contains($"{_options.CookieName}=") ?? false))
            {
                var cookies = headers["Cookie"].First();

                var keyIndex = cookies.IndexOf(_options.CookieName, StringComparison.Ordinal);
                var keyLength = _options.CookieName.Length;
                var startingIndex = keyIndex + keyLength + 1;
                var endingIndex = cookies.IndexOf(";", keyIndex, StringComparison.Ordinal);

                var cookieValue = cookies.Substring(startingIndex, endingIndex != -1 ? endingIndex - startingIndex : cookies.Length - startingIndex);

                var payload = DecodeAndValidateToken(cookieValue);

                principal = _options.CreatePrincipal(payload);
            }

            if (principal != null)
                environment[_options.ClaimsPrincipalResourceName] = principal;

            return _next(environment);
        }

        private void DefineJwtGenerator(IDictionary<string, object> environment, Options options)
        {
            environment["jwtandcookie.signin"] = new Func<Func<Guid, IDictionary<string, object>>, bool, string>(
                (claimBuilder, buildCookie) =>
                {
                    var jti = Guid.NewGuid();
                    var jwt = Jose.JWT.Encode(new Dictionary<string, object>
                    {
                        { "jti", jti.ToString() },
                        { "exp",  BuildExpHeader(options) }
                    }.Union(claimBuilder(jti)).ToDictionary(p => p.Key, p => p.Value),
                        _options.Certificate.GetRSAPublicKey(),
                        Jose.JweAlgorithm.RSA_OAEP, encryptionAlg);

                    if (!buildCookie) return jwt;

                    var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

                    responseHeaders.Add("Set-Cookie", new[] { $"{options.CookieName}={jwt};path={options.CookiePath};httponly={options.CookieHttpOnly}" });

                    return jwt;
                });
        }

        private static string BuildExpHeader(Options options)
        {
            var timePeriod = DateTime.UtcNow
                             - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                             + options.TokenLifeSpan;

            return Math.Round(timePeriod.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        private IDictionary<string, object> DecodeAndValidateToken(string token)
        {
            IDictionary<string, object> payload = null;

            try
            {
                payload = Jose.JWT.Decode<IDictionary<string, object>>(token,
                    _options.Certificate.GetRSAPrivateKey(),
                    Jose.JweAlgorithm.RSA_OAEP, encryptionAlg);
            }
            catch (Jose.JoseException) //No meaningful valid payload, return null.
            {
                return payload;
            }
            catch (JsonReaderException)
            {
                return payload;
            }

            return payload;
        }
    }
}
