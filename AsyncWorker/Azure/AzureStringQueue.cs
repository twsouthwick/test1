using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker.Azure
{
    public class AzureStringQueue : IQueue
    {
        private readonly Task<QueueClient> _client;
        private readonly ILogger<AzureStringQueue> _logger;

        public AzureStringQueue(
            IOptions<AzureOptions> options,
            ILogger<AzureStringQueue> logger)
        {
            _client = CreateClientAsync(options.Value);
            _logger = logger;
        }

        public async Task<IQueueItem> DequeueAsync(CancellationToken token)
        {
            var client = await _client;

            while (true)
            {
                var messages = await client.ReceiveMessagesAsync(maxMessages: 1, cancellationToken: token);

                if (messages.Value.Length > 0)
                {
                    var message = messages.Value[0];

                    return new AzureQueueItem(client, message);
                }

                _logger.LogDebug("Waiting to long poll...");

                await Task.Delay(TimeSpan.FromSeconds(10), token);
            }
        }

        public async Task EnqueueAsync(string item, CancellationToken token)
        {
            var client = await _client;

            await client.SendMessageAsync(item, token);
        }

        public async Task<int> GetCountAsync(CancellationToken token)
        {
            var client = await _client;
            var properties = await client.GetPropertiesAsync(token);

            return properties.Value.ApproximateMessagesCount;
        }

        private static async Task<QueueClient> CreateClientAsync(AzureOptions options)
        {
            var client = new QueueClient(options.ConnectionString, options.QueueName);

            await client.CreateAsync();

            return client;
        }

        private class AzureQueueItem : IQueueItem
        {
            private readonly QueueClient _client;
            private readonly QueueMessage _message;

            public AzureQueueItem(QueueClient client, QueueMessage message)
            {
                _client = client;
                _message = message;
            }

            public string Message => _message.MessageText;

            public ValueTask DisposeAsync() => new ValueTask(_client.DeleteMessageAsync(_message.MessageId, _message.PopReceipt));
        }
    }
}
