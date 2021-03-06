using Autofac;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Synthesis.Logging;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Synthesis.InProductTrainingService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class InProductTrainingService : StatelessService
    {
        public const string AppRoot = "inproducttraining";

        public InProductTrainingService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var loggerFactory = InProductTrainingServiceBootstrapper.RootContainer.Resolve<ILoggerFactory>();
            return Context.CodePackageActivationContext.GetEndpoints()
                .Where(ep => ep.Protocol == EndpointProtocol.Http || ep.Protocol == EndpointProtocol.Https)
                .Select(ep =>
                    new ServiceInstanceListener(
                        sc => new OwinCommunicationListener(Startup.ConfigureApp, sc, loggerFactory, ep.Name, AppRoot),
                        ep.Protocol.ToString().ToUpper()));
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await ReportHealth(Context, cancellationToken);
        }

        private async Task ReportHealth(ServiceContext context, CancellationToken cancellationToken)
        {
            var serviceEndpoint = Context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            var protocol = serviceEndpoint.Protocol.ToString().ToLower();
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"{protocol}://localhost:{serviceEndpoint.Port}/{AppRoot}/")
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000, cancellationToken);

                try
                {
                    // Customize this route for the service
                    var response = await httpClient.GetAsync("v1/health", cancellationToken);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var info = new HealthInformation(context.NodeContext.NodeName, "InProductTraining-healthCheck", HealthState.Error)
                        {
                            Description = response.ReasonPhrase
                        };

                        Partition.ReportInstanceHealth(info);
                    }
                    else
                    {
                        var info = new HealthInformation(context.NodeContext.NodeName, "InProductTraining-healthCheck", HealthState.Ok);
                        Partition.ReportInstanceHealth(info);
                    }
                }
                catch (Exception exception)
                {
                    var info = new HealthInformation(context.NodeContext.NodeName, "InProductTraining-healthCheck", HealthState.Error)
                    {
                        Description = exception.Message
                    };

                    Partition.ReportInstanceHealth(info);
                }
            }
        }
    }
}
