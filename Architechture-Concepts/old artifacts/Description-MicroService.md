# Microservices — Complete Study Guide

A deep-dive into microservices architecture from foundations to advanced patterns. Written for college students and engineers preparing for system design interviews.

---

# 1. Monolith vs. Microservices

## What is a Monolith?

A **monolithic application** is a single, large codebase where all features — user interface, business logic, and database access — are built and deployed as one unit.

**Example:** An early-stage e-commerce site where the product catalog, shopping cart, user accounts, and payments all live in a single Spring Boot application deployed as one JAR file.

**Problems with a monolith at scale:**

- **Deployment risk** — A bug in the payment module requires redeploying the entire app, including unrelated features like the catalog.
- **Scaling inefficiency** — If only the search feature is slow, you must scale the entire monolith, not just search.
- **Team bottlenecks** — Multiple teams modifying the same codebase constantly conflict with each other.
- **Technology lock-in** — Everything must use the same language and framework.
- **Slow build times** — As the codebase grows, CI/CD pipelines take longer and longer.

---

## What are Microservices?

A **microservices architecture** breaks a large application into a suite of small, independent services. Each service:
- Handles a single business function (e.g., "Order Service", "Payment Service").
- Has its own database.
- Communicates with other services over a network (HTTP/REST or message queues).
- Can be deployed, scaled, and updated independently.

**Example:** Netflix runs hundreds of microservices — one for recommendations, one for billing, one for video encoding, one for user profiles, etc. Each team owns and deploys their service independently.

---

## Key Benefits of Microservices

| Benefit | Explanation |
|---|---|
| **Scalability** | Scale only the services that need more capacity. |
| **Fault Isolation** | If the Recommendation service crashes, users can still watch videos. |
| **Independent Deployability** | Deploy a fix to the Payment service without touching anything else. |
| **Technology Diversity** | Use Python for ML, Java for transactions, Node.js for real-time features. |
| **Team Autonomy** | Each team owns their service end-to-end. |

---

# 2. Microservice Core Principles

## Single Responsibility Principle (SRP)
Each microservice should do **one thing well**. A service that handles both orders and payments is doing too much — split them.

**Rule of thumb:** If you struggle to name a service without using "and" (e.g., "OrderAndPaymentService"), it probably should be two services.

## Decentralized Data Management
Each service **owns its own database**. No two services share a database schema. This is one of the most important and counterintuitive principles.

**Why?** Shared databases create tight coupling — a schema change in a shared table breaks multiple services. Each service should be the single source of truth for its data.

## Service Autonomy
A service should be able to operate **without depending on other services being available** at the same moment. Use asynchronous messaging (Kafka) to decouple services from each other's availability.

## API-First Design
Design the service's API contract (request/response format) **before** writing any implementation code. This enables parallel development — one team builds the service while another builds the client that calls it.

---

# 3. Key Terminology

## Service Registry and Discovery
In a microservices system, services start and stop dynamically — you cannot hardcode their IP addresses.

**Service Registry** (e.g., Eureka, Consul) — A central directory where each service registers its address when it starts. Other services query the registry to find where to send requests.

**Service Discovery** — Two patterns:
- **Client-side discovery** — The calling service queries the registry itself and calls the target service directly.
- **Server-side discovery** — A load balancer or API Gateway queries the registry and routes the request on behalf of the caller.

## API Gateway
A single entry point that sits in front of all microservices. It handles:
- **Routing** — `/api/orders` → Order Service, `/api/payments` → Payment Service.
- **Authentication** — Verify JWT tokens before requests reach services.
- **Rate limiting** — Throttle abusive clients.
- **SSL termination** — Handle HTTPS at the gateway; services communicate over HTTP internally.

Popular options: AWS API Gateway, Kong, Azure API Management, Spring Cloud Gateway.

## Circuit Breaker
Prevents **cascading failures**. If Service A calls Service B and B is failing (returning errors or timing out), the circuit breaker "trips" and stops A from sending more requests to B. B is given time to recover.

See the Resilience section for detailed states (Closed, Open, Half-Open).

## Synchronous vs. Asynchronous Communication

- **Synchronous** — Service A calls Service B and *waits* for a response (HTTP/REST, gRPC). Simple but creates temporal coupling — if B is slow, A is blocked.
- **Asynchronous** — Service A publishes an event to a message queue and *does not wait*. Service B processes it whenever it is ready. More complex but much more resilient.

## Eventual Consistency
In a distributed system, all copies of data **will eventually become consistent** — but not necessarily at the exact same moment. This is an accepted trade-off for availability and performance.

**Example:** When you post on Instagram, your post might appear immediately to you but take a few seconds to appear to followers in other regions. The system is eventually consistent.

---

# 4. Architecture Patterns

## Database per Service

**The Pattern:** Every microservice has its own private database. No other service can directly query it.

**Why it matters:** Enforces loose coupling. The Order Service can switch from MySQL to MongoDB without affecting any other team.

**The challenge:** Queries that would be a simple SQL JOIN in a monolith now require coordinating across multiple services (API Composition or CQRS).

---

## API Gateway Pattern

**The Pattern:** A reverse proxy that is the single entry point for all external client traffic.

```
Mobile App  ──────┐
Web Browser ──────┤──> [API Gateway] ──> User Service
3rd Party   ──────┘             |──────> Product Service
                                └──────> Order Service
```

**Responsibilities:**
- Authentication and authorization.
- Request routing.
- Rate limiting and throttling.
- Response caching.
- Request/response transformation (e.g., gRPC to JSON).
- Logging and monitoring.

---

## Backend for Frontend (BFF)

**The Problem:** A single API Gateway serves both mobile apps (which need small payloads) and web apps (which need rich data). The gateway becomes bloated trying to serve everyone.

**The Pattern:** Create a **dedicated backend** for each type of client:
- **Web BFF** — Aggregates data from multiple services in the format the web app needs.
- **Mobile BFF** — Returns a lighter payload optimized for mobile bandwidth.

```
Web App  ──> [Web BFF]    ──> Product Service
                      └──> Review Service
Mobile App ──> [Mobile BFF] ──> Product Service
```

Netflix uses this pattern — each platform (Android, iOS, TV, Web) has its own backend layer.

---

## Service Mesh

**The Problem:** Cross-cutting concerns like retries, timeouts, mTLS encryption, and observability need to be implemented in every service.

**The Pattern:** A **service mesh** (like Istio or Linkerd) injects a lightweight **sidecar proxy** (like Envoy) alongside each service container. The proxy handles all network communication transparently.

```
Service A ──> [Envoy Sidecar] ──network──> [Envoy Sidecar] ──> Service B
```

The service mesh handles:
- mTLS (mutual TLS) between all services automatically.
- Retries and circuit breaking at the network layer.
- Distributed tracing.
- Traffic shaping (canary deployments, A/B testing).

---

## CQRS (Command Query Responsibility Segregation)

**The Pattern:** Separate the **write model** (Commands — create, update, delete) from the **read model** (Queries — fetch data).

**Why?** Write operations are transactional and need consistency. Read operations are often much higher volume and benefit from denormalized, read-optimized data stores.

```
Client ──> [Command Handler] ──> Write Database (normalized, transactional)
                            ──> publishes event ──> [Read Model Updater]
Client ──> [Query Handler]  ──> Read Database (denormalized, optimized for queries)
```

**Example:** An order system where writes go to a normalized SQL database, and reads are served from a pre-computed, denormalized Elasticsearch index for fast search.

---

## Event Sourcing

**The Pattern:** Instead of storing the current state of an entity, store the **full history of events** that led to that state.

**Normal approach:** Store the current balance in a `users` table column.

**Event Sourcing approach:** Store every deposit and withdrawal event. The current balance is computed by replaying all events.

**Benefits:**
- Complete audit trail.
- Can reconstruct state at any point in time.
- Naturally integrates with CQRS.

**Trade-offs:**
- Querying current state requires replaying events (mitigated by snapshots).
- More complex to implement than simple CRUD.

---

# 5. Communication Styles

## Synchronous: REST (HTTP)

**When to use:** When the caller needs the result immediately to proceed (e.g., a user submits a payment and needs a success/failure response).

**Tools:** Spring Boot REST controllers, HTTP clients.

**Trade-offs:**
- Simple and familiar.
- Creates temporal coupling — if the downstream service is slow or down, the caller is blocked.

---

## Synchronous: gRPC

**What it is:** A high-performance RPC framework from Google. Uses Protocol Buffers (binary format) over HTTP/2.

**When to use:** High-throughput inter-service communication where latency matters (microservice to microservice calls in the same data center).

**Advantages over REST:**
- Binary protocol — much smaller payload, faster serialization.
- Strongly typed contracts (`.proto` files).
- Supports bidirectional streaming.

---

## Asynchronous: Kafka / RabbitMQ / EventBus

**When to use:**
- When you do not need an immediate response (fire and forget).
- When you want to decouple services from each other's availability.
- For event-driven workflows (e.g., "Order Placed" triggers Inventory, Billing, and Shipping services independently).

**Tools:** Apache Kafka (high-throughput streaming), RabbitMQ (complex routing), AWS SQS/SNS (managed cloud queues).

---

## Decision Matrix: Sync vs. Async

| Scenario | Recommended Pattern |
|---|---|
| User submits a form and needs a result | Synchronous REST |
| One service needs to notify many others | Async Pub/Sub (Kafka/SNS) |
| Long-running background job | Async Message Queue |
| High-performance internal service calls | gRPC |
| Real-time bidirectional updates | WebSockets |

---

# 6. Data Management in Microservices

## Why Shared Databases Are Bad

If two services share a database:
- A schema change by Team A breaks Team B's service.
- Teams become tightly coupled — they must coordinate every migration.
- You lose the ability to scale or replace services independently.
- The shared database becomes a bottleneck.

**The rule:** Each microservice owns its data. Other services access that data only through the service's API — never directly via SQL.

---

## Managing Data Consistency

With separate databases, you lose the ability to use a single SQL transaction across services. Instead, use these patterns:

### SAGA Pattern

A **SAGA** is a sequence of local transactions, each in a different service. If one transaction fails, **compensating transactions** undo the previous steps.

**Example — Order Placement Saga:**
1. Order Service creates the order (status: PENDING).
2. Inventory Service reserves the items.
3. Payment Service charges the credit card.
4. Order Service updates status to CONFIRMED.

If step 3 (payment) fails:
- Compensating transaction in Inventory Service releases the reserved items.
- Compensating transaction in Order Service marks the order as FAILED.

**Two implementation styles:**

**Choreography** — Each service publishes events and listens to events from other services. No central coordinator.
```
Order Service publishes "OrderCreated"
    -> Inventory Service listens, reserves items, publishes "ItemsReserved"
    -> Payment Service listens, charges card, publishes "PaymentProcessed"
    -> Order Service listens, marks order CONFIRMED
```
Pros: Decentralized, no single point of failure.
Cons: Hard to understand the full workflow; debugging is difficult.

**Orchestration** — A central **Orchestrator Service** directs each step and handles failures.
```
Orchestrator -> calls Inventory Service -> calls Payment Service -> updates Order Service
```
Pros: Clear, centralized workflow visibility.
Cons: The orchestrator becomes a critical dependency.

---

### Outbox Pattern

**The Problem:** When a service writes to its database and publishes an event to Kafka, these two operations are not atomic. If the service crashes after the DB write but before the Kafka publish, the event is lost.

**The Solution — Outbox Pattern:**
1. Write the database change AND an outbox record in a single local transaction.
2. A separate "outbox reader" process polls the outbox table and publishes events to Kafka.
3. Once published, the outbox record is deleted or marked as sent.

This guarantees **at-least-once delivery** — the event will always be published, even if the service crashes.

---

### Two-Phase Commit (2PC) — Why It Is Avoided

2PC is a distributed transaction protocol where a coordinator tells all participants to prepare, then tells them all to commit.

**Problems:**
- **Blocking** — If the coordinator crashes, all participants are stuck in a prepared state indefinitely.
- **Performance** — Multiple round trips between services add significant latency.
- **Tight coupling** — All participating services must be available simultaneously.

In microservices, the SAGA pattern is strongly preferred over 2PC.

---

# 7. Security in Microservices

## OAuth 2.0 and OpenID Connect

**OAuth 2.0** is an *authorization* framework. It allows users to grant third-party apps limited access to their resources without sharing their password.

**OpenID Connect (OIDC)** is an *authentication* layer built on top of OAuth 2.0. It adds a standardized way to verify the user's identity.

**Flow (Authorization Code Grant):**
1. User clicks "Login with Google."
2. Browser redirects to Google's authorization server.
3. User logs in to Google and grants permission.
4. Google redirects back with an authorization code.
5. Your server exchanges the code for an access token and ID token.
6. Your server validates the ID token to confirm the user's identity.

---

## JWT (JSON Web Token)

A **JWT** is a compact, self-contained token that encodes user information (claims). It has three parts:

```
Header.Payload.Signature
eyJhbGci... . eyJ1c2VySWQi... . SflKxwRJSMeKKF2QT4...
```

- **Header** — Algorithm used for signing (e.g., RS256).
- **Payload** — Claims: userId, roles, expiration time.
- **Signature** — Verifies the token was not tampered with.

**How microservices use JWTs:**
1. The API Gateway validates the JWT on every incoming request.
2. Valid requests are forwarded to services with the user's identity in a header.
3. Services trust the gateway's assertion — they do not re-validate the JWT.

**Security best practices:**
- Never store PII (Personally Identifiable Information) in the JWT payload — it is only Base64-encoded, not encrypted.
- Use short expiry times for access tokens (15 minutes).
- Use refresh tokens for session persistence.
- Always verify the signature using the public key, not just decode the payload.

---

## API Gateway as Security Layer

The API Gateway is the **single choke point** for all incoming traffic. Centralize security here:
- JWT/OAuth token validation.
- Rate limiting per client.
- IP allowlist/blocklist.
- Request validation (reject malformed inputs early).

This prevents security logic from being duplicated across every service.

---

## RBAC (Role-Based Access Control)

Assign users to roles (Admin, Editor, Viewer). Each role has a defined set of permissions.

**In JWTs:** Include the user's roles in the token payload. The API Gateway or individual services check roles before allowing access.

---

# 8. Observability and Monitoring

## The 3 Pillars of Observability

**Logs** — Record of what happened, with timestamps.
**Metrics** — Numeric measurements over time (requests/second, error rate, CPU usage).
**Traces** — End-to-end tracking of a single request as it flows through multiple services.

Without all three, you are flying blind in production.

---

## Correlation IDs

**The Problem:** A user reports an error. You have logs from 10 different services. How do you find all the logs related to that specific user's request?

**The Solution:** Generate a unique **Correlation ID** (a UUID) for every incoming request at the API Gateway. Propagate it in the HTTP headers to every downstream service call. Include it in every log entry.

```
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000
```

Now you can search all your logs for that Correlation ID and see the full request journey.

---

## Structured Logging

Instead of logging plain text:
```
ERROR: Failed to process order 12345
```

Log as structured JSON:
```json
{
  "timestamp": "2026-07-31T10:22:01Z",
  "level": "ERROR",
  "service": "order-service",
  "correlationId": "550e8400-...",
  "orderId": "12345",
  "message": "Failed to process order",
  "error": "PaymentService timeout after 5000ms"
}
```

Structured logs can be queried like a database (e.g., in Elasticsearch/Kibana).

**Tools:** Logback + Logstash encoder (Java), ELK Stack (Elasticsearch, Logstash, Kibana).

---

## Distributed Tracing

**Zipkin** and **Jaeger** are open-source distributed tracing systems. They collect trace data from all services and visualize the complete journey of a request as a timeline (called a "flame graph").

```
Gateway (5ms) ──> Order Service (12ms) ──> Inventory Service (45ms) ← SLOW
                                      └──> Notification Service (3ms)
Total: 65ms
```

This immediately shows that the Inventory Service is the bottleneck.

---

## Metrics with Prometheus and Grafana

**Prometheus** scrapes metric endpoints from your services (e.g., `/actuator/prometheus` in Spring Boot) and stores time-series data.

**Grafana** visualizes those metrics in dashboards: request rates, error rates, latency percentiles (P50, P95, P99), CPU/memory usage.

**Spring Boot Actuator** exposes application health and metrics endpoints automatically — a single dependency enables Prometheus scraping.

---

# 9. Resilience and Fault Tolerance

## Circuit Breaker (Resilience4j)

Three states:

| State | Behavior |
|---|---|
| **Closed** | Requests flow normally. Failure rate is monitored. |
| **Open** | Circuit is tripped. Requests fail immediately (no network call). Timer starts. |
| **Half-Open** | A small number of test requests are allowed through. If they succeed, circuit closes. If they fail, circuit reopens. |

**Configuration example:**
- Open circuit if error rate exceeds 50% in a 10-request sliding window.
- Wait 10 seconds in Open state before allowing test requests.
- Close circuit after 5 consecutive successful test requests.

---

## Retry and Timeout

**Timeout** — Every outbound call must have a timeout. Without it, a slow downstream service holds your threads indefinitely, eventually exhausting your thread pool.

**Retry** — Automatically retry transient failures (network glitch, brief service outage).

**Exponential Backoff with Jitter:**
- Retry 1: wait 1 second.
- Retry 2: wait 2 seconds.
- Retry 3: wait 4 seconds.
- Add random jitter (±500ms) to prevent synchronized retries from all callers.

---

## Bulkhead Pattern

**The Problem:** Service A calls both Service B and Service C using a shared thread pool. Service B is slow and consumes all threads. Now Service C calls from Service A also fail — even though Service C is healthy.

**The Solution:** Isolate resources (thread pools, semaphores) per dependency. Service B and Service C each get their own pool. A slowdown in B cannot starve C.

---

## Rate Limiting

Limit the number of requests a client can make in a time window.

**Algorithms:**
- **Token Bucket** — A bucket refills with tokens at a fixed rate. Each request consumes a token. Allows bursts.
- **Leaky Bucket** — Requests enter a queue and are processed at a fixed rate. Smooths out bursts.
- **Fixed Window** — Count requests per fixed time window (simple but has boundary exploitation issues).
- **Sliding Window** — Count requests in a rolling time window (more accurate).

**Tools:** Resilience4j RateLimiter, Bucket4j (Java), Istio (service mesh).

---

# 10. Deployment Strategies

## Containerization with Docker

Each microservice is packaged as a **Docker image** — a portable, self-contained unit that includes the application and all its dependencies.

**Dockerfile (Spring Boot example):**
```dockerfile
FROM eclipse-temurin:21-jre
COPY target/order-service.jar /app/order-service.jar
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "/app/order-service.jar"]
```

Build and run:
```bash
docker build -t order-service:1.0 .
docker run -p 8080:8080 order-service:1.0
```

**Docker Compose** lets you define and run multiple containers together for local development:
```yaml
services:
  order-service:
    image: order-service:1.0
    ports: ["8080:8080"]
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: orders
```

---

## Kubernetes (K8s)

**Kubernetes** is the container orchestration platform that manages deploying, scaling, and operating Docker containers in production.

Key concepts:
- **Pod** — The smallest deployable unit. Wraps one or more containers.
- **Deployment** — Manages a set of identical pods; handles rolling updates.
- **Service** — A stable DNS name and load balancer for a set of pods.
- **Ingress** — Routes external HTTP/HTTPS traffic to services.
- **ConfigMap / Secret** — Inject configuration and secrets into pods.
- **HPA (Horizontal Pod Autoscaler)** — Automatically scales the number of pods based on CPU/memory metrics.

---

## Blue-Green Deployment

Run two identical production environments — **Blue** (current version) and **Green** (new version). When Green is tested and ready, switch all traffic from Blue to Green instantly. If something goes wrong, switch back.

**Benefit:** Zero-downtime deployments with instant rollback.

---

## Canary Deployment

Gradually shift a small percentage of traffic to the new version:
- 5% of users get v2, 95% get v1.
- Monitor error rates and latency.
- Gradually increase to 100% if metrics look good.
- Roll back instantly if metrics degrade.

**Tools:** Kubernetes with Argo Rollouts, Istio traffic weights, AWS CodeDeploy.

---

# Quick Reference

## Microservices Design Checklist

- [ ] Does each service have a single, clear responsibility?
- [ ] Does each service own its own database?
- [ ] Is inter-service communication via API or events (not shared DB)?
- [ ] Is there an API Gateway for external traffic?
- [ ] Are Circuit Breakers configured for all downstream calls?
- [ ] Is distributed tracing (Correlation IDs) implemented?
- [ ] Is each service independently deployable?
- [ ] Are timeouts configured on every outbound call?

## Common Interview Questions

**Q: Why not just use a monolith?**
A: Monoliths are fine for small teams and early-stage products. The complexity of microservices is only justified when you have multiple teams, different scaling requirements per feature, or a need for independent deployment cycles.

**Q: How do you handle transactions across microservices?**
A: Use the SAGA pattern with either orchestration or choreography. Avoid 2PC due to its blocking nature and tight coupling.

**Q: How does Service A find Service B's address?**
A: Through Service Discovery — either client-side (querying a Service Registry like Consul) or server-side (the load balancer/API Gateway queries the registry on behalf of the caller).

**Q: What is the biggest operational challenge of microservices?**
A: Observability. With 20+ services, debugging a failing request requires distributed tracing, structured logging with Correlation IDs, and centralized metrics — none of which exist in a monolith.
