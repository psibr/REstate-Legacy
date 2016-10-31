namespace REstateClient.Models
{
    internal class StateModel
    {
        public string MachineName { get; set; }
        public string StateName { get; set; }
        public string CommitTag { get; set; }

        public static implicit operator REstate.State(StateModel model)
        {
            return new REstate.State(model.MachineName, model.StateName, model.CommitTag);
        }
    }
}
