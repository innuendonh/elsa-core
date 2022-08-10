using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Multitenant;
using Elsa;
using Elsa.Multitenancy;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;
using Elsa.Services.Bookmarks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Elsa.Samples.ExtendHttpEndpoint
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddElsaServices()
                .AddRazorPages(); 

            services
                .AddElsaSwagger()
                .AddElsaApiEndpoints();

            // Allow arbitrary client browser apps to access the API.
            // In a production environment, make sure to allow only origins you trust.
            services.AddCors(cors => cors.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("Content-Disposition")));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // This will all go in the ROOT CONTAINER and is NOT TENANT SPECIFIC.

            // Elsa Server settings.
            var elsaSection = Configuration.GetSection("Elsa");

            var services = new ServiceCollection();

            builder.ConfigureElsaServices(services, options => options
                    .UseEntityFrameworkPersistence(ef => ef.UseSqlite())
                    .AddConsoleActivities()
                    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
                    .AddJavaScriptActivities()
                    .AddActivitiesFrom<Startup>()
                    .AddFeatures(new[] { typeof(Startup) }, Configuration)
                    .WithContainerName(elsaSection.GetSection("Server:ContainerName").Get<string>()));

            builder
                .AddNotificationHandlers(typeof(Startup))
                .AddBookmarkProvider<TestReceiveRequestBookmarkkProvider>();

            builder.Populate(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elsa"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseHttpActivities();
            app.UseEndpoints(endpoints =>
            {
                // Elsa Server uses ASP.NET Core Controllers.
                endpoints.MapControllers();

                endpoints.MapFallbackToPage("/_Host");
            });
        }

        public static MultitenantContainer ConfigureMultitenantContainer(IContainer container)
        {
            return MultitenantContainerFactory.CreateSampleMultitenantContainer(container);
        }
    }
}