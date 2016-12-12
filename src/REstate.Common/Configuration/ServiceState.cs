using REstate.Configuration;

public class ServiceState
{
    public string StateName { get; set; }

    public string Description { get; set; }

    public bool DisableAcknowledgement { get; set; }

    public Transition[] Transitions { get; set; }

    public long? RetryDelaySeconds { get; set; }

    public ServiceEntryConnector OnEntry { get; set; }
}