using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateMachineFactory
    {
        IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId,
            Machine configuration);
    }
}
