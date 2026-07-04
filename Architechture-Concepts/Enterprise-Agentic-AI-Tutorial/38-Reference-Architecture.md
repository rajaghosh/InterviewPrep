# 38 — Reference Architecture

> **Level:** Advanced | **Time to complete:** 3 hours | **Azure services:** Full enterprise Azure AI stack

---

## 1. Overview

This module presents the complete **Enterprise AI Platform Reference Architecture** — the canonical architecture for building production-grade, multi-agent AI systems on Azure. It synthesizes all prior modules into a single deployable reference.

---

## 2. Complete Enterprise AI Platform

```mermaid
graph TB
    subgraph CLIENTS["Client Applications"]
        TEAMS["Microsoft Teams\n(Bot Framework)"]
        WEB["Web Application\n(React + Azure Static Web Apps)"]
        API_CLIENT["API Client\n(3rd party systems)"]
    end

    subgraph GATEWAY["API Gateway Layer"]
        APIM["Azure API Management\n• JWT / Entra ID auth\n• Rate limiting (60 RPM/user)\n• API versioning (/v1, /v2)\n• Usage analytics\n• Semantic response cache (Redis)"]
    end

    subgraph AGENT_TIER["Agent Orchestration Tier (Container Apps)"]
        ORCH["Orchestrator Agent\n(LangGraph StateGraph)\n• Task decomposition\n• Agent routing\n• State management\n• Result synthesis"]

        AGENTS["Specialist Agents\n• Knowledge Agent (RAG)\n• Tool Agent (API calls)\n• Code Agent (code gen)\n• Data Agent (SQL/analytics)"]

        EVAL["Evaluation Service\n• Groundedness check\n• Content safety\n• PII redaction\n• Quality score"]
    end

    subgraph AI_INFRA["Azure AI Infrastructure"]
        AOAI["Azure OpenAI\n(PTU Deployment)\n• GPT-4o: orchestration, complex tasks\n• GPT-4o-mini: classification, extraction\n• text-embedding-3-large: embeddings\n• o1: reasoning tasks"]

        AISEARCH["Azure AI Search\n(Hybrid + Semantic Reranker)\n• HR Policy index\n• Product Catalog index\n• Support Knowledge index\n• Security trimming per-chunk"]

        AI_CS["Azure AI Content Safety\n• Input/output filtering\n• Hate, violence, sexual, self-harm\n• Prompt Shield (injection)"]
    end

    subgraph STATE["State and Memory Layer"]
        REDIS["Azure Cache for Redis (Premium)\n• Session memory (4h TTL)\n• Semantic query cache (1h TTL)\n• Distributed locks\n• Rate limit counters"]

        COSMOS["Azure Cosmos DB\n• Conversation history (90d)\n• Agent task state\n• LangGraph checkpoints\n• Audit log (immutable)"]

        BLOB["Azure Blob Storage\n• Original documents\n• Batch job inputs/outputs\n• Model outputs archive"]
    end

    subgraph ASYNC_TIER["Async Processing Tier"]
        SB["Azure Service Bus\n• Document ingestion queue\n• Agent task queue\n• Dead letter queue"]

        FUNC["Azure Functions / Durable Functions\n• Document processing\n• Long-running orchestrations\n• Timer-based evaluations\n• Webhook handlers"]

        ACA_JOBS["Container Apps Jobs (KEDA)\n• Batch embedding workers\n• Nightly report generation\n• Model evaluation jobs"]
    end

    subgraph INGEST["Document Ingestion Pipeline"]
        SHAREPOINT["SharePoint / Confluence\n(Source documents)"]
        ADF["Azure Data Factory\n(orchestrate ingestion)"]
        EMBED_WORKERS["Embedding Workers\n(KEDA-scaled jobs)\n• Chunk → Embed → Index"]
    end

    subgraph OBSERVABILITY["Observability Stack"]
        APPINS["Application Insights\n• Distributed traces (OpenTelemetry)\n• Request logs\n• Exception tracking"]
        LAW["Log Analytics Workspace\n• AI quality metrics\n• Token usage + cost\n• Kusto queries"]
        MONITOR["Azure Monitor\n• Alerts (latency, error rate, cost)\n• Dashboards\n• Autoscale triggers"]
    end

    subgraph SECURITY["Security Layer"]
        KV["Azure Key Vault\n(3rd party secrets only)\n(Azure services use Managed Identity)"]
        PE["Private Endpoints\n(all Azure services on private VNet)"]
        POLICIES["Azure Policy\n• Approved models only\n• Private endpoints required\n• Diagnostic settings mandatory"]
    end

    subgraph CICD["CI/CD (GitHub Actions / Azure DevOps)"]
        BUILD["Build: docker build → ACR push (SHA tag)"]
        EVAL_GATE["Prompt Eval Gate (block if < 85%)"]
        DEPLOY["Deploy: staging → approval → canary 10% → 100%"]
    end

    CLIENTS --> APIM
    APIM --> ORCH
    ORCH --> AGENTS & EVAL
    AGENTS --> AOAI & AISEARCH & AI_CS
    ORCH --> REDIS & COSMOS
    AISEARCH --> REDIS
    FUNC --> SB --> ACA_JOBS
    SHAREPOINT --> ADF --> EMBED_WORKERS --> AISEARCH & BLOB
    ORCH & AGENTS & EVAL --> APPINS
    APPINS --> LAW --> MONITOR
    KV -.-> FUNC
    PE -.-> AOAI & AISEARCH & COSMOS & REDIS
    CICD -.->|"deploy"| ORCH & AGENTS & EVAL

    classDef primary   fill:#0078d4,color:#fff,stroke:#005a9e
    classDef secondary fill:#7b2d8b,color:#fff,stroke:#5a1a6b
    classDef storage   fill:#2d7d4a,color:#fff,stroke:#1f5c35
    classDef security  fill:#c7372f,color:#fff,stroke:#a02020
    classDef monitor   fill:#e67300,color:#fff,stroke:#b35500
    classDef success   fill:#27ae60,color:#fff,stroke:#1e8449
    classDef warning   fill:#f39c12,color:#333,stroke:#d68910
    classDef neutral   fill:#f3f3f3,color:#333,stroke:#999
    classDef user      fill:#cce5ff,color:#004085,stroke:#004085
    classDef decision  fill:#fff3cd,color:#856404,stroke:#856404
    classDef highlight fill:#e74c3c,color:#fff,stroke:#c0392b

    class TEAMS user
    class WEB user
    class API_CLIENT user
    class APIM primary
    class ORCH secondary
    class AGENTS secondary
    class EVAL monitor
    class AOAI primary
    class AISEARCH primary
    class AI_CS security
    class REDIS storage
    class COSMOS storage
    class BLOB storage
    class SB primary
    class FUNC primary
    class ACA_JOBS neutral
    class SHAREPOINT neutral
    class ADF primary
    class EMBED_WORKERS secondary
    class APPINS monitor
    class LAW monitor
    class MONITOR monitor
    class KV security
    class PE security
    class POLICIES security
    class BUILD neutral
    class EVAL_GATE warning
    class DEPLOY success
```

---

## 3. Component Selection Matrix

| Tier | Component | Azure Service | Justification |
|---|---|---|---|
| API Gateway | Auth + rate limit | APIM | Enterprise-grade, 400+ connectors |
| LLM (primary) | GPT-4o | Azure OpenAI PTU | Best quality, consistent SLA |
| LLM (secondary) | GPT-4o-mini | Azure OpenAI PAYG | 33× cheaper for simple tasks |
| Embedding | text-embedding-3-large | Azure OpenAI | Best RAG quality |
| Vector search | Azure AI Search | AI Search Standard/Premium | Hybrid + semantic reranker + RBAC |
| Session state | Redis Premium | Azure Cache for Redis | <1ms access, auto-TTL |
| Durable state | Cosmos DB | Cosmos DB serverless / auto-scale | Multi-region, NoSQL, change feed |
| Async messaging | Service Bus Premium | Azure Service Bus | Guaranteed delivery, DLQ |
| Orchestration | Durable Functions | Azure Functions Premium | Long-running, external events |
| Workers | KEDA jobs | Container Apps Jobs | Scale-to-zero for batch |
| Container runtime | Container Apps | Azure Container Apps | Managed K8s, KEDA built-in |
| Observability | OpenTelemetry | App Insights + Log Analytics | Unified traces, metrics, logs |
| Content safety | Prompt Shield + filters | Azure AI Content Safety | Multi-layer defense |
| Secrets | Managed Identity | Azure Entra ID | Zero API keys in code |

---

## 4. Network Architecture

```mermaid
graph TB
    subgraph PUBLIC["Public Internet"]
        USERS["Users / Client Apps"]
    end

    subgraph AZURE["Azure (Private VNet: 10.0.0.0/16)"]
        subgraph INGRESS["DMZ Subnet (10.0.1.0/24)"]
            APIM_PE["APIM (Public Inbound)\nFirewall WAF"]
        end

        subgraph APP["App Subnet (10.0.2.0/24)"]
            CA_ENV["Container Apps Environment"]
        end

        subgraph DATA["Data Subnet (10.0.3.0/24)"]
            REDIS_PE["Redis Private Endpoint"]
            COSMOS_PE["Cosmos DB Private Endpoint"]
        end

        subgraph AI["AI Subnet (10.0.4.0/24)"]
            AOAI_PE["Azure OpenAI Private Endpoint"]
            AISEARCH_PE["AI Search Private Endpoint"]
        end
    end

    USERS --> APIM_PE
    APIM_PE --> CA_ENV
    CA_ENV --> REDIS_PE & COSMOS_PE & AOAI_PE & AISEARCH_PE

    classDef primary   fill:#0078d4,color:#fff,stroke:#005a9e
    classDef secondary fill:#7b2d8b,color:#fff,stroke:#5a1a6b
    classDef storage   fill:#2d7d4a,color:#fff,stroke:#1f5c35
    classDef security  fill:#c7372f,color:#fff,stroke:#a02020
    classDef monitor   fill:#e67300,color:#fff,stroke:#b35500
    classDef success   fill:#27ae60,color:#fff,stroke:#1e8449
    classDef warning   fill:#f39c12,color:#333,stroke:#d68910
    classDef neutral   fill:#f3f3f3,color:#333,stroke:#999
    classDef user      fill:#cce5ff,color:#004085,stroke:#004085
    classDef decision  fill:#fff3cd,color:#856404,stroke:#856404
    classDef highlight fill:#e74c3c,color:#fff,stroke:#c0392b

    class USERS user
    class APIM_PE security
    class CA_ENV primary
    class REDIS_PE storage
    class COSMOS_PE storage
    class AOAI_PE primary
    class AISEARCH_PE primary
```

---

## 5. Deployment Topology

| Environment | Scale | Cost mode | Purpose |
|---|---|---|---|
| **Development** | 0-2 replicas | PAYG, no PTU | Local testing, feature branches |
| **Staging** | 2-5 replicas | PAYG | Integration testing, QA |
| **Production (Primary)** | 5-20 replicas | PTU | Live production traffic |
| **Production (DR)** | 2-5 replicas (warm standby) | PTU (West Europe) | Failover within 2 minutes |

---

## 6. SLA Targets

| Metric | Target | Alert threshold |
|---|---|---|
| Availability | 99.9% (8.7h downtime/year) | < 99.5% in any hour |
| P50 latency | < 1.5s (with cache) | > 3s |
| P95 latency | < 4s | > 8s |
| P99 latency | < 10s | > 20s |
| Groundedness score | ≥ 4.0/5 | < 3.5 |
| Error rate | < 0.5% | > 1% |
| Cache hit rate | ≥ 30% | < 15% |
| Cost per query | < $0.05 | > $0.15 |

---

## 7. Security Posture

```mermaid
graph LR
    subgraph ZERO_TRUST["Zero Trust Architecture"]
        AUTH["Verify Explicitly\n• JWT token validation (APIM)\n• Managed Identity for service-to-service\n• RBAC: least privilege per service"]

        BREACH["Assume Breach\n• Private endpoints (no public access)\n• WAF in front of APIM\n• Audit log for all AI decisions\n• Anomaly detection (Azure Sentinel)"]

        LEAST_PRIV["Least Privilege\n• Each service has own Managed Identity\n• AI Search: Search Index Data Reader only\n• AOAI: Cognitive Services OpenAI User only\n• No wildcard permissions"]
    end

    classDef primary   fill:#0078d4,color:#fff,stroke:#005a9e
    classDef secondary fill:#7b2d8b,color:#fff,stroke:#5a1a6b
    classDef storage   fill:#2d7d4a,color:#fff,stroke:#1f5c35
    classDef security  fill:#c7372f,color:#fff,stroke:#a02020
    classDef monitor   fill:#e67300,color:#fff,stroke:#b35500
    classDef success   fill:#27ae60,color:#fff,stroke:#1e8449
    classDef warning   fill:#f39c12,color:#333,stroke:#d68910
    classDef neutral   fill:#f3f3f3,color:#333,stroke:#999
    classDef user      fill:#cce5ff,color:#004085,stroke:#004085
    classDef decision  fill:#fff3cd,color:#856404,stroke:#856404
    classDef highlight fill:#e74c3c,color:#fff,stroke:#c0392b

    class AUTH security
    class BREACH highlight
    class LEAST_PRIV security
```

---

## 8. Production Checklist

- [ ] All Azure services connected via Private Endpoints — zero public access
- [ ] Managed Identity used for all service-to-service auth — no API keys
- [ ] PTU deployment in primary region; PAYG failover in secondary region
- [ ] APIM rate limits enforced: 60 RPM/user, WAF enabled
- [ ] Disaster recovery tested: failover to West Europe in < 2 minutes
- [ ] LangGraph checkpoints in Cosmos DB: conversations survive replica restarts
- [ ] All deployments via CI/CD with prompt evaluation gate and canary rollout
- [ ] Weekly automated evaluation run; alert if quality drops > 10%

---

## 9. Interview Q&A

### Q1 (System Design): Design an enterprise AI platform that can handle 1 million conversations per day across multiple departments.

**Answer:** At 1M conversations/day = ~12 conversations/second average, with peaks at 3-5×: (1) **API tier**: APIM handles auth and rate limiting; Container Apps (auto-scale 5-100 replicas) serves as the API tier. Target P95 < 4s; (2) **LLM capacity**: 12 QPS × 4,000 tokens avg × 60 = 2.88M TPM. GPT-4o at 6K TPM/PTU = 480 PTU. Cost: 480 × $4.50/hr × 730 = $1.57M/month. Optimization: route 70% to GPT-4o-mini (reduces to ~$400K/month); (3) **Search**: Azure AI Search Standard S3 (35M docs, 36 queries/sec) with semantic reranker; (4) **State**: Redis Premium (P2, 13GB) for session state + semantic cache; Cosmos DB (autoscale, 10 regions) for durable history; (5) **Async ingestion**: KEDA workers on Container Apps Jobs, 20 parallel workers; (6) **Observability**: Application Insights + Log Analytics, alerting on P95 latency > 8s, error > 1%, cost/day > $50K; (7) **Multi-region**: primary East US, failover West Europe (warm standby); (8) **Governance**: per-department RBAC, content safety filtering, audit log for all decisions.

---

## Cross-links

- Previous: [37 — Cost Optimization](./37-Cost-Optimization.md)
- Next: [39 — End-to-End Projects](./39-End-to-End-Projects.md)
- Related: All previous modules — this is the synthesis

---

*Module 38 | Series: Enterprise Agentic AI Tutorial | Last updated: June 2026*
