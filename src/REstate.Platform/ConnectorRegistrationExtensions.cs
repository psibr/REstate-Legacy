using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;

namespace REstate.Platform
{
    public static class ConnectorRegistrationExtensions
    {
        /// <summary>
        /// Registers all available connectors from the bin directory.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The configuration.</param>
        public static void RegisterConnectors(this ContainerBuilder builder, REstatePlatformConfiguration configuration)
        {
            var binFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent;
            var pluginFolder = new DirectoryInfo(Path.Combine(binFolder.Parent.Parent.Parent.FullName, "connectors"));
            IEnumerable<FileInfo> pluginAssemblies = binFolder.GetFiles("REstate.Connectors.*.dll", SearchOption.TopDirectoryOnly);

            if (pluginFolder.Exists)
            {
                pluginAssemblies = pluginAssemblies
                    .Union(pluginFolder.GetFiles("REstate.Connectors.*.dll", SearchOption.AllDirectories));
            }
            
            foreach (var module in pluginAssemblies
                .Select(pluginAssemblyFile => Assembly.LoadFrom(pluginAssemblyFile.FullName))
                .Select(asm => asm.GetExportedTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IREstateConnectorModule))))
                .SelectMany(connectorModules => connectorModules.Select(connectorModule => 
                    (IREstateConnectorModule)Activator.CreateInstance(connectorModule))))
            {
                module.Configuration = configuration;
                builder.RegisterModule(module);
            }
        }
    }
}