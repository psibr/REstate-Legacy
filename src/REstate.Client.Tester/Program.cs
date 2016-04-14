using System;
using System.Text;
using Consul;
using Newtonsoft.Json;
using Psibr.Platform;
using REstate.Configuration;
using REstate.Platform;

namespace REstate.Client.Tester
{
    class Program
    {
        static void Main(string[] args)
        {

            var configString = PlatformConfiguration.LoadConfigurationFile("REstateConfig.json");

            var config = JsonConvert.DeserializeObject<REstatePlatformConfiguration>(configString);

            var clientFactory = new REstateClientFactory($"{config.AuthHttpService.Address}apikey");

            var sess = clientFactory
                .GetConfigurationClient($"{config.CoreHttpService.Address}configuration")
                .GetSession("F627451D-B20C-4640-80C7-CC9DFEB7D326").Result;

            var machine = sess
                .DefineStateMachine(new StateMachineConfiguration
                {
                    MachineDefinition = new MachineDefinition
                    {
                        MachineName = "AutoRotate",
                        InitialStateName = "Created",
                        IsActive = true
                    },
                    StateConfigurations = new IStateConfiguration[]
                    {
                        new StateConfiguration
                        {
                            State = new Configuration.State
                            {
                                StateName = "Created"
                            },
                            Transitions = new ITransition[]
                            {
                                new Transition
                                {
                                    StateName = "Created",
                                    TriggerName = "Configure",
                                    ResultantStateName = "Configured"
                                }
                            }
                        },
                        new StateConfiguration
                        {
                            State = new Configuration.State
                            {
                                StateName = "Configured"
                            },
                            Transitions = new ITransition[]
                            {
                                new Transition
                                {
                                    StateName = "Configured",
                                    TriggerName = "Receive",
                                    ResultantStateName = "AutoRotateReceived"
                                }
                            }
                        },
                        new StateConfiguration
                        {
                            State = new Configuration.State
                            {
                                StateName = "AutoRotateReceived"
                            }
                        }
                    },
                    Triggers = new ITrigger[]
                    {
                        new Configuration.Trigger
                        {
                            TriggerName = "Configure",
                            IsActive = true
                        },
                        new Configuration.Trigger{
                            TriggerName = "Receive",
                            IsActive = true
                        }
                    }
                }).Result;

            var instanceSession = clientFactory.GetInstancesClient(config.CoreHttpService.Address)
                .GetSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result;

            var instanceId = instanceSession
                .InstantiateMachine(machine.MachineDefinition.MachineDefinitionId).Result;

            Console.WriteLine(instanceSession.GetMachineState(instanceId).Result);

            Console.WriteLine(instanceSession.FireTrigger(instanceId, "Up").Result.StateName);


        }
    }
}
