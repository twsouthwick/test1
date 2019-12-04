using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsyncWorker
{
    public class QueuedBackgroundService : BackgroundService
    {
        private readonly IQueue _queue;
        private readonly IProcessor _processor;
        private readonly ILogger<QueuedBackgroundService> _logger;

        public QueuedBackgroundService(
            IQueue queue,
            IProcessor processor,
            ILogger<QueuedBackgroundService> logger)
        {
            _queue = queue;
            _processor = processor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var item = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    using (_logger.BeginScope("Processor {Id}", item.Message))
                    {
                        await _processor.RunAsync(item.Message, stoppingToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected exception occurred during processing");
                }
                finally
                {
                    await item.DisposeAsync();
                }
            }
        }
    }
}
