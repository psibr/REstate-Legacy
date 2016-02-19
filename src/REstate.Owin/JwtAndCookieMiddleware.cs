using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JWT;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace REstate.Owin
{
    public class JwtAndCookieMiddleware
    {
        private readonly AppFunc _next;
        private readonly JwtAndCookieMiddlewareOptions _options;

        public JwtAndCookieMiddleware(AppFunc next, JwtAndCookieMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment["REstate.passphrase"] = _options.PassPhrase;

            DefineJwtGenerator(environment, _options);

            var headers = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];

            ClaimsPrincipal principal = null;
            if (headers.ContainsKey("Authorization"))
            {
                var token = headers["Authorization"]?.FirstOrDefault()?.Replace("Bearer ", "");

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
                environment["server.User"] = principal;

            // Buffer the response
            var stream = (Stream)environment["owin.ResponseBody"];
            var buffer = new MemoryStream();
            environment["owin.ResponseBody"] = buffer;

            await _next(environment);

            //Handle Response
            buffer.Seek(0, SeekOrigin.Begin);

            await buffer.CopyToAsync(stream);
        }

        private void DefineJwtGenerator(IDictionary<string, object> environment, JwtAndCookieMiddlewareOptions options)
        {
            environment["jwtandcookie.signin"] = new Func<Func<Guid, IDictionary<string, object>>, bool, string>(
                (claimBuilder, buildCookie) =>
                {
                    var jti = Guid.NewGuid();
                    var jwt = JsonWebToken.Encode(new Dictionary<string, object>
                    {
                        { "jti", jti.ToString() },
                        { "exp", Math.Round(
                                ((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)) +
                                 TimeSpan.FromMinutes(30))
                                    .TotalSeconds).ToString(CultureInfo.InvariantCulture) }
                    }.Union(claimBuilder(jti)).ToDictionary(p => p.Key, p => p.Value), _options.PassPhrase,
                        JwtHashAlgorithm.HS256);

                    if (!buildCookie) return jwt;

                    var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

                    responseHeaders.Add("Set-Cookie", new[] { $"{options.CookieName}={jwt};path={options.CookiePath};httponly={options.CookieHttpOnly}" });

                    return jwt;
                });
        }

        private IDictionary<string, object> DecodeAndValidateToken(string token)
        {

            IDictionary<string, object> payload = null;

            try
            {
                payload = JsonWebToken.DecodeToObject<Dictionary<string, object>>(token, _options.PassPhrase);
            }
            catch (SignatureVerificationException) //No meaningful valid payload, return null.
            {
                return payload;
            }

            return payload;
        }
    }
}
