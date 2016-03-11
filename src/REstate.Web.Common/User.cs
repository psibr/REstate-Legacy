using Nancy.Security;
using System.Collections.Generic;

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