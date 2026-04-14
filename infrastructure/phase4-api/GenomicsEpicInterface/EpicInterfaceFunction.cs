using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace GenomicsEpicInterface
{
    public class EpicInterfaceFunction
    {
        private readonly ILogger<EpicInterfaceFunction> _logger;
        private static CosmosClient? _cosmosClient;
        private static Container? _container;

        public EpicInterfaceFunction(ILogger<EpicInterfaceFunction> logger)
        {
            _logger = logger;
        }

        private Container GetContainer()
        {
            if (_container != null) return _container;
            var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
            var databaseId = Environment.GetEnvironmentVariable("COSMOS_DATABASE_ID") ?? "genomics-research-db";
            var containerId = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_ID") ?? "variants";
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer(databaseId, containerId);
            return _container;
        }

        [Function("ProcessGenomicVariants")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
            HttpRequestData req)
        {
            _logger.LogInformation("Genomics Epic Interface triggered: {time}", DateTime.UtcNow);
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/json");
            try
            {
                var container = GetContainer();
                var query = new QueryDefinition("SELECT * FROM c WHERE c.clinvarSignificance = 'Pathogenic'");
                var variants = new List<GenomicVariant>();
                using var feed = container.GetItemQueryIterator<GenomicVariant>(query);
                while (feed.HasMoreResults)
                {
                    var batch = await feed.ReadNextAsync();
                    variants.AddRange(batch);
                }
                var result = new
                {
                    status = "success",
                    source = "CosmosDB",
                    queryTime = DateTime.UtcNow,
                    totalVariants = variants.Count,
                    variants = variants
                };
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync(JsonSerializer.Serialize(result));
            }
            catch (Exception ex)
            {
                _logger.LogError("Cosmos DB error: {message}", ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(JsonSerializer.Serialize(new
                {
                    status = "error",
                    message = ex.Message
                }));
            }
            return response;
        }
    }

    public class GenomicVariant
    {
        public string Id { get; set; } = string.Empty;
        public string Gene { get; set; } = string.Empty;
        public string AminoAcidChange { get; set; } = string.Empty;
        public string PatientMrn { get; set; } = string.Empty;
        public string CosmicId { get; set; } = string.Empty;
        public int CosmicOccurrences { get; set; }
        public string ClinvarSignificance { get; set; } = string.Empty;
        public string OncokbLevel { get; set; } = string.Empty;
        public string Therapies { get; set; } = string.Empty;
        public double TumorAlleleFrequency { get; set; }
        public string EpicOrderId { get; set; } = string.Empty;
    }
}