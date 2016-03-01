using System.Collections.Generic;
using Nancy.Security;

namespace REstate.Web
{
    public class User 
        : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }

        public string ApiKey { get; set; }
    }
}