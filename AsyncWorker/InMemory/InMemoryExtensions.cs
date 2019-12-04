using Microsoft.Extensions.DependencyInjection;
using AsyncWorker.InMemory;

namespace AsyncWorker
{
    public static class InMemoryExtensions
    {
        public static void AddInMemoryStorage(this IServiceCollection services)
        {
            services.AddSingleton<IQueue, InMemoryQueue>();
            services.AddSingleton(typeof(IStorage<>), typeof(InMemoryStorage<>));
        }
    }
}
