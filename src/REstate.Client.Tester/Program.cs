using System;
using Newtonsoft.Json;
using REstate.Configuration;
using REstate.Platform;

namespace REstate.Client.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var configString = REstateConfiguration.LoadConfigurationFile();

            var config = JsonConvert.DeserializeObject<REstateConfiguration>(configString);

            var clientFactory = new REstateClientFactory($"{config.AuthAddress.Address}apikey");

            

            var machine = clientFactory
                .GetConfigurationClient(config.ConfigurationAddress.Address)
                .GetSession("98B8C4F3-4F45-4974-B434-53A18D9DCE36").Result
                .DefineStateMachine(new StateMachineConfiguration
                {
                    MachineDefinition = new MachineDefinition
                    {
                        MachineName = "Elevator",
                        InitialStateName = "GroundFloor",
                        IsActive = true
                    },
                    StateConfigurations = new IStateConfiguration[]
                    {
                        new StateConfiguration
                        {
                            State = new Configuration.State
                            {
                                StateName = "GroundFloor"
                            },
                            Transitions = new ITransition[]
                            {
                                new Transition
                                {
                                    StateName = "GroundFloor",
                                    TriggerName = "Down",
                                    ResultantStateName = "GroundFloor"
                                },
                                new Transition
                                {
                                    StateName = "GroundFloor",
                                    TriggerName = "Up",
                                    ResultantStateName = "FirstFloor"
                                }
                            }
                        },
                        new StateConfiguration
                        {
                            State = new Configuration.State
                            {
                                StateName = "FirstFloor"
                            },
                            Transitions = new ITransition[]
                            {
                                new Transition
                                {
                                    StateName = "FirstFloor",
                                    TriggerName = "Up",
                                    ResultantStateName = "FirstFloor"
                                },
                                new Transition
                                {
                                    StateName = "FirstFloor",
                                    TriggerName = "Down",
                                    ResultantStateName = "GroundFloor"
                                }
                            }
                        }
                    },
                    Triggers = new ITrigger[]
                    {
                        new Configuration.Trigger
                        {
                            TriggerName = "Up",
                            IsActive = true
                        },
                        new Configuration.Trigger{
                            TriggerName = "Down",
                            IsActive = true
                        }
                    }
                }).Result;

            var instanceSession = clientFactory.GetInstancesClient(config.InstancesAddress.Address)
                .GetSession("98B8C4F3-4F45-4974-B434-53A18D9DCE36").Result;

            var instanceGuid = instanceSession
                .InstantiateMachine(machine.MachineDefinition.MachineDefinitionId).Result;

            Console.WriteLine(instanceSession.GetMachineState(instanceGuid).Result);

            Console.WriteLine(instanceSession.FireTrigger(instanceGuid, "Up").Result.StateName);


        }
    }
}
