# Azure Genomic Research Data Integration Platform

## Project Overview

This project simulates the backend cloud infrastructure for a 
HIPAA-compliant genomic data integration platform, designed to 
support Epic EHR interfaces in an academic research health system 
environment — modeled after real-world requirements at institutions 
like UT Health San Antonio.

It demonstrates how raw genomic sequencing data (FASTQ, BAM, VCF) 
moves from a sequencing lab through a secure cloud pipeline, gets 
annotated against clinical databases like COSMIC and ClinVar, and 
becomes available for integration with an Epic EHR system via 
standards-based interfaces.

---

## Clinical Context

When an oncology patient receives a tumor genomic panel, the 
resulting data must be:
- Stored securely with encryption at rest and in transit
- Access-controlled by role (researcher vs. clinician vs. analyst)
- Annotated against somatic mutation databases (COSMIC)
- Surfaced in Epic in a structured, workflow-integrated format

This platform addresses each of those requirements using Azure 
native services.

---

## Architecture

---

## Phases

| Phase | Focus | Status |
|-------|-------|--------|
| 1 | Secure Storage + Key Vault | 🔄 In Progress |
| 2 | Identity & Access Control (RBAC) | ⬜ Planned |
| 3 | Database Layer (Cosmos DB + SQL) | ⬜ Planned |
| 4 | API / Epic Integration Layer | ⬜ Planned |
| 5 | Monitoring & HIPAA Compliance | ⬜ Planned |

---



---

## Tech Stack

- Microsoft Azure (Blob Storage, Key Vault, Cosmos DB, 
  Azure SQL, Functions, Monitor)
- Azure CLI
- Python (Azure SDK)
- HL7 FHIR concepts
- Genomic data standards: VCF, BAM, FASTQ

---

## Author

**Kristina Ankrah**  
Healthcare IT | Azure Cloud | Epic EHR  
B.S. Information Technology — Colorado Technical University  
5+ years pharmacy operations across academic medical centers