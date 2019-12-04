using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AsyncWorker.Azure;

namespace AsyncWorker
{
    public static class AzureExtensions
    {
        public static OptionsBuilder<AzureOptions> AddAzureServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IStorage<>), typeof(AzureStorage<>));
            services.AddSingleton<IQueue, AzureStringQueue>();

            return services.AddOptions<AzureOptions>();
        }
    }
}
