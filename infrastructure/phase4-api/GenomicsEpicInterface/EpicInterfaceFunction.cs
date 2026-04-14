// ─────────────────────────────────────────────────────────────
// Epic Genomics Interface Function
// Project: Azure Genomic Research Data Integration Platform
//
// Clinical Context:
// This function simulates the integration layer between the
// genomics platform and Epic EHR. It:
// 1. Reads variants with reportedToEpic = false from Cosmos DB
// 2. Formats them as FHIR R4 DiagnosticReport resources
// 3. Simulates pushing to Epic's FHIR endpoint
// 4. Marks variants as reportedToEpic = true
//
// In production this would connect to Epic's FHIR R4 API:
// POST /api/FHIR/R4/DiagnosticReport
//
// Trigger: HTTP trigger (dev) or Cosmos DB change feed (prod)
//
// FHIR R4 DiagnosticReport maps to Epic Genomics module:
// - result array → individual variant observations
// - conclusion → clinical interpretation summary
// - conclusionCode → SNOMED CT codes for variant significance
//
// HL7 Connection:
// Before FHIR, genomic results traveled via HL7 v2.5 ORU^R01
// messages. FHIR R4 is the modern standard Epic supports.
// Both carry the same clinical data — different packaging.
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GenomicsEpicInterface
{
    public class EpicInterfaceFunction
    {
        private readonly ILogger<EpicInterfaceFunction> _logger;

        public EpicInterfaceFunction(ILogger<EpicInterfaceFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessGenomicVariants")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] 
            HttpRequest req)
        {
            _logger.LogInformation(
                "Genomics Epic Interface triggered at: {time}", 
                DateTime.UtcNow);

            // Sample variant — in production this comes from Cosmos DB
            // query: SELECT * FROM c WHERE c.reportedToEpic = false
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
                Therapies = new[] 
                { 
                    "Vemurafenib", 
                    "Dabrafenib", 
                    "Trametinib" 
                },
                TumorAlleleFrequency = 0.42,
                ReadDepth = 847,
                EpicOrderId = "ORD-2026-78456"
            };

            // Build FHIR R4 DiagnosticReport
            var fhirReport = BuildFhirDiagnosticReport(variant);

            // Log what would be sent to Epic
            _logger.LogInformation(
                "FHIR DiagnosticReport built for variant {variantId}: " +
                "Gene={gene}, Change={change}, Significance={sig}",
                variant.Id, variant.Gene, 
                variant.AminoAcidChange, 
                variant.ClinvarSignificance);

            // In production: POST to Epic FHIR endpoint
            // var epicResponse = await PostToEpicFhir(fhirReport);
            // await MarkVariantAsReported(variant.Id);

            return new OkObjectResult(new
            {
                status = "success",
                message = "Variant processed and ready for Epic",
                variantId = variant.Id,
                epicOrderId = variant.EpicOrderId,
                fhirReport = fhirReport,
                nextStep = "POST to Epic FHIR R4 /DiagnosticReport endpoint"
            });
        }

        private FhirDiagnosticReport BuildFhirDiagnosticReport(
            GenomicVariant variant)
        {
            // FHIR R4 DiagnosticReport for genomics
            // Maps directly to Epic's Genomics module fields
            return new FhirDiagnosticReport
            {
                ResourceType = "DiagnosticReport",
                Status = "final",
                Category = new[]
                {
                    new FhirCodeableConcept
                    {
                        // LOINC code for genetics studies
                        Code = "GE",
                        Display = "Genetics"
                    }
                },
                Code = new FhirCodeableConcept
                {
                    // LOINC 81247-9: Master HL7 genetic variant
                    Code = "81247-9",
                    Display = "Master HL7 genetic variant reporting panel"
                },
                Subject = new FhirReference
                {
                    // Links to Epic patient via MRN
                    Reference = $"Patient/{variant.PatientMrn}",
                    Display = variant.PatientMrn
                },
                BasedOn = new[]
                {
                    new FhirReference
                    {
                        // Links back to originating Epic order
                        Reference = $"ServiceRequest/{variant.EpicOrderId}",
                        Display = variant.EpicOrderId
                    }
                },
                Conclusion = BuildClinicalConclusion(variant),
                ConclusionCode = new[]
                {
                    new FhirCodeableConcept
                    {
                        // SNOMED CT: Pathogenic variant
                        Code = "367493005",
                        Display = variant