// ─────────────────────────────────────────────────────────────
// Epic Genomics Interface Function
// Project: Azure Genomic Research Data Integration Platform
//
// Clinical Context:
// Bridges genomics platform and Epic EHR via FHIR R4.
// Reads variants with reportedToEpic=false from Cosmos DB
// and formats them as FHIR R4 DiagnosticReport resources.
//
// Trigger: HTTP (dev) / Cosmos DB change feed (production)
// Runtime: .NET 8 isolated worker model
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
            HttpRequest req)
        {
            _logger.LogInformation(
                "Genomics Epic Interface triggered: {time}",
                DateTime.UtcNow);

            var variant = new GenomicVariant
            {
                Id = "S001-chr7-140453136",
                SampleId = "S001",
                PatientMrn = "MRN-789456",
                Gene = "BRAF",
                AminoAcidChange = "V600E",
                ChromosomePosition = "chr7:140453136",
                CosmicId = "COSV57014367",
                CosmicOccurrences = 75842,
                ClinvarSignificance = "Pathogenic",
                OncokbLevel = "1",
                Therapies = "Vemurafenib, Dabrafenib, Trametinib",
                TumorAlleleFrequency = 0.42,
                ReadDepth = 847,
                EpicOrderId = "ORD-2026-78456"
            };

            var conclusion =
                $"PATHOGENIC VARIANT: {variant.Gene} " +
                $"{variant.AminoAcidChange} at " +
                $"{variant.ChromosomePosition}. " +
                $"COSMIC ID: {variant.CosmicId} " +
                $"({variant.CosmicOccurrences} occurrences). " +
                $"ClinVar: {variant.ClinvarSignificance}. " +
                $"OncoKB Level {variant.OncokbLevel}. " +
                $"Therapies: {variant.Therapies}.";

            var fhirReport = new
            {
                resourceType = "DiagnosticReport",
                status = "final",
                code = new
                {
                    coding = new[]
                    {
                        new
                        {
                            system = "http://loinc.org",
                            code = "81247-9",
                            display = "Master HL7 genetic variant panel"
                        }
                    }
                },
                subject = new
                {
                    reference = $"Patient/{variant.PatientMrn}"
                },
                basedOn = new[]
                {
                    new
                    {
                        reference = $"ServiceRequest/{variant.EpicOrderId}"
                    }
                },
                conclusion = conclusion,
                result = new[]
                {
                    new { code = "48018-6", display = "Gene studied",
                          value = variant.Gene },
                    new { code = "48005-3", display = "Amino acid change",
                          value = variant.AminoAcidChange },
                    new { code = "53037-8", display = "Clinical significance",
                          value = variant.ClinvarSignificance },
                    new { code = "81258-6", display = "Allele frequency",
                          value = $"{variant.TumorAlleleFrequency:P0}" }
                }
            };

            _logger.LogInformation(
                "FHIR report built for {gene} {change} - " +
                "Order: {orderId}",
                variant.Gene,
                variant.AminoAcidChange,
                variant.EpicOrderId);

            return new OkObjectResult(new
            {
                status = "success",
                variantId = variant.Id,
                epicOrderId = variant.EpicOrderId,
                fhirReport = fhirReport,
                nextStep = "POST to Epic FHIR R4 /DiagnosticReport"
            });
        }
    }

    public class GenomicVariant
    {
        public string Id { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public string PatientMrn { get; set; } = string.Empty;
        public string Gene { get; set; } = string.Empty;
        public string AminoAcidChange { get; set; } = string.Empty;
        public string ChromosomePosition { get; set; } = string.Empty;
        public string CosmicId { get; set; } = string.Empty;
        public int CosmicOccurrences { get; set; }
        public string ClinvarSignificance { get; set; } = string.Empty;
        public string OncokbLevel { get; set; } = string.Empty;
        public string Therapies { get; set; } = string.Empty;
        public double TumorAlleleFrequency { get; set; }
        public int ReadDepth { get; set; }
        public string EpicOrderId { get; set; } = string.Empty;
    }
}