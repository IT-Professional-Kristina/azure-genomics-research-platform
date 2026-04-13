#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Phase 1 - Step 1: Create Resource Group
# Project: Azure Genomic Research Data Integration Platform
# Purpose: Logical container for all genomics platform resources
#
# Clinical Context:
# The resource group establishes the governance boundary for all
# PHI-adjacent genomic infrastructure. HIPAA requires knowing
# exactly where data lives — the resource group is that boundary.
#
# Azure Context:
# All resources inherit access policies and tags from this group.
# Deleting this group deletes everything inside it — useful for
# complete environment teardown in research project lifecycle.
# ─────────────────────────────────────────────────────────────

# Variables
RESOURCE_GROUP="genomics-research-rg"
LOCATION="eastus"
ENVIRONMENT="dev"
PROJECT="genomics-research"

# Create Resource Group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --tags \
    Environment=$ENVIRONMENT \
    Project=$PROJECT \
    DataClassification=PHI-Adjacent \
    ComplianceScope=HIPAA \
    CreatedBy=KristinaAnkrah

# Verify creation
az group show \
  --name $RESOURCE_GROUP \
  --output table