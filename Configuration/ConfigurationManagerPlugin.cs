using Stormancer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Server;

namespace Stormancer.Configuration
{
    public class ConfigurationManagerPlugin : IHostPlugin
    {
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.HostStarting += RegisterHost;
        }

        private static void RegisterHost(IHost host)
        {
            var service = new ConfigurationService(host);
            host.DependencyResolver.Register(service);
        }
    }
}
