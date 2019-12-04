using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker
{
    public interface IStorage<T>
    {
        Task<T> GetAsync(string id, CancellationToken token);

        Task SaveAsync(string id, T item, CancellationToken token);
    }
}
