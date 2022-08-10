using System.IO;
using System.Threading.Tasks;
using Elsa.Extensions;
using Elsa.Models;
using Elsa.Multitenancy;
using Elsa.Providers.Workflows;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Storage.Net;

namespace Elsa.Samples.FileBasedWorkflow
{
    /// <summary>
    /// Demonstrates running a workflow that is stored on disk.
    /// </summary>
    static class Program
    {
        private static async Task Main()
        {
            // The directory containing the workflow files.
            var currentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Workflows");

            // Create a service container with Elsa services.
            var serviceCollection = new ServiceCollection()
                .AddElsaServices()
                .Configure<BlobStorageWorkflowProviderOptions>(options => options.BlobStorageFactory = () => StorageFactory.Blobs.DirectoryFiles(currentDirectory));

            var services = MultitenantContainerFactory.CreateSampleMultitenantContainer(serviceCollection,
                options => options.AddConsoleActivities());

            // Get the workflow registry.
            var workflowRegistry = services.GetRequiredService<IWorkflowRegistry>();

            // Get the workflow blueprint for "SampleWorkflow" (stored in "hello-world-workflow.json", provided by StorageWorkflowProvider).
            var sampleWorkflow = await workflowRegistry.GetWorkflowAsync("SampleWorkflow", VersionOptions.Published);

            // Get a workflow runner.
            var workflowRunner = services.GetRequiredService<IStartsWorkflow>();

            // Execute the workflow.
            await workflowRunner.StartWorkflowAsync(sampleWorkflow!);
        }
    }
}