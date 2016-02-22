using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Client.Models
{
    internal class TriggerModel
    {
        public int MachineDefinitionId { get; set; }
        public string TriggerName { get; set; }

        public static implicit operator REstate.Trigger(TriggerModel model)
        {
            return new REstate.Trigger(model.MachineDefinitionId, model.TriggerName);
        }
    }
}
