﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace REstate.Platform
{
    public static class ConnectorRegistrationExtensions
    {
        /// <summary>
        /// Registers all available connectors from the bin directory.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The configuration.</param>
        public static void RegisterConnectors(this ContainerBuilder builder, REstateConfiguration configuration)
        {
            var binFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent;
            var pluginFolder = new DirectoryInfo(binFolder.Parent.Parent.Parent.FullName + "/Connectors");
            IEnumerable<FileInfo> pluginAssemblies = binFolder.GetFiles("REstate.Connectors.*.dll", SearchOption.TopDirectoryOnly);

            if (pluginFolder.Exists)
            {
                pluginAssemblies = pluginAssemblies
                    .Union(pluginFolder.GetFiles("REstate.Connectors.*.dll", SearchOption.AllDirectories));
            }
            
            foreach (var pluginAssemblyFile in pluginAssemblies)
            {
                var asm = Assembly.LoadFrom(pluginAssemblyFile.FullName);
                var connectorModules = asm.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeof(IREstateConnectorModule)));

                foreach (var connectorModule in connectorModules)
                {
                    var module = (IREstateConnectorModule)Activator.CreateInstance(connectorModule);
                    module.Configuration = configuration;
                    builder.RegisterModule(module);
                }
            }
        }
    }
}