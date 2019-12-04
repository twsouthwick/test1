using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorker.Azure
{
    public class AzureStorage<T> : IStorage<T>
        where T : class
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();

        private readonly Task<BlobContainerClient> _client;
        private readonly ILogger<AzureStorage<T>> _logger;

        public AzureStorage(
            IOptions<AzureOptions> options,
            ILogger<AzureStorage<T>> logger)
        {
            _client = CreateClientAsync(options.Value.ConnectionString);
            _logger = logger;
        }

        public async Task<T> GetAsync(string id, CancellationToken token)
        {
            var client = await _client;
            var blobClient = client.GetBlobClient(id);

            try
            {
                using (var ms = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(ms, token);

                    ms.Position = 0;

                    using (var reader = new StreamReader(ms))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        return _serializer.Deserialize<T>(jsonReader);
                    }
                }
            }
            catch (RequestFailedException e)
            {
                _logger.LogWarning("Failed to retrieve requested item {Id}: {Message}", id, e.Message);

                return null;
            }
        }

        public async Task SaveAsync(string id, T item, CancellationToken token)
        {
            var client = await _client;

            var contents = Serialize(item);

            using (var ms = new MemoryStream(contents))
            {
                await client.UploadBlobAsync(id, ms);
            }
        }

        private static byte[] Serialize(T item)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonWriter, item);
                }

                return ms.ToArray();
            }
        }

        private static async Task<BlobContainerClient> CreateClientAsync(string connectionString)
        {
            var client = new BlobContainerClient(connectionString, typeof(T).Name.ToLowerInvariant());

            await client.CreateIfNotExistsAsync();

            return client;
        }
    }
}
