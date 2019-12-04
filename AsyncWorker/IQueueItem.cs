using System;

namespace AsyncWorker
{
    public interface IQueueItem : IAsyncDisposable
    {
        string Message { get; }
    }
}
