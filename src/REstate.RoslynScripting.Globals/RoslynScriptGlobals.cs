using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.RoslynScripting.Globals
{
    public class RoslynScriptGlobals
    {
        public string CurrentState =>
            Machine.GetCurrentState().StateName;

        public IStateMachine Machine { get; set; }

        public string Payload { get; set; }
    }
}