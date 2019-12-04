using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker.InMemory
{
    public class InMemoryQueue : IQueue
    {
        private readonly ConcurrentQueue<string> _queue;
        private readonly SemaphoreSlim _signal;

        public InMemoryQueue()
        {
            _signal = new SemaphoreSlim(0);
            _queue = new ConcurrentQueue<string>();
        }

        public async Task<IQueueItem> DequeueAsync(CancellationToken token)
        {
            await _signal.WaitAsync(token);

            _queue.TryDequeue(out var message);

            return new InMemoryQueueItem(message);
        }

        public Task EnqueueAsync(string message, CancellationToken token)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _queue.Enqueue(message);
            _signal.Release();

            return Task.CompletedTask;
        }

        public Task<int> GetCountAsync(CancellationToken token) => Task.FromResult(_queue.Count);

        private class InMemoryQueueItem : IQueueItem
        {
            public InMemoryQueueItem(string message)
            {
                Message = message;
            }

            public string Message { get; }

            public ValueTask DisposeAsync() => new ValueTask();
        }
    }
}
