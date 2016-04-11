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

            //var client = new ConsulClient(new ConsulClientConfiguration
            //{
            //    Address = new Uri("http://68.97.116.37:8500", UriKind.Absolute),
            //    Datacenter = "home"
            //});

            //var putPair = new KVPair("hello")
            //{
            //    Value = Encoding.UTF8.GetBytes("Hello Consul")
            //};

            //var putAttempt = client.KV.Put(putPair).Result;

            //if (putAttempt.Response)
            //{
            //    var getPair = client.KV.Get("hello").Result;

            //    var res =  Encoding.UTF8.GetString(getPair.Response.Value, 0,
            //        getPair.Response.Value.Length);
            //}

            var configString = PlatformConfiguration.LoadConfigurationFile("REstateConfig.json");

            var config = JsonConvert.DeserializeObject<REstatePlatformConfiguration>(configString);

            var clientFactory = new REstateClientFactory($"{config.AuthAddress.Address}apikey");

            var machine = clientFactory
                .GetConfigurationClient($"{config.CoreAddress.Address}configuration")
                .GetSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result
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

            var instanceSession = clientFactory.GetInstancesClient(config.CoreAddress.Address)
                .GetSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result;

            var instanceId = instanceSession
                .InstantiateMachine(machine.MachineDefinition.MachineDefinitionId).Result;

            Console.WriteLine(instanceSession.GetMachineState(instanceId).Result);

            Console.WriteLine(instanceSession.FireTrigger(instanceId, "Up").Result.StateName);


        }
    }
}
