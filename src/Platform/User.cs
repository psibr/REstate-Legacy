using System.Collections.Generic;

namespace Platform
{
    public class Principal : IPrincipal
    {
        public string UserOrApplicationName { get; set; }

        public string PrincipalType { get; set; }

        public ICollection<string> Claims { get; set; }

        public string ApiKey { get; set; }
    }
}