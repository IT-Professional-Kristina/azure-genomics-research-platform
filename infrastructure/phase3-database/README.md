# Phase 3 — Database Layer

## What This Phase Builds

- Azure Cosmos DB (NoSQL) — variant and annotation data
- Azure SQL Database — structured clinical research data

## Why Two Databases

### Cosmos DB (NoSQL Document Store)
Genomic variant data is schema-flexible:
- Patient A: 50,000 somatic variants, full COSMIC annotation
- Patient B: 12 germline variants, ClinVar only
- Each document has different fields, different depth

NoSQL handles this naturally — no rigid schema required.
Each variant record is a JSON document that can have
any fields without breaking other records.

### Azure SQL (Relational)
Clinical metadata is highly structured:
- Patient MRN, demographics, consent status
- Sample collection dates and pathology links
- Tumor board decisions and Epic order references
- 340B drug program eligibility flags

Relational model enforces referential integrity —
a genomic result cannot exist without a valid patient
record and sample ID linking it to Epic.

## Epic Cosmos Connection

Epic Cosmos = de-identified patient data network
(260M+ patients, 260+ health systems)

Similar hybrid architecture:
- Structured clinical data → relational tables
- Unstructured genomic/notes data → document storage
- Research queries span both layers

Understanding this architecture is core to the
UT Health SA Research, Genomics & Cosmos analyst role.

## Data Flow

```
VCF file (cosmic-annotations container)
        ↓ parsed by pipeline
Cosmos DB document (variant record)
        ↓ linked by MRN/SampleID
Azure SQL row (clinical metadata)
        ↓ queried by Epic interface
Epic Genomics module (Phase 4)
```