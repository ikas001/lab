using Microsoft.Xrm.Sdk;
using System;

namespace Plugins.Warehouse
{
    public interface ILocalPluginContext
    {
        IPluginExecutionContext PluginExecutionContext { get; }
        IOrganizationServiceFactory OrgSvcFactory { get; }
        ITracingService TracingService { get; }
    }

    internal sealed class LocalPluginContext : ILocalPluginContext
    {
        public LocalPluginContext(
            IPluginExecutionContext pluginExecutionContext,
            IOrganizationServiceFactory orgSvcFactory,
            ITracingService tracingService)
        {
            PluginExecutionContext = pluginExecutionContext;
            OrgSvcFactory = orgSvcFactory;
            TracingService = tracingService;
        }

        public IPluginExecutionContext PluginExecutionContext { get; private set; }
        public IOrganizationServiceFactory OrgSvcFactory { get; private set; }
        public ITracingService TracingService { get; private set; }
    }

    public abstract class PluginBase : IPlugin
    {
        private readonly Type _pluginType;

        protected PluginBase(Type pluginType)
        {
            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType));
            }

            _pluginType = pluginType;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var tracingService = serviceProvider.GetService(typeof(ITracingService)) as ITracingService;
            var executionContext = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            var serviceFactory = serviceProvider.GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory;

            if (tracingService == null || executionContext == null || serviceFactory == null)
            {
                throw new InvalidOperationException("The plugin execution context is missing required services.");
            }

            tracingService.Trace("Executing plugin {0}.", _pluginType.FullName);
            ExecuteDataversePlugin(new LocalPluginContext(executionContext, serviceFactory, tracingService));
        }

        protected abstract void ExecuteDataversePlugin(ILocalPluginContext localPluginContext);
    }
}
