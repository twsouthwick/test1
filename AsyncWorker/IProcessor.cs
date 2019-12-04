using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker
{
    public interface IProcessor
    {
        Task RunAsync(string id, CancellationToken token);
    }
}
