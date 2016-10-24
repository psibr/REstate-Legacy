using REstate.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REstateScratchPad
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var act = new Action(() =>
            {
                var restateClientFactory = new REstateClient.REstateClientFactory("http://localhost:5000/auth/apikey");

                var configClient = restateClientFactory.GetConfigurationClient("http://localhost:5000");

                var session = configClient.GetSession("E17705B5-D0FD-47F5-849F-F0881B954C58").Result;

                var machineDefinitnion = session.GetMachine("Load-Test-Machine").Result;

                if (machineDefinitnion == null)
                {
                    machineDefinitnion = session.DefineStateMachine(new Machine
                    {
                        MachineName = "Load-Test-Machine",
                        InitialState = "Idle",
                        StateConfigurations = new[]
                        {
                    new StateConfiguration
                    {
                        StateName = "Idle",
                        Transitions = new []
                        {
                            new Transition { TriggerName = "GoPathA", ResultantStateName = "PathA" },
                            new Transition { TriggerName = "GoPathB", ResultantStateName = "PathB" }
                        }
                    },
                    new StateConfiguration
                    {
                        StateName = "PathA",
                        Transitions = new []
                        {
                            new Transition { TriggerName = "GoIdle", ResultantStateName = "Idle" }
                        }
                    },
                    new StateConfiguration
                    {
                        StateName = "PathB",
                        Transitions = new []
                        {
                            new Transition { TriggerName = "GoIdle", ResultantStateName = "Idle" }
                        }
                    }
                }
                    }).Result;
                }

                var instanceId = session.Instantiate("Load-Test-Machine").Result;

                var triggers = session.GetAvailableTriggers(instanceId).Result;
                var random = new Random();

                while (true)
                {
                    var resultingState = session.FireTrigger(instanceId, triggers.ElementAt(random.Next(1, triggers.Count + 1) - 1).TriggerName).Result;

                    //Console.WriteLine(resultingState.StateName);

                    var idleState = session.FireTrigger(instanceId, "GoIdle").Result;

                    //Console.WriteLine(idleState.StateName);
                }
            });
            
            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = 8 }, act, act, act,act,act,act,act,act);

        }
    }
}
