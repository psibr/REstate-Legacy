using System;
using System.Threading.Tasks;

namespace REstateScratchPad
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var restateClientFactory = new REstateClient.REstateClientFactory("http://localhost:5000/auth/apikey");

            var configClient = restateClientFactory.GetConfigurationClient("http://localhost:5000");

            var session = configClient.GetSession("E17705B5-D0FD-47F5-849F-F0881B954C58").Result;

            var act = new Action(() =>
            {


                //var machineDefinitnion = session.GetMachine("Load-Test-Machine3").Result;

                //if (machineDefinitnion == null)
                //{
                //    var def = new Machine
                //    {
                //        MachineName = "Load-Test-Machine3",
                //        InitialState = "Idle",
                //        StateConfigurations = new[]
                //        {
                //            new StateConfiguration
                //            {
                //                StateName = "Idle",
                //                Transitions = new []
                //                {
                //                    new Transition { TriggerName = "GoPathA", ResultantStateName = "PathA" },
                //                    new Transition { TriggerName = "GoPathB", ResultantStateName = "PathB" }
                //                }
                //            },
                //            new StateConfiguration
                //            {
                //                StateName = "PathA",
                //                OnEntry = new Code
                //                {
                //                    Name = "ScheduleIdle",
                //                    ConnectorKey = "Delay",
                //                    //Body = @"{ ""triggerName"": ""GoIdle"", ""delay"": 30 }"
                //                    Configuration = new Dictionary<string, string>
                //                    {
                //                        ["TriggerName"] = "GoIdle",
                //                        ["Delay"] = "0",
                //                        ["VerifyCommitTag"] = "True"
                //                    }
                //                },
                //                Transitions = new []
                //                {
                //                    new Transition { TriggerName = "GoIdle", ResultantStateName = "Idle" }
                //                }
                //            },
                //            new StateConfiguration
                //            {
                //                StateName = "PathB",
                //                Transitions = new []
                //                {
                //                    new Transition { TriggerName = "GoIdle", ResultantStateName = "Idle" }
                //                }
                //            }
                //        }
                //    };

                //    machineDefinitnion = session.DefineStateMachine(def).Result;
                //}

                var instanceId = session.Instantiate("Load-Test-Machine3").Result;

                var resultState = session.FireTrigger(instanceId, "GoPathA").Result;

                //var triggers = session.GetAvailableTriggers(instanceId).Result;
                //var random = new Random();

                //while (true)
                //{
                //    var resultingState = session.FireTrigger(instanceId, triggers.ElementAt(random.Next(1, triggers.Count + 1) - 1).TriggerName).Result;

                //    //Console.WriteLine(resultingState.StateName);

                //    var idleState = session.FireTrigger(instanceId, "GoIdle").Result;

                //    //Console.WriteLine(idleState.StateName);
                //}
            });

            //act.Invoke();

            Parallel.Invoke(act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act, act);

        }
    }
}
