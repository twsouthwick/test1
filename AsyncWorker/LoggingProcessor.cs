using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker
{
    public class LoggingProcessor : IProcessor
    {
        private readonly IStorage<ComputationRequest> _requests;
        private readonly IStorage<ComputationResult> _results;
        private readonly ILogger<LoggingProcessor> _logger;

        public LoggingProcessor(
            IStorage<ComputationRequest> requests,
            IStorage<ComputationResult> results,
            ILogger<LoggingProcessor> logger)
        {
            _requests = requests;
            _results = results;
            _logger = logger;
        }

        public async Task RunAsync(string id, CancellationToken token)
        {
            _logger.LogInformation("Processing item '{Id}'", id);

            var item = await _requests.GetAsync(id, token);

            if (item is null)
            {
                _logger.LogWarning("Could not find a request for claim");
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(15), token);

                await _results.SaveAsync(id, new ComputationResult { Id = $"finished: {id}" }, token);
            }
        }
    }
}
