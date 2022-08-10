using System.Threading.Tasks;
using Elsa.Extensions;
using Elsa.Multitenancy;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Samples.SwitchConsole
{
    /// <summary>
    /// Demonstrates a workflow with a While looping construct.
    /// </summary>
    static class Program
    {
        private static async Task Main()
        {
            // Create a service container with Elsa services.
            var serviceCollection = new ServiceCollection().AddElsaServices();

            var services = MultitenantContainerFactory.CreateSampleMultitenantContainer(serviceCollection,
                options => options
                    .AddConsoleActivities()
                    .AddActivitiesFrom<SimpleForkWorkflow>()
                    .AddWorkflow<GrayscaleWorkflow>());

            // Get a workflow runner.
            var workflowRunner = services.GetRequiredService<IBuildsAndStartsWorkflow>();

            // Execute the workflow.
            await workflowRunner.BuildAndStartWorkflowAsync<SimpleForkWorkflow>();
        }
    }
}