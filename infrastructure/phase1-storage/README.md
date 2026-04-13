# Phase 1 — Secure Genomic Data Storage

## What This Phase Builds

- Azure Resource Group (logical container for all project resources)
- Azure Key Vault (encryption key management)
- Azure Blob Storage Account (genomic file storage)
  - Container: raw-sequencing (FASTQ/BAM files)
  - Container: variant-calls (VCF files)
  - Container: cosmic-annotations (annotated results)

## Clinical Purpose

Provides HIPAA-compliant storage infrastructure for genomic 
sequencing data before it enters the Epic EHR workflow.
Encryption keys are managed separately from data — a requirement
for controlled-access genomic datasets (dbGaP, TCGA).

## Security Controls Applied

- Encryption at rest: AES-256
- Encryption in transit: TLS 1.2+
- Key management: Azure Key Vault (customer-managed keys)
- Network access: restricted to authorized services only
- Blob versioning: enabled for audit trail