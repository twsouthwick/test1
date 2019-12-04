using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker
{
    public interface IQueue
    {
        Task<IQueueItem> DequeueAsync(CancellationToken token);

        Task EnqueueAsync(string message, CancellationToken token);

        Task<int> GetCountAsync(CancellationToken token);
    }
}
