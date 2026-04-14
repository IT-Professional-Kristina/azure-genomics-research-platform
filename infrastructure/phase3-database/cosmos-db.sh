#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Phase 3 - Step 1: Azure Cosmos DB Setup
# Project: Azure Genomic Research Data Integration Platform
#
# Clinical Context:
# Cosmos DB stores structured variant metadata from the
# genomics pipeline. Three containers reflect clinical
# data separation:
#
# variants: Patient-specific somatic/germline variant calls
#   - Partition key: /sampleId
#   - Links to Epic via epicOrderId field
#   - reportedToEpic flag drives Phase 4 interface
#
# annotations: Reference databases (COSMIC, ClinVar, OncoKB)
#   - Partition key: /source
#   - Updated when databases release new versions
#
# research-cases: Research cohort coordination layer
#   - Partition key: /studyId
#   - cosmosEligible flag controls Epic Cosmos contribution
#
# Epic Cosmos Connection:
#   research-cases.cosmosEligible = true triggers
#   de-identification and contribution to Epic Cosmos
#   network (260M+ patient de-identified dataset)
#
# Region Note:
#   Central US used due to subscription quota constraints
#   Production recommendation: East US (co-locate with
#   Storage and Key Vault for minimum latency)
# ─────────────────────────────────────────────────────────────

RESOURCE_GROUP="genomics-research-rg"
LOCATION="centralus"
COSMOS_ACCOUNT="genomics-cosmos-ka"
DATABASE="genomics-research-db"
SUBSCRIPTION_ID="eab8d958-aad5-4d90-b21f-7835235870d4"

# Create Cosmos DB Account
az cosmosdb create \
  --name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --locations regionName=$LOCATION \
  --capabilities EnableServerless \
  --backup-policy-type Continuous \
  --disable-key-based-metadata-write-access true \
  --tags \
    Environment=dev \
    Project=genomics-research \
    DataClassification=PHI-Adjacent \
    ComplianceScope=HIPAA

# Create Database
az cosmosdb sql database create \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --name $DATABASE

# Create variants container
# Partition key: /sampleId
az cosmosdb sql container create \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE \
  --name variants \
  --partition-key-path "/sampleId"

# Create annotations container
# Partition key: /source
az cosmosdb sql container create \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE \
  --name annotations \
  --partition-key-path "/source"

# Create research-cases container
# Partition key: /studyId
az cosmosdb sql container create \
  --account-name $COSMOS_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE \
  --name research-cases \
  --partition-key-path "/studyId"

# Assign Cosmos DB Built-in Data Contributor role
# IMPORTANT: Run in PowerShell not Git Bash
# Git Bash corrupts forward slash in --scope parameter
# PowerShell command:
# $principalId = az ad signed-in-user show --query id --output tsv
# az cosmosdb sql role assignment create `
#   --account-name genomics-cosmos-ka `
#   --resource-group genomics-research-rg `
#   --role-definition-name "Cosmos DB Built-in Data Contributor" `
#   --principal-id $principalId `
#   --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/genomics-research-rg/providers/Microsoft.DocumentDB/databaseAccounts/genomics-cosmos-ka"

# Sample Clinical Query - Tumor Board BRAF Review:
# SELECT * FROM c
# WHERE c.gene = "BRAF"
# AND c.clinvarSignificance = "Pathogenic"
# Returns all patients with actionable BRAF mutations