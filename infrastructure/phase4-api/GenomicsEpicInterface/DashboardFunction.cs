using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Net;

namespace GenomicsEpicInterface
{
    public class DashboardFunction
    {
        private readonly ILogger<DashboardFunction> _logger;

        public DashboardFunction(ILogger<DashboardFunction> logger)
        {
            _logger = logger;
        }

        private static CosmosClient? _client;
        private static Container? _container;

        private Container GetContainer()
        {
            if (_container != null) return _container;
            var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
            var databaseId = Environment.GetEnvironmentVariable("COSMOS_DATABASE_ID") ?? "genomics-research-db";
            var containerId = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_ID") ?? "variants";
            _client = new CosmosClient(connectionString);
            _container = _client.GetContainer(databaseId, containerId);
            return _container;
        }

        [Function("Dashboard")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestData req)
        {
            _logger.LogInformation("Dashboard triggered: {time}", DateTime.UtcNow);

            var variants = new List<GenomicVariant>();
            try
            {
                var container = GetContainer();
                var query = new QueryDefinition("SELECT * FROM c WHERE c.clinvarSignificance = 'Pathogenic' ORDER BY c.cosmicOccurrences DESC");
                using var feed = container.GetItemQueryIterator<GenomicVariant>(query);
                while (feed.HasMoreResults)
                {
                    var batch = await feed.ReadNextAsync();
                    variants.AddRange(batch);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Cosmos DB error: {message}", ex.Message);
            }

            var rows = string.Join("\n", variants.Select(v => $@"
                <tr>
                    <td><span class='gene-badge'>{v.Gene}</span></td>
                    <td>{v.AminoAcidChange}</td>
                    <td><span class='path-badge'>Pathogenic</span></td>
                    <td>{v.OncokbLevel}</td>
                    <td>{string.Join(", ", v.Therapies)}</td>
                    <td>{v.TumorAlleleFrequency:P0}</td>
                    <td>{v.CosmicOccurrences:N0}</td>
                    <td>{v.EpicOrderId}</td>
                </tr>"));

            var html = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Genomics Research Platform</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', sans-serif; background: #0f1923; color: #e0e0e0; }}
        .header {{ background: linear-gradient(135deg, #1a3a5c, #0d2137); padding: 30px 40px; border-bottom: 2px solid #2e75b6; }}
        .header h1 {{ font-size: 26px; color: #4fc3f7; letter-spacing: 1px; }}
        .header p {{ color: #90a4ae; margin-top: 6px; font-size: 14px; }}
        .stats {{ display: flex; gap: 20px; padding: 24px 40px; background: #111e2a; }}
        .stat-card {{ background: #1a2e40; border: 1px solid #2e75b6; border-radius: 8px; padding: 16px 24px; flex: 1; }}
        .stat-card .number {{ font-size: 32px; font-weight: bold; color: #4fc3f7; }}
        .stat-card .label {{ font-size: 12px; color: #78909c; margin-top: 4px; text-transform: uppercase; letter-spacing: 1px; }}
        .content {{ padding: 24px 40px; }}
        .section-title {{ font-size: 16px; color: #4fc3f7; margin-bottom: 16px; text-transform: uppercase; letter-spacing: 1px; }}
        table {{ width: 100%; border-collapse: collapse; background: #1a2e40; border-radius: 8px; overflow: hidden; }}
        th {{ background: #0d2137; color: #4fc3f7; padding: 12px 16px; text-align: left; font-size: 12px; text-transform: uppercase; letter-spacing: 1px; }}
        td {{ padding: 12px 16px; border-bottom: 1px solid #1e3a52; font-size: 13px; }}
        tr:hover td {{ background: #1e3a52; }}
        .gene-badge {{ background: #1a3a5c; color: #4fc3f7; padding: 3px 10px; border-radius: 12px; font-weight: bold; border: 1px solid #2e75b6; }}
        .path-badge {{ background: #3e1a1a; color: #ef5350; padding: 3px 10px; border-radius: 12px; font-size: 11px; border: 1px solid #ef5350; }}
        .footer {{ padding: 20px 40px; color: #546e7a; font-size: 12px; border-top: 1px solid #1e3a52; margin-top: 24px; }}
        .live-dot {{ display: inline-block; width: 8px; height: 8px; background: #4caf50; border-radius: 50%; margin-right: 6px; animation: pulse 2s infinite; }}
        @keyframes pulse {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.4; }} 100% {{ opacity: 1; }} }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🧬 Genomics Research Platform</h1>
        <p><span class='live-dot'></span>Live data from Azure Cosmos DB &nbsp;|&nbsp; Epic EHR Integration &nbsp;|&nbsp; {DateTime.UtcNow:MMMM dd, yyyy HH:mm} UTC</p>
    </div>
    <div class='stats'>
        <div class='stat-card'><div class='number'>{variants.Count}</div><div class='label'>Pathogenic Variants</div></div>
        <div class='stat-card'><div class='number'>{variants.Select(v => v.Gene).Distinct().Count()}</div><div class='label'>Genes Tracked</div></div>
        <div class='stat-card'><div class='number'>{variants.SelectMany(v => v.Therapies).Distinct().Count()}</div><div class='label'>Targeted Therapies</div></div>
        <div class='stat-card'><div class='number'>{variants.Sum(v => v.CosmicOccurrences):N0}</div><div class='label'>Total COSMIC Cases</div></div>
    </div>
    <div class='content'>
        <div class='section-title'>Actionable Variants — Tumor Board Review</div>
        <table>
            <thead>
                <tr>
                    <th>Gene</th>
                    <th>Variant</th>
                    <th>Classification</th>
                    <th>OncoKB</th>
                    <th>Therapies</th>
                    <th>VAF</th>
                    <th>COSMIC Cases</th>
                    <th>Epic Order</th>
                </tr>
            </thead>
            <tbody>
                {rows}
            </tbody>
        </table>
    </div>
    <div class='footer'>
        Azure Genomics Research Platform &nbsp;|&nbsp; Built by Kristina Ankrah &nbsp;|&nbsp; Azure Function App + Cosmos DB + GitHub Actions CI/CD
    </div>
</body>
</html>";

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html");
            await response.WriteStringAsync(html);
            return response;
        }
    }
}