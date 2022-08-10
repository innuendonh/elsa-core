using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Multitenant;
using Elsa.Builders;
using Elsa.Extensions;
using Elsa.Multitenancy;
using Elsa.Options;
using Elsa.Persistence;
using Elsa.Services;
using Elsa.Services.WorkflowStorage;
using Elsa.Testing.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Elsa.Testing.Shared.Unit
{
    public abstract class WorkflowsUnitTestBase : IAsyncLifetime, IDisposable
    {
        private readonly TemporaryFolder _tempFolder;

        protected WorkflowsUnitTestBase(
            ITestOutputHelper testOutputHelper,
            Action<IServiceCollection>? configureServices = default,
            Action<ContainerBuilder>? configureContainerBuilder = default,
            Action<ElsaOptionsBuilder>? extraOptions = null)
        {
            _tempFolder = new TemporaryFolder();
            TestOutputHelper = testOutputHelper;

            var serviceCollection = new ServiceCollection().AddElsaServices();

            serviceCollection.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();
            configureServices?.Invoke(serviceCollection);

            var serviceProvider = MultitenantContainerFactory.CreateSampleMultitenantContainer(serviceCollection,
                options =>
                {
                    options.AddConsoleActivities(Console.In, new XunitConsoleForwarder(testOutputHelper));
                    extraOptions?.Invoke(options);
                },
                configureContainerBuilder);

            
            ServiceProvider = serviceProvider;
            ServiceScope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            WorkflowRunner = ServiceScope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
            WorkflowBuilderAndStarter = ServiceScope.ServiceProvider.GetRequiredService<IBuildsAndStartsWorkflow>();
            WorkflowStarter = ServiceScope.ServiceProvider.GetRequiredService<IStartsWorkflow>();
            WorkflowResumer = ServiceScope.ServiceProvider.GetRequiredService<IResumesWorkflow>();
            WorkflowBlueprintMaterializer = ServiceScope.ServiceProvider.GetRequiredService<IWorkflowBlueprintMaterializer>();
            WorkflowBuilder = ServiceScope.ServiceProvider.GetRequiredService<IWorkflowBuilder>();
            WorkflowRegistry = ServiceScope.ServiceProvider.GetRequiredService<IWorkflowRegistry>();
            BookmarkFinder = ServiceScope.ServiceProvider.GetRequiredService<IBookmarkFinder>();
            WorkflowExecutionLogStore = ServiceScope.ServiceProvider.GetRequiredService<IWorkflowExecutionLogStore>();
            WorkflowStorageService = ServiceScope.ServiceProvider.GetRequiredService<IWorkflowStorageService>();
        }

        protected ITestOutputHelper TestOutputHelper { get; }
        protected MultitenantContainer ServiceProvider { get; }
        protected IServiceScope ServiceScope { get; }
        protected IWorkflowRunner WorkflowRunner { get; }
        protected IBuildsAndStartsWorkflow WorkflowBuilderAndStarter { get; }
        protected IStartsWorkflow WorkflowStarter { get; }
        protected IResumesWorkflow WorkflowResumer { get; }
        protected IWorkflowExecutionLogStore WorkflowExecutionLogStore { get; }
        protected IWorkflowBlueprintMaterializer WorkflowBlueprintMaterializer { get; }
        protected IWorkflowBuilder WorkflowBuilder { get; }
        protected IWorkflowRegistry WorkflowRegistry { get; }
        protected IBookmarkFinder BookmarkFinder { get; }
        protected IWorkflowStorageService WorkflowStorageService { get; }
        public virtual void Dispose() => _tempFolder.Dispose();

        public virtual async Task InitializeAsync()
        {
            var startupRunner = ServiceProvider.GetRequiredService<IStartupRunner>();
            await startupRunner.StartupAsync();
        }

        public virtual async Task DisposeAsync()
        {
            await ServiceProvider.DisposeAsync();
            Dispose();
        }
    }
}