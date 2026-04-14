#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Phase 2 - Step 2: RBAC Role Assignments
# Project: Azure Genomic Research Data Integration Platform
#
# Clinical Context:
# Four distinct roles reflecting real genomics research
# access patterns at an academic medical center:
#
# genomics-researcher:
#   - Runs sequencing pipeline
#   - Writes FASTQ/BAM to raw-sequencing
#   - Reads variant-calls for QC
#   - Never sees annotated clinical results
#
# genomics-analyst:
#   - Reviews variant calls for clinical significance
#   - Reads/writes variant-calls and cosmic-annotations
#   - Prepares findings for tumor board
#   - Never modifies raw sequencing data
#
# genomics-epic-interface:
#   - Automated service reading cosmic-annotations
#   - Pushes annotated results into Epic EHR
#   - Read-only, single container access
#   - No human identity — service principal
#
# genomics-compliance-auditor:
#   - Read-only access to audit logs
#   - No genomic data access
#   - Reviews change feed for HIPAA compliance
#
# HIPAA Minimum Necessary Standard:
#   Each role accesses only what their workflow requires.
# ─────────────────────────────────────────────────────────────

RESOURCE_GROUP="genomics-research-rg"
STORAGE_ACCOUNT="genomicsdataka"
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

# Storage account resource ID
STORAGE_ID="/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT"

# ── Researcher Role ──────────────────────────────────────────
# Storage Blob Data Contributor on raw-sequencing only
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id "<researcher-object-id>" \
  --scope "$STORAGE_ID/blobServices/default/containers/raw-sequencing"

# Storage Blob Data Reader on variant-calls (QC review)
az role assignment create \
  --role "Storage Blob Data Reader" \
  --assignee-object-id "<researcher-object-id>" \
  --scope "$STORAGE_ID/blobServices/default/containers/variant-calls"

# ── Analyst Role ─────────────────────────────────────────────
# Storage Blob Data Contributor on variant-calls
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id "<analyst-object-id>" \
  --scope "$STORAGE_ID/blobServices/default/containers/variant-calls"

# Storage Blob Data Contributor on cosmic-annotations
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id "<analyst-object-id>" \
  --scope "$STORAGE_ID/blobServices/default/containers/cosmic-annotations"

# ── Epic Interface Role ───────────────────────────────────────
# Storage Blob Data Reader on cosmic-annotations only
# This identity reads annotated results and pushes to Epic EHR
az role assignment create \
  --role "Storage Blob Data Reader" \
  --assignee-object-id "<epic-interface-object-id>" \
  --scope "$STORAGE_ID/blobServices/default/containers/cosmic-annotations"

# ── Compliance Auditor Role ───────────────────────────────────
# Storage Blob Data Reader at account level (audit logs only)
az role assignment create \
  --role "Storage Blob Data Reader" \
  --assignee-object-id "<auditor-object-id>" \
  --scope "$STORAGE_ID"

# ─────────────────────────────────────────────────────────────
# NOTE: Replace <*-object-id> placeholders with actual
# Azure AD object IDs when assigning to real users/services.
# Use: az ad user show --id user@domain.com --query id
# ─────────────────────────────────────────────────────────────