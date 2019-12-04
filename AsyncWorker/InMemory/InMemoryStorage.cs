using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker.InMemory
{
    public class InMemoryStorage<T> : IStorage<T>
        where T : class
    {
        private readonly Task<T> _null = Task.FromResult((T)null);
        private readonly ConcurrentDictionary<string, T> _items = new ConcurrentDictionary<string, T>(StringComparer.Ordinal);

        public Task<T> GetAsync(string id, CancellationToken token)
        {
            return _items.TryGetValue(id, out var result) ? Task.FromResult(result) : _null;
        }

        public Task SaveAsync(string id, T item, CancellationToken token)
        {
            _items.TryAdd(id, item);

            return Task.CompletedTask;
        }
    }
}
