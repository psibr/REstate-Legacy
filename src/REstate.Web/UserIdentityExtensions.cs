using System.Linq;
using System.Security.Claims;

namespace REstate.Web
{
    public static class UserIdentityExtensions
    {
        public static string GetApiKey(this ClaimsPrincipal user)
        {
            return (user).Claims.Single(claim => claim.Type == "ApiKey").Value;
        }
    }
}
