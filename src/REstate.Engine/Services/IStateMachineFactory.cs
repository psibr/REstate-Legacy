using REstate.Configuration;

namespace REstate.Engine.Services
{
    public interface IStateMachineFactory
    {
        IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId,
            Machine configuration);

        IStateMachine ConstructFromConfiguration(string apiKey, Machine configuration);
    }
}
