using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using RabbitMQ.Client;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.RabbitMq
{
    public class RabbitMqConnectorModule
        : Module, IREstateConnectorModule
    {
        public REstatePlatformConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<RabbitMqConnectorFactory>()
                .As<IConnectorFactory>();

            builder.Register(ctx => new ConnectionFactory
            {
                HostName = Configuration.ConnectorConfigurations["RabbitMq"]["hostName"],
                VirtualHost = Configuration.ConnectorConfigurations["RabbitMq"]["virtualHost"],
                UserName = Configuration.ConnectorConfigurations["RabbitMq"]["userName"],
                Password = Configuration.ConnectorConfigurations["RabbitMq"]["password"]
            });
        }
    }
}
