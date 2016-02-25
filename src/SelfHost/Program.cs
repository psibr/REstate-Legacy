using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Microsoft.Owin.Hosting;
using REstate.Chrono.Susanoo;
using REstate.Owin;
using REstate.Client;

namespace SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["REstate.url"];

            Startup.PassPhrase = ConfigurationManager.AppSettings["REstate.passphrase"];

            using (var app = WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");

                var client = new REstateClient("http://localhost/restate/");
                using (var session = client.GetAuthenticatedSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result)
                {
                    var engine = new ChronoEngineFactory().CreateEngine();
                    var consumer = new ChronoConsumer(engine, session);

                    consumer.Start();

                    Console.ReadLine();

                    consumer.Stop();

                }
            }
        }
    }
}