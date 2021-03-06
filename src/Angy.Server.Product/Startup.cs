using Angy.Server.Data;
using Angy.Server.Product.GraphQL;
using Angy.Server.Product.IoC;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GraphQL.Server;
using GraphQL.Server.Ui.Altair;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Angy.Server.Product
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfiguration Configuration { get; }
        IWebHostEnvironment Environment { get; }
        public ILifetimeScope AutofacContainer { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AlbyPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
            });

            services.AddGraphQL(options =>
                {
                    options.EnableMetrics = Environment.IsDevelopment() || Environment.IsStaging();
                    options.ExposeExceptions = Environment.IsDevelopment() || Environment.IsStaging();
                })
                .AddSystemTextJson()
                .AddWebSockets()
                .AddDataLoader();

            services.AddDbContext<LuciferContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Lucifer")), ServiceLifetime.Transient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("AlbyPolicy");

            app.UseWebSockets();
            app.UseGraphQLWebSockets<Schema>();
            app.UseGraphQL<Schema>();

            if (Environment.IsDevelopment() || Environment.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseGraphQLAltair(new GraphQLAltairOptions());
            }

            AutofacContainer = app.ApplicationServices.GetAutofacRoot();
        }

        // ConfigureContainer is where you can register things directly with Autofac.
        // This runs after ConfigureServices so the things here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder) => builder.RegisterModule(new ProductModule());
    }
}