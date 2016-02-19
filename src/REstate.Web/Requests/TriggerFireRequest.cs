using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Web.Requests
{
    public class TriggerFireRequest
    {
        public Guid MachineInstanceGuid { get; set; }

        public string TriggerName { get; set; }

        public string Payload { get; set; }
    }
}
