# Azure Genomics Research Platform

A live, cloud-native genomics data platform built on Microsoft Azure, demonstrating serverless compute, NoSQL database integration, and automated CI/CD deployment.

**Live API Endpoint:** Available upon request (Azure Function App with key-based auth)

---

## What It Does

This platform simulates a clinical genomics interface integrated with Epic EHR workflows. It queries a live Azure Cosmos DB database of oncology genomic variants and returns structured FHIR-adjacent diagnostic data including:

- Gene mutations and amino acid changes
- ClinVar pathogenicity classifications
- COSMIC occurrence frequencies
- OncoKB actionability levels
- Targeted therapy recommendations
- Epic Order IDs for EHR integration

---

## Architecture

GitHub (source code)
↓  push to main
GitHub Actions (CI/CD)
↓  dotnet build + deploy
Azure Function App (serverless .NET 8)
↓  HTTP trigger
Azure Cosmos DB (NoSQL)
↓  returns variant data
JSON Response (FHIR-adjacent)

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Compute | Azure Function App (Serverless, .NET 8) |
| Database | Azure Cosmos DB (NoSQL, Core SQL API) |
| CI/CD | GitHub Actions |
| Language | C# / .NET 8 |
| Auth | Azure Function Keys + Cosmos DB connection string |
| Monitoring | Azure Application Insights |

---

## Sample Response

```json
{
  "status": "success",
  "source": "CosmosDB",
  "queryTime": "2026-04-14T18:02:39Z",
  "totalVariants": 6,
  "variants": [
    {
      "Gene": "BRAF",
      "AminoAcidChange": "V600E",
      "ClinvarSignificance": "Pathogenic",
      "OncokbLevel": "1",
      "Therapies": ["Vemurafenib", "Dabrafenib", "Trametinib"],
      "TumorAlleleFrequency": 0.42,
      "EpicOrderId": "ORD-2026-78456"
    }
  ]
}
```

---

## Genomic Variants in Database

| Gene | Variant | Cancer Relevance | OncoKB Level | Therapies |
|------|---------|-----------------|--------------|-----------|
| BRAF | V600E | Melanoma, CRC, NSCLC | 1 | Vemurafenib, Dabrafenib, Trametinib |
| TP53 | R248W | Pan-cancer | 1 | Pembrolizumab, Nivolumab |
| KRAS | G12D | NSCLC, CRC, Pancreatic | 2 | Sotorasib, Adagrasib |
| EGFR | L858R | NSCLC | 1 | Erlotinib, Osimertinib, Gefitinib |
| ERBB2 | V842I | Breast, Gastric | 2 | Trastuzumab, Pertuzumab, Tucatinib |
| ABL1 | T315I | CML | 1 | Ponatinib, Asciminib |

---

## CI/CD Pipeline

Automated build and deployment via GitHub Actions on every push to `main`:

- Builds .NET 8 Function App
- Deploys to Azure Function App production slot
- 12+ consecutive successful deployments

---

## Project Phases

- **Phase 1** — Azure Storage configuration
- **Phase 2** — Identity and access management (Entra ID)
- **Phase 3** — Cosmos DB setup and data modeling
- **Phase 4** — Azure Function App with HTTP trigger
- **Phase 5** — Live Cosmos DB integration with real query results

---

## Author

**Kristina Ankrah** — Healthcare IT Professional | Azure Cloud | Epic EHR

[GitHub](https://github.com/IT-Professional-Kristina) | [LinkedIn](https://www.linkedin.com/in/kristina-ankrah-7b970a282)