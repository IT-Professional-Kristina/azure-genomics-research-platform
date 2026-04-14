using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace GenomicsEpicInterface
{
    public class EpicInterfaceFunction
    {
        private readonly ILogger<EpicInterfaceFunction> _logger;

        public EpicInterfaceFunction(
            ILogger<EpicInterfaceFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessGenomicVariants")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
            HttpRequestData req)
        {
            _logger.LogInformation(
                "Genomics Epic Interface triggered: {time}",
                DateTime.UtcNow);

            var variant = new GenomicVariant
            {
                Id = "S001-chr7-140453136",
                Gene = "BRAF",
                AminoAcidChange = "V600E",
                PatientMrn = "MRN-789456",
                CosmicId = "COSV57014367",
                CosmicOccurrences = 75842,
                ClinvarSignificance = "Pathogenic",
                OncokbLevel = "1",
                Therapies = "Vemurafenib, Dabrafenib, Trametinib",
                TumorAlleleFrequency = 0.42,
                EpicOrderId = "ORD-2026-78456"
            };

            var result = new
            {
                status = "success",
                variantId = variant.Id,
                epicOrderId = variant.EpicOrderId,
                fhirReport = new
                {
                    resourceType = "DiagnosticReport",
                    status = "final",
                    subject = new
                    {
                        reference = $"Patient/{variant.PatientMrn}"
                    },
                    conclusion =
                        $"PATHOGENIC: {variant.Gene} " +
                        $"{variant.AminoAcidChange}. " +
                        $"COSMIC: {variant.CosmicId} " +
                        $"({variant.CosmicOccurrences} cases). " +
                        $"Therapies: {variant.Therapies}."
                }
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(
                JsonSerializer.Serialize(result));
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