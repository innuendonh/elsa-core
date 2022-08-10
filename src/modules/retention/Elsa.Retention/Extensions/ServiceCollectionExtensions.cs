using System;
using System.Collections.Generic;
using Autofac;
using Elsa.Extensions;
using Elsa.Retention.Contracts;
using Elsa.Retention.HostedServices;
using Elsa.Retention.Jobs;
using Elsa.Retention.Options;
using Elsa.Retention.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elsa.Retention.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRetentionServices(this IServiceCollection services, ContainerBuilder containerBuilder, Action<CleanupOptions> configureOptions)
        {
            services
                .Configure(configureOptions)
                .AddSingleton(CreateRetentionFilterPipeline)
                .AddScoped<CleanupJob>();

            containerBuilder.AddHostedService<CleanupHostedService>();

            return services;
        }

        private static IRetentionFilterPipeline CreateRetentionFilterPipeline(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<CleanupOptions>>().Value;
            var pipeline = ActivatorUtilities.CreateInstance<RetentionFilterPipeline>(serviceProvider);

            options.ConfigurePipeline(pipeline);
            return pipeline;
        }
    }
}