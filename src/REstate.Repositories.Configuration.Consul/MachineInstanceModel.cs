namespace REstate.Repositories.Core.Consul
{
    public class MachineInstanceModel
    {
        public string MachineInstanceId { get; set; }

        public string MachineDefinitionId { get; set; }

        public string StateName { get; set; }
    }
}