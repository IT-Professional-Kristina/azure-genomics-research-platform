#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Phase 1 - Step 3: Create Storage Account and Containers
# Project: Azure Genomic Research Data Integration Platform
#
# Clinical Context:
# Three containers reflect the genomic data pipeline stages:
#
# raw-sequencing: FASTQ/BAM files - complete patient genome reads
#   - Most sensitive: contains full genomic sequence
#   - Access: pipeline service accounts only
#   - Size: 50-200GB per patient sample
#
# variant-calls: VCF files - identified genetic variants
#   - Clinically actionable: drives treatment decisions
#   - Access: pipeline write, clinician/analyst read
#   - Size: 1-50MB per patient sample
#
# cosmic-annotations: Annotated VCF - COSMIC/ClinVar enriched
#   - Epic integration source: feeds EHR genomics module
#   - Access: Epic interface read, analyst read
#   - Size: 1-100MB per patient sample
#
# Security Controls:
# - GRS replication: 6 copies across 2 regions
# - TLS 1.2 minimum: encryption in transit
# - Blob public access: disabled
# - Soft delete: 14 days (blobs and containers)
# - Point-in-time restore: 7 days
# - Blob versioning: enabled (preserves annotation history)
# - Change feed: enabled (HIPAA audit trail)
# - Infrastructure encryption: double AES-256 encryption
# - Network: restricted to authorized IPs only
#
# Production Upgrade (Phase 2):
# - Migrate from Microsoft-managed to Customer-managed keys
# - Requires User-Assigned Managed Identity with Key Vault access
# - Identity needs: Get, WrapKey, UnwrapKey permissions
# ─────────────────────────────────────────────────────────────

# Variables
RESOURCE_GROUP="genomics-research-rg"
LOCATION="eastus"
STORAGE_ACCOUNT="genomicsdataka"
KEY_VAULT="genomics-kv-ka"

# Create Storage Account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_RAGRS \
  --kind StorageV2 \
  --access-tier Hot \
  --min-tls-version TLS1_2 \
  --allow-blob-public-access false \
  --require-infrastructure-encryption true \
  --default-action Deny \
  --tags \
    Environment=dev \
    Project=genomics-research \
    DataClassification=PHI-Adjacent \
    ComplianceScope=HIPAA

# Enable data protection features
az storage account blob-service-properties update \
  --account-name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --enable-versioning true \
  --enable-change-feed true \
  --enable-delete-retention true \
  --delete-retention-days 14 \
  --enable-container-delete-retention true \
  --container-delete-retention-days 14 \
  --enable-restore-policy true \
  --restore-days 7

# Create genomic data containers
# raw-sequencing: FASTQ/BAM files - highest sensitivity
az storage container create \
  --name raw-sequencing \
  --account-name $STORAGE_ACCOUNT \
  --public-access off

# variant-calls: VCF files - clinically actionable variants
az storage container create \
  --name variant-calls \
  --account-name $STORAGE_ACCOUNT \
  --public-access off

# cosmic-annotations: COSMIC/ClinVar annotated results
# This container feeds the Epic genomics integration interface
az storage container create \
  --name cosmic-annotations \
  --account-name $STORAGE_ACCOUNT \
  --public-access off

# Verify all containers created
az storage container list \
  --account-name $STORAGE_ACCOUNT \
  --output table

# ─────────────────────────────────────────────────────────────
# PRODUCTION GAPS - Phase 2 upgrades:
# 1. Migrate to customer-managed keys (Key Vault integration)
# 2. Create User-Assigned Managed Identity for storage-keyvault auth
# 3. Replace IP firewall rules with VNet service endpoints
# 4. Enable Microsoft Defender for Storage
# 5. Configure lifecycle management policies:
#    - raw-sequencing: Hot → Cool after 30 days → Archive after 90 days
#    - variant-calls: Hot → Cool after 90 days
#    - cosmic-annotations: Hot (always active for Epic interface)
# ─────────────────────────────────────────────────────────────