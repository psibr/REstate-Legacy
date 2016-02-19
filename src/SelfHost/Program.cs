using System;
using System.Configuration;
using Microsoft.Owin.Hosting;
using REstate.Owin;

namespace SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["REstate.url"];

            Startup.PassPhrase = ConfigurationManager.AppSettings["REstate.passphrase"];

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");

                Console.ReadLine();
            }
        }
    }
}