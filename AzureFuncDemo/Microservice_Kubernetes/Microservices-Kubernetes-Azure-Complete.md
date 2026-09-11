# Microservices with Kubernetes on Azure — Complete Architecture Guide

> Covers: Microservice principles · Kubernetes architecture · North-South & East-West traffic · Ingress & Load Balancing · Azure AKS deployment · Service Mesh · Security

---

## Table of Contents

1. [Microservices Fundamentals](#1-microservices-fundamentals)
2. [Kubernetes Architecture Overview](#2-kubernetes-architecture-overview)
3. [Traffic Patterns: North-South vs East-West](#3-traffic-patterns-north-south-vs-east-west)
4. [Kubernetes Ingress & Load Balancing](#4-kubernetes-ingress--load-balancing)
5. [Azure as the Hyperscaler — AKS Architecture](#5-azure-as-the-hyperscaler--aks-architecture)
6. [Azure Ingress with Application Gateway (AGIC)](#6-azure-ingress-with-application-gateway-agic)
7. [Service Mesh for East-West Traffic (Istio on AKS)](#7-service-mesh-for-east-west-traffic-istio-on-aks)
8. [Security Architecture](#8-security-architecture)
9. [Scalability & Data Patterns](#9-scalability--data-patterns)
10. [Azure AKS Best Practices Summary](#10-azure-aks-best-practices-summary)
11. [Architecture Decision Reference](#11-architecture-decision-reference)

---

## 1. Microservices Fundamentals

A **microservice** is a loosely coupled, independently deployable unit of business capability, each owning its own data store and exposed through well-defined APIs.

### Core Characteristics

| Property | Description |
|---|---|
| **Single Responsibility** | Each service owns one bounded context |
| **Independent Deployability** | Deploy, scale, and update without coupling to others |
| **Decentralized Data** | Each service owns its own database (polyglot persistence) |
| **API-First** | Communication via REST, gRPC, or messaging |
| **Failure Isolation** | Circuit breakers prevent cascade failures |
| **Stateless** | State lives in external managed stores, not in-pod memory |

### Microservices vs Monolith

```
Monolith                              Microservices
┌─────────────────────────┐          ┌──────────┐ ┌──────────┐ ┌──────────┐
│  UI + Business Logic    │          │ Order Svc│ │ User Svc │ │Payment Svc│
│  + Data Access Layer    │  ─────►  ├──────────┤ ├──────────┤ ├──────────┤
│  [Single Deployable]    │          │  Own DB  │ │  Own DB  │ │  Own DB  │
└─────────────────────────┘          └──────────┘ └──────────┘ └──────────┘
```

### Communication Patterns

- **Synchronous**: REST (HTTP/HTTPS), gRPC (Protobuf over HTTP/2)
- **Asynchronous**: Message queues (Azure Service Bus, Event Hub, Kafka)
- **Service Discovery**: Kubernetes DNS (`service-name.namespace.svc.cluster.local`)

---

## 2. Kubernetes Architecture Overview

Kubernetes (K8s) is the standard container orchestration platform that AKS is built upon.

### Cluster Components

```
┌─────────────────────────── AKS Cluster ───────────────────────────┐
│                                                                     │
│  ┌──────────── Control Plane (Managed by Azure) ─────────────┐    │
│  │  API Server  │  etcd  │  Scheduler  │  Controller Manager │    │
│  └──────────────────────────────────────────────────────────-─┘    │
│                                                                     │
│  ┌────── System Node Pool ──────┐  ┌────── User Node Pool ────┐   │
│  │  CoreDNS  │  kube-proxy      │  │  App Pods               │   │
│  │  metrics-server │  Daemonsets│  │  Ingress Controller     │   │
│  └──────────────────────────────┘  └─────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

### AKS Baseline Architecture — Hub-and-Spoke Network Topology

> *Source: [Azure Architecture Center — Baseline AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)*

![AKS Baseline Architecture — hub-and-spoke network topology](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/images/aks-baseline-architecture.svg)

### Key Kubernetes Objects

| Object | Purpose |
|---|---|
| **Pod** | Smallest deployable unit — one or more containers |
| **Deployment** | Manages replica sets and rolling updates |
| **Service** | Stable network endpoint for a set of pods |
| **ConfigMap / Secret** | Externalised configuration and secrets |
| **Namespace** | Logical isolation boundary within the cluster |
| **HorizontalPodAutoscaler** | Auto-scale pods based on CPU/memory/custom metrics |
| **Ingress** | HTTP(S) routing rules for external access |
| **NetworkPolicy** | Firewall rules for pod-to-pod traffic |

### Node Pool Strategy (Azure Recommendation)

```
┌──────────────────────────────────────────┐
│            AKS Cluster                   │
│                                          │
│  ┌─────────────┐   ┌───────────────────┐ │
│  │ System Pool │   │  User Node Pools  │ │
│  │  (tainted)  │   │  ┌─────────────┐  │ │
│  │ CoreDNS     │   │  │ Frontend NP │  │ │
│  │ kube-proxy  │   │  ├─────────────┤  │ │
│  │ metrics     │   │  │ Backend NP  │  │ │
│  └─────────────┘   │  ├─────────────┤  │ │
│                    │  │ GPU NP      │  │ │
│                    │  └─────────────┘  │ │
│                    └───────────────────┘ │
└──────────────────────────────────────────┘
```

Separate system and user node pools so infrastructure components never compete with application workloads for resources.

---

## 3. Traffic Patterns: North-South vs East-West

This is the most critical architecture concept for microservices on Kubernetes. Think of the cluster as a compass map.

```
                     NORTH
                  (External Users,
                   Browsers, APIs)
                        │
                        ▼
         ┌──────────────────────────┐
WEST ◄──►│       Kubernetes         │◄──► EAST
         │  ┌────┐  ┌────┐  ┌────┐ │
         │  │ Pod│──│ Pod│──│ Pod│ │
         │  └────┘  └────┘  └────┘ │
         └──────────────────────────┘
                        │
                        ▼
                      SOUTH
                 (Databases, on-prem,
                  external services)
```

### North-South Traffic (External ↔ Cluster)

**Inbound (Northbound):** User/client requests entering the cluster
- Secured by: **API Gateway**, **Ingress Controller**, **Load Balancer**, **WAF**
- Example: `User → Azure App Gateway → Ingress → Service → Pod`

**Outbound (Southbound):** Requests leaving the cluster
- Secured by: **Egress policies**, **Azure Firewall**, **NAT Gateway**
- Example: `Pod → Azure Service Bus → External Payment API`

### East-West Traffic (Service ↔ Service inside cluster)

Internal lateral traffic between pods and services within the cluster.

```
Order Service ──────► Inventory Service
      │                      │
      ▼                      ▼
Payment Service ────► Notification Service
```

- **Not secured by default** in vanilla Kubernetes — every pod can talk to every other pod
- Secured by: **Service Mesh (mTLS)**, **NetworkPolicy**, **Zero Trust** principles
- Example: `Orders Pod → gRPC → Payments Pod → REST → Notification Pod`

### Comparison Table

| Dimension | North-South | East-West |
|---|---|---|
| **Direction** | External ↔ Cluster | Service ↔ Service (internal) |
| **Tools** | Ingress, Load Balancer, API Gateway | Service Mesh (Istio/Linkerd) |
| **Security** | Secured by default (TLS, WAF) | NOT secured by default |
| **Azure Service** | Azure App Gateway, Azure Front Door | AKS Istio Add-on |
| **Risk** | Perimeter breach | Lateral movement |
| **Protocol** | HTTP/HTTPS, TCP | HTTP, gRPC, TCP |
| **Visibility** | Easily monitored | Requires Service Mesh for observability |

### AKS Cluster Traffic Flow (North-South + East-West)

> *Source: [Azure Architecture Center — Baseline AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)*

![Diagram that shows the cluster traffic flow](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/images/traffic-flow.svg)

### Security Concern: Lateral Movement

If East-West traffic is unmonitored and a single service is compromised, an attacker can freely move across all internal services. The solution is a **Zero Trust** approach:

```
Default: DENY ALL inter-pod traffic
Then:    Explicitly ALLOW only required service-to-service paths
```

---

## 4. Kubernetes Ingress & Load Balancing

### The Problem Without Ingress

Without Ingress, you need a separate cloud Load Balancer per service — expensive and hard to manage at scale.

```
WITHOUT Ingress (expensive):              WITH Ingress (efficient):
                                          
  LB-1 ──► Service A                     Single LB ──► Ingress Controller
  LB-2 ──► Service B                                        │
  LB-3 ──► Service C                                   ┌────┴────┐
  LB-4 ──► Service D                                   ▼         ▼
                                              /api  ──► Svc A    /web ──► Svc B
  4 cloud load balancers = $$$               /auth ──► Svc C    /img ──► Svc D
```

### Ingress Resource

An `Ingress` is a Kubernetes API object that defines HTTP(S) routing rules.

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: microservices-ingress
  namespace: production
  annotations:
    kubernetes.io/ingress.class: "azure/application-gateway"
spec:
  tls:
  - hosts:
    - api.myapp.com
    secretName: tls-secret
  rules:
  - host: api.myapp.com
    http:
      paths:
      - path: /orders
        pathType: Prefix
        backend:
          service:
            name: order-service
            port:
              number: 80
      - path: /users
        pathType: Prefix
        backend:
          service:
            name: user-service
            port:
              number: 80
      - path: /payments
        pathType: Prefix
        backend:
          service:
            name: payment-service
            port:
              number: 80
```

### Ingress Controller

The Ingress resource alone does nothing — you need an **Ingress Controller** that watches Ingress resources and configures the actual load balancer.

```
┌─────────────────────────────────────────────────┐
│ Kubernetes Cluster                               │
│                                                  │
│  Ingress Resource ──watch──► Ingress Controller  │
│  (routing rules)             (nginx / AGIC)      │
│                                     │            │
│                                     ▼            │
│                              Backend Services    │
│                              (ClusterIP pods)    │
└─────────────────────────────────────────────────-┘
        ▲
        │
  External LB / Azure App Gateway
        ▲
        │
     Internet
```

### Traffic Flow (Step-by-step)

```
1. User hits:  https://api.myapp.com/orders/123
2. DNS resolves to Azure Load Balancer / App Gateway public IP
3. Azure App Gateway terminates TLS (HTTPS → HTTP)
4. App Gateway forwards to Ingress Controller pod
5. Ingress Controller reads rules: /orders → order-service
6. Routes to ClusterIP Service: order-service:80
7. kube-proxy load balances across pod replicas (Round Robin)
8. Request lands on an Order Service pod
```

### Kubernetes Service Types

| Type | Scope | Use Case |
|---|---|---|
| **ClusterIP** | Internal only | Default — pod-to-pod within cluster |
| **NodePort** | Node IP + port | Dev/test, or TCP services outside HTTP |
| **LoadBalancer** | External cloud LB | One LB per service (costly at scale) |
| **Ingress** | L7 routing | Multiple services behind one LB (recommended) |

### Load Balancing Algorithms

| Algorithm | Description | Default in K8s |
|---|---|---|
| **Round Robin** | Distribute evenly across pods | Yes (default) |
| **Least Connections** | Route to pod with fewest active conns | Via NGINX Ingress |
| **IP Hash** | Sticky sessions based on client IP | Via NGINX Ingress |
| **Weighted** | Route percentage to different versions (canary) | Via Service Mesh |

---

## 5. Azure as the Hyperscaler — AKS Architecture

### Azure Kubernetes Service (AKS) Overview

AKS is Microsoft Azure's **fully managed Kubernetes service**. Azure manages:
- The **Control Plane** (API Server, etcd, Scheduler) — free of charge
- Upgrades, patches, health monitoring of master nodes
- Integration with Azure AD, Azure Monitor, Azure Policy

You manage:
- Worker node pools (VM size, count, OS)
- Application deployments and configurations
- Network policies and ingress rules

### Microsoft Official Reference Architecture — Microservices on AKS

> *Source: [Azure Architecture Center — Microservices on AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices)*

![Diagram that shows the microservices on AKS reference architecture](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/images/microservices-architecture.svg)

### Complete AKS Reference Architecture

```
┌─────────────────────── Azure Subscription ─────────────────────────┐
│                                                                      │
│  ┌──── Azure Virtual Network (VNet) ──────────────────────────┐    │
│  │                                                             │    │
│  │  ┌─ Subnet: App Gateway ─┐   ┌─ Subnet: AKS Nodes ──────┐│    │
│  │  │                       │   │                            ││    │
│  │  │  Azure Application    │   │  ┌── System Node Pool ──┐ ││    │
│  │  │  Gateway (WAF v2)     │   │  │  CoreDNS             │ ││    │
│  │  │  + AGIC               │   │  │  kube-proxy          │ ││    │
│  │  └──────────┬────────────┘   │  └──────────────────────┘ ││    │
│  │             │                │                            ││    │
│  │             ▼                │  ┌── User Node Pool 1 ──┐  ││    │
│  │     ┌───────────────┐        │  │  Order Service Pods  │  ││    │
│  │     │ Ingress Rules │        │  │  User Service Pods   │  ││    │
│  │     └───────┬───────┘        │  └──────────────────────┘  ││    │
│  │             │                │                            ││    │
│  │             ▼                │  ┌── User Node Pool 2 ──┐  ││    │
│  │     ┌───────────────┐        │  │  Payment Service     │  ││    │
│  │     │  ClusterIP    │        │  │  Notification Svc    │  ││    │
│  │     │  Services     │        │  └──────────────────────┘  ││    │
│  │     └───────────────┘        └────────────────────────────┘│    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                                                      │
│  ┌────── Azure Managed Services ──────────────────────────────┐    │
│  │  Azure Cosmos DB │ Azure SQL │ Azure Service Bus            │    │
│  │  Azure Key Vault │ Azure Container Registry (ACR)           │    │
│  │  Azure Monitor + Log Analytics │ Microsoft Entra ID         │    │
│  └────────────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
         ▲
         │ HTTPS (North-South)
         │
    Azure Front Door / DNS
         ▲
         │
      Internet
```

### Advanced AKS — Hub-Spoke Network with Peered VNets

> *Source: [Azure Architecture Center — Advanced AKS Microservices](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices-advanced)*

![Network diagram showing a hub-spoke network with two peered virtual networks and Azure resources](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/images/aks-microservices-advanced-production-deployment.svg)

### AKS Cluster Network Topology

> *Source: [Azure Architecture Center — Baseline AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)*

![Diagram that shows the network topology of the AKS cluster](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/images/aks-baseline-network-topology.svg)

### AKS Networking Options

| Model | Description | Best For |
|---|---|---|
| **Kubenet** | Pods get IPs from an overlay network | Small clusters, simple setups |
| **Azure CNI** | Pods get real VNet IP addresses | Large clusters, VNet peering, direct pod access |
| **Azure CNI Overlay** | Pods get overlay IPs but with Azure CNI capabilities | Large clusters with IP conservation |
| **Cilium** | eBPF-based networking (fastest, security-rich) | High performance, advanced network policies |

**Azure Recommendation:** Use **Azure CNI** or **Azure CNI Overlay** for production microservices.

---

## 6. Azure Ingress with Application Gateway (AGIC)

### What is AGIC?

**Azure Application Gateway Ingress Controller (AGIC)** is the recommended Kubernetes Ingress Controller for AKS. It maps Kubernetes Ingress resources directly to Azure Application Gateway configuration.

### Architecture

```
Internet
    │
    ▼
Azure Front Door (Optional — Global CDN + WAF)
    │
    ▼
Azure Application Gateway (WAF v2)        ← L7 Load Balancer
    │   ┌─────────────────────────┐
    │   │  Web Application        │
    │   │  Firewall (WAF)         │  ← Protects against OWASP Top 10
    │   │  TLS Termination        │  ← HTTPS → HTTP inside cluster
    │   │  Path-based Routing     │  ← /api/* → api-service
    │   │  Header Rewriting       │
    │   └─────────────────────────┘
    │
    ▼
AGIC (Ingress Controller pod in AKS)
    │   Watches Kubernetes Ingress objects
    │   Programs Application Gateway rules automatically
    │
    ▼
Kubernetes Services (ClusterIP)
    │
    ▼
Microservice Pods (Order, User, Payment, Notification...)
```

### Application Gateway for Containers with Multitenant AKS

> *Source: [Azure Architecture Center — AKS with Application Gateway for Containers](https://learn.microsoft.com/en-us/azure/architecture/example-scenario/aks-agic/aks-agc)*

![Diagram showing multitenant AKS cluster with Application Gateway for Containers architecture](https://learn.microsoft.com/en-us/azure/architecture/example-scenario/aks-agic/media/aks-agc-architecture.svg)

**Per-Tenant Namespace Routing via Application Gateway for Containers:**

![Diagram of multitenant AKS architecture with per-tenant namespaces fronted by Application Gateway for Containers](https://learn.microsoft.com/en-us/azure/architecture/example-scenario/aks-agic/media/aks-agc-sample.svg)

### AGIC vs NGINX Ingress on Azure

| Feature | AGIC (App Gateway) | NGINX Ingress |
|---|---|---|
| **Type** | Azure-native, fully managed | Open-source, self-managed |
| **WAF** | Built-in WAF v2 | Requires ModSecurity add-on |
| **TLS** | Managed via Key Vault | Manual cert management |
| **Scaling** | Auto-scales (Azure managed) | Requires HPA |
| **Cost** | App Gateway pricing | Free (pay only for pod resources) |
| **Recommendation** | Production / Enterprise | Dev / Cost-sensitive |

### TLS Termination at Application Gateway

> *Source: [Azure Architecture Center — Baseline AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)*

![Diagram that shows TLS termination at Application Gateway before traffic reaches the AKS cluster](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/images/tls-termination.svg)

### Sample AGIC Deployment

```yaml
# Application Gateway Ingress with WAF and TLS via Key Vault
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: production-ingress
  annotations:
    kubernetes.io/ingress.class: azure/application-gateway
    appgw.ingress.kubernetes.io/ssl-redirect: "true"
    appgw.ingress.kubernetes.io/waf-policy-for-path: "/subscriptions/.../wafPolicy"
spec:
  tls:
  - secretName: azure-keyvault-tls-secret
  rules:
  - host: api.contoso.com
    http:
      paths:
      - path: /orders/*
        pathType: Prefix
        backend:
          service:
            name: order-service
            port:
              number: 80
      - path: /payments/*
        pathType: Prefix
        backend:
          service:
            name: payment-service
            port:
              number: 80
```

### Multi-Region with Azure Front Door

For global load balancing across multiple AKS clusters in different Azure regions:

```
Global Users
     │
     ▼
Azure Front Door (Global Anycast)
     │ Routes to nearest healthy region
     ├──► App Gateway (East US) → AKS Cluster (East US)
     ├──► App Gateway (West Europe) → AKS Cluster (West Europe)
     └──► App Gateway (Southeast Asia) → AKS Cluster (SEA)
```

#### Multi-Region AKS Cluster Architecture

> *Source: [Azure Architecture Center — AKS Baseline for Multiregion Clusters](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-multi-region/aks-multi-cluster)*

![Architecture diagram showing multiregion AKS deployment with Azure Front Door](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-multi-region/images/aks-multi-cluster.svg)

#### Multi-Region Ingress Traffic Flow

> Shows how Azure Front Door routes workload traffic across regions, with per-region Application Gateways and AKS Ingress Controllers handling failover.

![Architecture diagram showing workload traffic flow in multiregion AKS deployment](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-multi-region/images/aks-ingress-flow.svg)

---

## 7. Service Mesh for East-West Traffic (Istio on AKS)

### Why Service Mesh?

Standard Kubernetes handles North-South traffic well (via Ingress), but for East-West (service-to-service), it provides no:
- Mutual TLS (mTLS) between services
- Retries, timeouts, circuit breaking
- Traffic splitting (canary/blue-green)
- Distributed tracing
- Access control policies

A **Service Mesh** solves all of these through a **sidecar proxy** pattern.

### Istio Architecture on AKS

```
┌─────────────────── AKS Cluster ──────────────────────┐
│                                                       │
│  ┌──── Istio Control Plane ────┐                     │
│  │  istiod                     │                     │
│  │  ├── Pilot (traffic mgmt)   │                     │
│  │  ├── Citadel (certificates) │                     │
│  │  └── Galley (config)        │                     │
│  └────────────────────────────-┘                     │
│                                                       │
│  ┌─ Order Service Pod ─┐    ┌─ Payment Service Pod ─┐│
│  │  ┌──────────────┐   │    │  ┌──────────────┐     ││
│  │  │ App Container│   │    │  │ App Container│     ││
│  │  ├──────────────┤   │    │  ├──────────────┤     ││
│  │  │ Envoy Sidecar│◄──┼mTLS┼─►│ Envoy Sidecar│     ││
│  │  └──────────────┘   │    │  └──────────────┘     ││
│  └─────────────────────┘    └───────────────────────-┘│
│                                                       │
│  All East-West traffic flows through Envoy proxies   │
└───────────────────────────────────────────────────────┘
```

### AKS Managed Istio Add-on

Azure provides a **managed Istio add-on** for AKS — the recommended approach for enterprise:

```bash
# Enable Istio add-on on AKS
az aks update \
  --resource-group myRG \
  --name myAKS \
  --enable-asm

# Enable sidecar injection for a namespace
kubectl label namespace production istio-injection=enabled
```

### Key Istio Features

| Feature | Benefit |
|---|---|
| **mTLS** | Automatic mutual TLS between all services |
| **Traffic Splitting** | 90% → v1, 10% → v2 (canary deployments) |
| **Circuit Breaker** | Auto-eject unhealthy pods from load balancing |
| **Retry Policy** | Automatic retries with exponential backoff |
| **Rate Limiting** | Protect services from overload |
| **Distributed Tracing** | Jaeger/Zipkin integration via headers |
| **Observability** | Kiali dashboard for service graph visualization |

### Istio Traffic Management Example

```yaml
# VirtualService: Canary deployment — 90% stable, 10% canary
apiVersion: networking.istio.io/v1alpha3
kind: VirtualService
metadata:
  name: payment-service-routing
spec:
  hosts:
  - payment-service
  http:
  - route:
    - destination:
        host: payment-service
        subset: stable
      weight: 90
    - destination:
        host: payment-service
        subset: canary
      weight: 10
---
# DestinationRule: Circuit breaker settings
apiVersion: networking.istio.io/v1alpha3
kind: DestinationRule
metadata:
  name: payment-service-cb
spec:
  host: payment-service
  trafficPolicy:
    outlierDetection:
      consecutive5xxErrors: 3
      interval: 10s
      baseEjectionTime: 30s
```

### Ambient Mesh (Next Generation)

Istio's **Ambient Mesh** mode (now available) removes the sidecar pattern entirely, using a **node-level ztunnel proxy**. This reduces resource overhead significantly — a major consideration when running 100+ microservices.

---

## 8. Security Architecture

### Zero Trust for Microservices on AKS

```
Principle: "Never Trust, Always Verify"

External ──► WAF (App Gateway) ──► mTLS (Istio) ──► RBAC (K8s) ──► Workload Identity (Entra)
```

### Layer-by-Layer Security Model

```
┌─────────────────── Defense in Depth ───────────────────┐
│                                                         │
│  Layer 1: Network Perimeter                             │
│    Azure DDoS Protection + Azure Firewall               │
│    Azure Front Door WAF (OWASP rules)                  │
│                                                         │
│  Layer 2: Ingress Security                              │
│    App Gateway WAF v2                                   │
│    TLS Termination (certs from Azure Key Vault)         │
│    IP allowlisting / rate limiting                      │
│                                                         │
│  Layer 3: Cluster Security                              │
│    Azure AD RBAC for cluster access                     │
│    Kubernetes RBAC (ClusterRole / RoleBinding)          │
│    Pod Security Standards (restricted profile)          │
│    Azure Policy for AKS (deployment safeguards)         │
│                                                         │
│  Layer 4: Network Policies (East-West)                  │
│    Default: deny-all between namespaces                 │
│    Explicit allow: only required service paths          │
│    Kubernetes NetworkPolicy + Cilium Network Policies   │
│                                                         │
│  Layer 5: Service-to-Service (East-West mTLS)           │
│    Istio mTLS — all internal traffic encrypted          │
│    SPIFFE identity per workload                         │
│    AuthorizationPolicy (who can call whom)              │
│                                                         │
│  Layer 6: Workload Identity                             │
│    Microsoft Entra Workload ID (Federated Identity)     │
│    No stored secrets in pods — keyless access to        │
│    Azure Key Vault, Cosmos DB, Service Bus              │
│                                                         │
│  Layer 7: DevSecOps                                     │
│    Image scanning (Microsoft Defender for Containers)   │
│    SAST in CI/CD (GitHub Actions + Defender for DevOps) │
│    Admission webhooks (OPA Gatekeeper / Kyverno)        │
└─────────────────────────────────────────────────────────┘
```

### Network Policy Example (Default Deny + Explicit Allow)

```yaml
# Default deny-all for a namespace
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: default-deny-all
  namespace: production
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
---
# Allow: Order service can call Payment service
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-order-to-payment
  namespace: production
spec:
  podSelector:
    matchLabels:
      app: payment-service
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: order-service
    ports:
    - protocol: TCP
      port: 8080
```

---

## 9. Scalability & Data Patterns

### Horizontal Pod Autoscaler (HPA)

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-service
  minReplicas: 2
  maxReplicas: 20
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### KEDA — Event-Driven Autoscaling

KEDA (Kubernetes Event-Driven Autoscaler) allows scaling based on Azure Service Bus queue depth, Event Hub lag, or custom metrics:

```yaml
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: order-processor-scaler
spec:
  scaleTargetRef:
    name: order-processor
  minReplicaCount: 0
  maxReplicaCount: 50
  triggers:
  - type: azure-servicebus
    metadata:
      queueName: orders
      queueLength: "10"
```

### Recommended Azure Data Services per Microservice

| Microservice | Data Store | Azure Service |
|---|---|---|
| User Service | Relational | Azure Database for PostgreSQL Flexible |
| Order Service | Document | Azure Cosmos DB for NoSQL |
| Product Catalog | Document / Cache | Azure Cosmos DB + Azure Cache for Redis |
| Notification Service | Message Queue | Azure Service Bus |
| Analytics | Time-series / Big Data | Azure Data Explorer / Event Hub |
| Session Store | In-memory cache | Azure Cache for Redis |
| File Storage | Blob | Azure Blob Storage |
| Secrets | Key-Value | Azure Key Vault |

### Cluster Autoscaler

Automatically adds or removes nodes from the node pool based on pending pod resource requests:

```bash
az aks update \
  --resource-group myRG \
  --name myAKS \
  --enable-cluster-autoscaler \
  --min-count 2 \
  --max-count 10
```

---

## 10. Azure AKS Best Practices Summary

### Reliability

- Use **availability zones** across node pools: spread nodes across 3 AZs
- Set **Pod Disruption Budgets (PDB)** to ensure minimum replicas during maintenance
- Use **Liveness and Readiness probes** on all pods
- Deploy **multi-region** with Azure Front Door for global failover

### Performance Efficiency

- Enable **LocalDNS** (node-level DNS caching) to reduce DNS query overhead
- Use **Azure CNI Overlay** to avoid VNet IP exhaustion at scale
- Right-size pods using **Vertical Pod Autoscaler (VPA)** recommendations
- Use **KEDA** for event-driven scale-to-zero workloads

### Security

- Enable **Microsoft Defender for Containers** on the subscription
- Use **Entra Workload ID** — never store secrets in pod env vars
- Set **Pod Security Standards** to `restricted` for all production namespaces
- Enable **private cluster** mode: API server accessible only within VNet
- Rotate ACR credentials automatically via **managed identity**

### Operational Excellence

- Use **GitOps** (Flux or ArgoCD) for declarative cluster state management
- Enable **Azure Monitor + Container Insights** for metrics, logs, and dashboards
- Set **resource requests and limits** on every pod (required for HPA to work)
- Run **node image auto-upgrade** weekly in a maintenance window

### Cost Optimization

- Use **spot node pools** for batch/non-critical workloads (up to 90% savings)
- Enable **start/stop** for dev/test clusters after hours
- Use **KEDA scale-to-zero** for event-driven services during idle periods

---

## 11. Architecture Decision Reference

### When to Use What

| Scenario | Recommended Approach |
|---|---|
| Single entry point for all HTTP(S) services | Azure App Gateway + AGIC |
| Global multi-region traffic routing | Azure Front Door + App Gateway per region |
| Service-to-service security (mTLS) | AKS Istio Managed Add-on |
| Canary / Blue-Green deployments | Istio VirtualService weight-based routing |
| Autoscale on Azure queue depth | KEDA with Azure Service Bus trigger |
| Workload access to Azure resources | Microsoft Entra Workload Identity |
| Container vulnerability scanning | Microsoft Defender for Containers |
| Policy enforcement on deployments | Azure Policy for AKS + OPA Gatekeeper |
| Secrets management | Azure Key Vault + CSI Secrets Driver |
| Large-scale DNS performance | AKS LocalDNS add-on |

### Traffic Flow — End to End Summary

```
[User Browser / Mobile App]
        │  HTTPS (North-South Inbound)
        ▼
[Azure Front Door]  ←── Global WAF, CDN, Anycast routing
        │
        ▼
[Azure Application Gateway WAF v2]  ←── TLS termination, WAF rules, path routing
        │
        ▼
[AGIC — Ingress Controller (pod in AKS)]  ←── Routes to correct ClusterIP service
        │
        ▼
[ClusterIP Service]  ←── kube-proxy round-robin to healthy pods
        │
        ▼
[Target Microservice Pod]  ←── e.g. Order Service
        │  REST / gRPC (East-West)
        ▼
[Istio Envoy Sidecar → mTLS → Envoy Sidecar]
        │
        ▼
[Downstream Microservice Pod]  ←── e.g. Payment Service
        │  Async Message (South-bound)
        ▼
[Azure Service Bus]  ←── Decoupled messaging
        │
        ▼
[Notification Service]  ←── Scaled by KEDA on queue depth
```

---

## Sources

### Microsoft Azure Architecture Center — Official Diagrams

- [Microservices Architecture on AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices) — Reference architecture with official diagram
- [Advanced AKS Microservices Architecture](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices-advanced) — Hub-spoke production deployment diagram
- [Baseline AKS Cluster Architecture](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks) — Baseline network topology, traffic flow, and TLS termination diagrams
- [AKS with Application Gateway for Containers (Multitenant)](https://learn.microsoft.com/en-us/azure/architecture/example-scenario/aks-agic/aks-agc) — AGIC architecture and per-tenant namespace diagram
- [AKS Baseline for Multiregion Clusters](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-multi-region/aks-multi-cluster) — Multi-region deployment and ingress flow diagrams
- [AKS Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-kubernetes-service) — Architecture best practices
- [Istio-based Service Mesh Add-on for AKS](https://learn.microsoft.com/en-us/azure/aks/istio-about) — Managed Istio documentation

### Community & Reference

- [North-South vs East-West Traffic in Microservices — Nordic APIs](https://nordicapis.com/north-south-vs-east-west-traffic-strategies-for-microservices-and-api-communication/)
- [East-West Traffic Visualization — Creately](https://creately.com/guides/east-west-traffic/)
- [Service Mesh: Encrypt East-West Traffic in Kubernetes — Medium](https://medium.com/@nagarjoon.b/service-mesh-the-best-way-to-encrypt-east-west-traffic-in-kubernetes-89aab0c77794)
- [Kubernetes Ingress vs Load Balancer — Baeldung](https://www.baeldung.com/ops/kubernetes-ingress-vs-load-balancer)
- [Kubernetes Ingress Official Docs](https://kubernetes.io/docs/concepts/services-networking/ingress/)
- [Kubernetes Ingress & Load Balancer — ARMO Security](https://www.armosec.io/blog/kubernetes-ingress-and-load-balancer/)
- [Ingress Controllers & External Load Balancers — Traefik](https://traefik.io/blog/combining-ingress-controllers-and-external-load-balancers-with-kubernetes)
- [AKS Networking Best Practices — Medium](https://medium.com/@h.stoychev87/aks-azure-networking-and-services-best-practices-azure-caf-and-waf-2025-edition-part-3-9a342d9e2abd)
- [Azure AKS Best Practices from 100+ Microservices — DEV Community](https://dev.to/sanjaysundarmurthy/azure-aks-best-practices-from-managing-100-microservices-in-production-3j4e)
- [Kubernetes Service Mesh Ultimate Guide — Plural](https://www.plural.sh/blog/kubernetes-service-mesh-guide/)
