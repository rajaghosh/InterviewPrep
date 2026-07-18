# Microservices

---

## Introduction

Microservices is a software development style that increases the speed and efficiency of developing and managing software at scale. Each microservice:
- Lives and is deployed independently.
- Relies on other services through well-defined APIs.
- Can use different technologies (polyglot programming).

### Features

- Small, independent, loosely coupled — managed by a small team.
- Separate codebase per service.
- Independently deployable — no need to rebuild the entire app.
- Responsible for its own data persistence.
- Internal implementation hidden from other services.

---

## What Are Microservices?

**Monolithic system** — all components packed in one unit; any change requires full redeployment.

**Microservice architecture** — structures an application as a collection of services that are:
- Independently deployable.
- Loosely coupled.
- Organized around business capabilities.

---

## Typical Microservice Architecture

```
CLIENT
  ↓
API GATEWAY
  ↓
┌──────────────┐
│  SERVICE 1   │
├──────────────┤
│  SERVICE 2   │
├──────────────┤
│  SERVICE 3   │
└──────────────┘
  ↓
MANAGEMENT (Choreography / Orchestration)
```

### API Gateway
Entry point for clients — decouples clients from services.

**Advantages:**
- Services can be versioned without updating all clients.
- Can use non-web-friendly messaging protocols (e.g., AMQP).
- Handles cross-cutting concerns: authentication, logging, SSL termination, load balancing.
- Out-of-the-box policies: throttling, caching, transformation, validation.

---

## Pros and Cons of Microservices

### Pros

| Advantage | Description |
|-----------|-------------|
| **Easy Scaling** | Update and scale individual services independently |
| **Fault Tolerance** | One service failure doesn't affect others (loose coupling) |
| **Understandable Codebase** | Each module has a single focused responsibility |
| **Technology Flexibility** | Different services can use different tech stacks |
| **Independent Deployment** | Smaller codebases = faster, more frequent deployments |

### Cons

| Disadvantage | Description |
|-------------|-------------|
| **Communication Complexity** | More inter-service communication overhead |
| **More Resources** | Multiple databases, logs, and transaction management |
| **Testing Difficulty** | Each service must be launched and tested individually |
| **Not for Small Apps** | Over-engineering for smaller applications |
| **Complex Deployment** | Coordination between multiple services required |

---

## SAGA Pattern — Distributed Transaction Management

The SAGA pattern solves the problem of **distributed transactions** across multiple microservices.

**Problem:** In a distributed system, if one microservice fails mid-transaction, we need to roll back all previously completed transactions to maintain data consistency.

### Use Case: Online Restaurant Ordering

```
Order Service → Payment Service → Restaurant Service → Delivery Service
```

If any service fails, SAGA rolls back the transaction across all services.

### Types of SAGA

#### 1. Choreography (Event-Based)

- Each microservice communicates via events through a common queue (RabbitMQ, MSMQ).
- No centralized coordinator.
- Each stage has an event registered in the queue; the next service executes based on completed events.

| Pros | Cons |
|------|------|
| Simple to start — no coordinator | Complex at large scale |
| No single point of failure | Large documentation needed |
| | Integration testing is difficult |

#### 2. Orchestration (Command-Based)

- A centralized **Orchestration Service** manages all transactions and flow.
- Maintains state of each task (state machine).
- Sends rollback commands on failure.

| Pros | Cons |
|------|------|
| Easy to add new microservices | Single point of failure |
| No cyclic dependencies | Complex state management |
| Highly scalable | |

---

## Microservice Design Principles

### 1. Independent / Autonomous

Each service is deployed, scaled, and updated independently without requiring coordination.

```
Team A owns Order Service   ──▶ deploys independently
Team B owns Payment Service ──▶ deploys independently
```

- Each team owns the full lifecycle: design → build → deploy → monitor.
- Services communicate only through well-defined APIs (no shared code coupling).
- Failure in one service must not bring down others.

---

### 2. Resilient / Fault Tolerant / Design for Failure

Assume **anything can fail** — network, database, downstream services.

**Key resilience patterns:**
- **Circuit Breaker** — Stops calling a failing service; fails fast and returns a fallback.
- **Retry with backoff** — Retry transient failures with exponential delay.
- **Timeout** — Never wait indefinitely; set a deadline on all external calls.
- **Bulkhead** — Isolate failures to one section (like bulkheads on a ship).
- **Fallback** — Return cached or default data when a service is down.

```
Circuit Breaker States:
  CLOSED ──(failures exceed threshold)──▶ OPEN
  OPEN   ──(timeout expires)──▶ HALF-OPEN
  HALF-OPEN ──(test request succeeds)──▶ CLOSED
```

Library: **Polly** (.NET).

---

### 3. Observable

A microservice system is only manageable if you can see what is happening inside each service.

| Pillar | What it captures | Tools |
|--------|-----------------|-------|
| Logs | Discrete events (errors, requests) | Serilog, ELK Stack |
| Metrics | Numeric measurements over time | Prometheus, Grafana |
| Traces | Request path across services | Jaeger, Zipkin, OpenTelemetry |

```
Request: Service A → Service B → Service C
              |            |           |
        [TraceId: abc123 propagated across all hops]
```

- Structured (JSON) logs make logs queryable.
- Health endpoints (`/health`, `/ready`) enable Kubernetes probes.
- Correlation IDs link all log entries for a single request.

---

### 4. Discoverable

Services must be able to **find each other** dynamically without hardcoded addresses.

```
Service A
  ── register ──────────────────────▶ Service Registry (startup)
  ── lookup "payment-service" ───────▶
  ◀── returns 10.0.1.5:8080 ─────────
  ── call 10.0.1.5:8080 ─────────────▶ Service B
```

**Approaches:**
- **Client-side discovery** — Service queries registry + load-balances itself (Eureka + Ribbon).
- **Server-side discovery** — Load balancer queries registry (AWS ALB, Kubernetes).
- **Kubernetes DNS** — Services found via internal DNS (`http://payment-service:80`).
- **API Gateway** — Single entry point that routes to discovered services.

---

### 5. Domain Driven

Each service is aligned with a **bounded context** — a well-defined business domain.

```
E-Commerce Domain:
  ┌──────────────┐  ┌───────────────┐  ┌────────────────┐
  │ Order Service│  │Payment Service│  │ Catalog Service│
  │  (Orders BC) │  │  (Payment BC) │  │  (Product BC)  │
  └──────────────┘  └───────────────┘  └────────────────┘
```

**DDD concepts:**
- **Bounded Context** — A service owns a specific domain; its models are authoritative within that boundary.
- **Ubiquitous Language** — Domain experts and developers share the same vocabulary.
- **Aggregate** — A cluster of objects treated as a unit (e.g., Order with OrderLines).
- **Domain Events** — Something notable that happened (e.g., `OrderPlaced`, `PaymentFailed`).

---

### 6. Decentralization

No single point of control — both **governance** and **data** are decentralized.

**Data decentralization:**
- Each service owns its own database — no shared DB.
- Cross-service queries go through APIs, not direct DB joins.

```
Service A ──▶ DB-A (SQL Server)
Service B ──▶ DB-B (PostgreSQL)
Service C ──▶ DB-C (MongoDB)
```

**Governance decentralization:**
- Teams choose the right technology stack for their service (polyglot persistence).
- No centralized team dictating implementation.

Trade-off: distributed data creates eventual consistency challenges — managed with SAGA and domain events.

---

### 7. High Cohesion

Each service does **one thing well** — organized around a single business capability.

```
High Cohesion (good):
  User Service       → registration, login, profile
  Payment Service    → charge, refund, billing history
  Notification Svc   → email, SMS, push alerts

Low Cohesion (bad):
  "Utility Service"  → handles users + payments + notifications + reporting
```

- Cohesive services are smaller, easier to understand, test, and deploy.
- Mirrors the Single Responsibility Principle at the service level.
- A small team should be able to own and understand the full service.

---

### 8. Single Source of Truth

Every piece of data is **owned and authoritative** in exactly one service.

```
Who owns what:
  User Service      ──owns──▶ user profile data
  Inventory Service ──owns──▶ product stock levels
  Order Service     ──owns──▶ order state

  ❌ Two services storing the same user data → data drift
  ✓  One service owns user data; others query it via that service's API
```

- Prevents inconsistency where two services hold different versions of the same fact.
- Combined with DDD — the bounded context defines who is authoritative.
- For read performance, services may cache **projections** of foreign data, kept in sync via domain events.
