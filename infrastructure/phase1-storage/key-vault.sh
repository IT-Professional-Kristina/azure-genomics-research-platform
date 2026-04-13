#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Phase 1 - Step 2: Create Key Vault and Encryption Key
# Project: Azure Genomic Research Data Integration Platform
#
# Clinical Context:
# Key Vault stores the encryption keys that protect genomic data
# at rest. Keys are physically separated from the data they 
# protect — a HIPAA technical safeguard requirement.
# Envelope encryption pattern:
#   RSA-2048 key (Key Vault) encrypts →
#   AES-256 DEK encrypts →
#   Genomic file (VCF/BAM/FASTQ)
#
# Security Notes:
# - Soft delete enabled: 90-day recovery window
# - Purge protection DISABLED in dev (enable in production)
# - Network access: All networks (restrict in production)
# - Key rotation: Annual expiration enforced
# ─────────────────────────────────────────────────────────────

# Variables
RESOURCE_GROUP="genomics-research-rg"
LOCATION="eastus"
KEY_VAULT_NAME="genomics-kv-ka"
KEY_NAME="genomics-storage-key"

# Create Key Vault
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku standard \
  --enable-soft-delete true \
  --retention-days 90 \
  --tags \
    Environment=dev \
    Project=genomics-research \
    DataClassification=PHI-Adjacent \
    ComplianceScope=HIPAA

# Generate RSA-2048 encryption key with 1-year expiration
# This key is the Key Encryption Key (KEK) in envelope encryption
# It never leaves Key Vault in plaintext
az keyvault key create \
  --vault-name $KEY_VAULT_NAME \
  --name $KEY_NAME \
  --kty RSA \
  --size 2048 \
  --ops encrypt decrypt wrapKey unwrapKey

# Verify key was created
az keyvault key show \
  --vault-name $KEY_VAULT_NAME \
  --name $KEY_NAME \
  --output table

# ─────────────────────────────────────────────────────────────
# PRODUCTION GAPS - items to address before clinical use:
# 1. Enable purge protection
# 2. Restrict network access to Selected networks / Private endpoint
# 3. Enable Microsoft Defender for Key Vault
# 4. Configure automated key rotation policy
# 5. Set up expiration alerts in Azure Monitor
# ─────────────────────────────────────────────────────────────