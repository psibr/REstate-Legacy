using System.Collections.Generic;

namespace REstate.Auth
{
    public interface IPrincipal
    {
        string ApiKey { get; set; }
        ICollection<string> Claims { get; set; }
        int PrincipalId { get; set; }
        string PrincipalType { get; set; }
        string UserOrApplicationName { get; set; }
    }
}