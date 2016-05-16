using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy.Conventions;
using Psibr.Platform;
using Psibr.Platform.Nancy;

namespace REstate.Services.AdminUI
{
    public class Bootstrapper : PlatformNancyBootstrapper
    {
        public Bootstrapper(ILifetimeScope scopedKernel, IPlatformConfiguration configuration)
            : base(scopedKernel, configuration)
        {
            
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.AddDirectory("css", "wwwroot/css");
            nancyConventions.StaticContentsConventions.AddDirectory("js", "wwwroot/js");
        }
    }
}
