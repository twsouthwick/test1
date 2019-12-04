using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AsyncWorker
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHostedService<QueuedBackgroundService>();
            services.AddSingleton<IProcessor, LoggingProcessor>();

            if (_env.IsDevelopment())
            {
                services.AddInMemoryStorage();
            }
            else
            {
                services.AddAzureServices()
                    .Bind(Configuration.GetSection("Azure"));
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/health", health =>
            {
                health.Run(ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                });
            });

            app.UseMvc();
        }
    }
}
