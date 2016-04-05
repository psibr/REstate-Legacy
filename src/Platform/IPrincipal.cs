using System.Collections.Generic;

namespace Platform
{
    public interface IPrincipal
    {
        string ApiKey { get; set; }
        ICollection<string> Claims { get; set; }
        string PrincipalType { get; set; }
        string UserOrApplicationName { get; set; }
    }
}