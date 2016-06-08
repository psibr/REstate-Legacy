using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Web.Core.Responses
{
    public class IsInStateResponse
    {
        public string QueriedStateName { get; set; }

        public bool IsInState { get; set; }
    }
}
