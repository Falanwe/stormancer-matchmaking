using Stormancer.Server;
using Stormancer.Server.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Configuration
{
    public class ConfigurationService
    {
        private readonly Dictionary<IConfigurationRefresh, EventHandler<EventArgs>> _registrations = new Dictionary<IConfigurationRefresh, EventHandler<EventArgs>>();
        private readonly IHost _host;

        public ConfigurationService(IHost host)
        {
            _host = host;
        }

        public void RegisterComponent(IConfigurationRefresh component)
        {
            var environement = _host.DependencyResolver.Resolve<IEnvironment>();
            component.Init(environement.Configuration);
            EventHandler<EventArgs> registration = (sender, _) => component.ConfigChanged(((IEnvironment)sender).Configuration);
            environement.ConfigurationChanged += registration;
            _registrations.Add(component, registration);
        }

        public void UnRegisterComponenet(IConfigurationRefresh component)
        {
            EventHandler<EventArgs> registration;
            if (_registrations.TryGetValue(component, out registration))
            {
                var environement = _host.DependencyResolver.Resolve<IEnvironment>();
                environement.ConfigurationChanged -= registration;
                _registrations.Remove(component);
            }
        }
    }
}
