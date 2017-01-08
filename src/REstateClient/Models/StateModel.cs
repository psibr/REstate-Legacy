using System;

namespace REstateClient.Models
{
    internal class StateModel
    {
        public string StateName { get; set; }
        public string CommitTag { get; set; }

        public static implicit operator REstate.State(StateModel model)
        {
            return new REstate.State(model.StateName, Guid.Parse(model.CommitTag));
        }
    }
}
