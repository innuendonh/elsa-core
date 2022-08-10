using Autofac;
using Autofac.Extensions.DependencyInjection;
using Elsa.Multitenancy;
using Elsa.Samples.MultiTenantChildWorker.Messages;
using Elsa.Samples.MultiTenantChildWorker.Workflows;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Elsa.Samples.MultiTenantChildWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacMultitenantServiceProviderFactory(container => MultitenantContainerFactory.CreateSampleMultitenantContainer(container)))
                .ConfigureServices((_, services) => services.AddElsaServices())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    var sc = new ServiceCollection();

                    builder
                       .ConfigureElsaServices(sc,
                            options => options
                                .AddQuartzTemporalActivities()
                                .AddConsoleActivities()
                                .AddRebusActivities<OrderReceived>()
                                .AddWorkflow<GenerateOrdersWorkflow>()
                                .AddWorkflow<OrderReceivedWorkflow>()
                                .AddWorkflow<Tenant1ChildWorkflow>()
                                .AddWorkflow<Tenant2ChildWorkflow>());

                    builder.Populate(sc);
                });
    }
}