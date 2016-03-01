using Nancy.Security;

namespace REstate.Web
{
    public static class UserIdentityExtensions
    {
        public static string GetApiKey(this IUserIdentity user)
        {
            return (user as User)?.ApiKey;
        }
    }
}