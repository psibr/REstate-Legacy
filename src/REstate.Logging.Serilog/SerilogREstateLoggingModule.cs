using Autofac;
using Psibr.Platform.Logging;
using Serilog;

namespace REstate.Logging.Serilog
{
    public class SerilogREstateLoggingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAdapter<ILogger, IPlatformLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));
        }
    }
}