#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Phase 2 - Step 1: Managed Identity + CMK Migration
# Project: Azure Genomic Research Data Integration Platform
#
# Clinical Context:
# Managed Identity eliminates credential management for the
# Storage → Key Vault authentication chain. No passwords,
# no secrets, no certificates to manually rotate.
# Equivalent to Epic interface service account with
# certificate-based authentication — automated and auditable.
#
# Envelope Encryption Chain (now complete):
# genomics-storage-identity (Managed Identity)
#   → authenticates to genomics-kv-ka (Key Vault)
#   → uses genomics-storage-key (RSA-2048 KEK)
#   → wraps/unwraps AES-256 DEK (per-file key)
#   → protects genomic files in genomicsdataka
#
# Key Vault Permissions granted to Managed Identity:
#   - Get: retrieve key metadata
#   - Wrap Key: encrypt per-file DEK (on write)
#   - Unwrap Key: decrypt per-file DEK (on read)
#   NOT granted: Create, Delete, List, Backup, Restore
#   Principle of least privilege enforced.
# ─────────────────────────────────────────────────────────────

# Variables
RESOURCE_GROUP="genomics-research-rg"
LOCATION="eastus"
IDENTITY_NAME="genomics-storage-identity"
KEY_VAULT="genomics-kv-ka"
STORAGE_ACCOUNT="genomicsdataka"
KEY_NAME="genomics-storage-key"

# Create User-Assigned Managed Identity
az identity create \
  --name $IDENTITY_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --tags \
    Environment=dev \
    Project=genomics-research \
    DataClassification=PHI-Adjacent \
    ComplianceScope=HIPAA

# Get the identity's principal ID for role assignments
IDENTITY_PRINCIPAL_ID=$(az identity show \
  --name $IDENTITY_NAME \
  --resource-group $RESOURCE_GROUP \
  --query principalId \
  --output tsv)

# Get the identity's resource ID for storage assignment
IDENTITY_RESOURCE_ID=$(az identity show \
  --name $IDENTITY_NAME \
  --resource-group $RESOURCE_GROUP \
  --query id \
  --output tsv)

# Grant Key Vault access policy to Managed Identity
# Only Get, WrapKey, UnwrapKey — minimum required permissions
az keyvault set-policy \
  --name $KEY_VAULT \
  --object-id $IDENTITY_PRINCIPAL_ID \
  --key-permissions get wrapKey unwrapKey

# Enable purge protection on Key Vault (required for CMK)
az keyvault update \
  --name $KEY_VAULT \
  --resource-group $RESOURCE_GROUP \
  --enable-purge-protection true

# Get Key Vault URI and Key ID
KEY_VAULT_URI=$(az keyvault show \
  --name $KEY_VAULT \
  --query properties.vaultUri \
  --output tsv)

KEY_ID=$(az keyvault key show \
  --vault-name $KEY_VAULT \
  --name $KEY_NAME \
  --query key.kid \
  --output tsv)

# Assign Managed Identity to Storage Account
az storage account update \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --assign-identity $IDENTITY_RESOURCE_ID

# Migrate Storage to Customer-Managed Keys
az storage account update \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --encryption-key-source Microsoft.Keyvault \
  --encryption-key-vault $KEY_VAULT_URI \
  --encryption-key-name $KEY_NAME \
  --encryption-key-version "" \
  --key-vault-user-identity $IDENTITY_RESOURCE_ID

echo "Envelope encryption chain complete:"
echo "Storage → Managed Identity → Key Vault → RSA-2048 → AES-256 → Genomic Data"