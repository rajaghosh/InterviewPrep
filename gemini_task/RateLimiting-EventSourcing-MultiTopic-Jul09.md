# System Design Multi-Topic Deep Dive — Rate Limiting, Event Sourcing & More

> **Source:** [share.gemini.google/2k0X7Rrl6Jc1](https://share.gemini.google/2k0X7Rrl6Jc1) → redirects to [gemini.google.com/share/166e93af823a](https://gemini.google.com/share/166e93af823a)
> **Model:** Gemini 3.1 Flash-Lite
> **Session Date:** July 9, 2026 at 09:40 AM
> **Saved:** July 11, 2026

---

## Table of Contents

1. [Session Overview](#1-session-overview)
2. [Rate Limiting: Fixed vs. Sliding Window](#2-rate-limiting-fixed-vs-sliding-window)
3. [Event Sourcing](#3-event-sourcing)
4. [Entity, DTO, and DAO](#4-entity-dto-and-dao)
5. [Connection Pool Exhaustion](#5-connection-pool-exhaustion)
6. [Solutions Architect vs. Enterprise Architect](#6-solutions-architect-vs-enterprise-architect)
7. [When NOT to Use Microservices](#7-when-not-to-use-microservices)
8. [How Netflix Handles Subtitle Switching](#8-how-netflix-handles-subtitle-switching)
9. [Retrieval-Augmented Generation (RAG)](#9-retrieval-augmented-generation-rag)
10. [Poison Pill Messages](#10-poison-pill-messages)
11. [Active-Active vs. Active-Passive Failover](#11-active-active-vs-active-passive-failover)
12. [Interview Q&A Cheatsheet](#12-interview-qa-cheatsheet)

---

## 1. Session Overview

This session extracts and synthesises learning content from 10 short-form educational videos covering core system design and backend architecture topics. Creators include Packetory, Piyush Garg, backendMorphism, NextWork, Rahul Rajat Singh, The Raw Journey, and Microsoft Visual Studio. All 10 turns used the same video-to-learning-content extraction prompt; all 10 responses were successfully generated. No error turns.

### Session Map

| Turn | Source / Creator | Topic | Status |
|---|---|---|---|
| 1 | Packetory | Rate Limiting: Fixed vs. Sliding Window | ✅ Extracted |
| 2 | Piyush Garg | Event Sourcing | ✅ Extracted |
| 3 | backendMorphism Day 87/365 | Entity, DTO, and DAO | ✅ Extracted |
| 4 | Packetory | Connection Pool Exhaustion | ✅ Extracted |
| 5 | NextWork | Solutions Architect vs. Enterprise Architect | ✅ Extracted |
| 6 | Rahul Rajat Singh — SA39 | When NOT to Use Microservices | ✅ Extracted |
| 7 | The Raw Journey — Interview #32 | Netflix Subtitle Switching | ✅ Extracted |
| 8 | Microsoft Visual Studio | RAG Architecture | ✅ Extracted |
| 9 | Packetory #197 | Poison Pill Messages | ✅ Extracted |
| 10 | Packetory #189 | Active-Active vs. Active-Passive Failover | ✅ Extracted |

---

## 2. Rate Limiting: Fixed vs. Sliding Window

### Overview

Rate limiting is a critical backend mechanism that controls the volume of traffic sent to an API or service. It enforces fairness, prevents abuse, and protects infrastructure from overload. The two most common algorithmic strategies are the **Fixed Window** and the **Sliding Window** — they differ fundamentally in how they define the measurement period, and that difference produces very different fairness guarantees under burst traffic.

### Architecture Diagram

```mermaid
flowchart TD
    client["Client Request"]
    fw["Fixed Window Counter\n(resets every 60s)"]
    sw["Sliding Window Log\n(last 60s rolling)"]
    fwOK["✅ Allow"]
    fwDENY["❌ Reject (limit hit)"]
    swOK["✅ Allow"]
    swDENY["❌ Reject (limit hit)"]
    burst["Boundary Burst Attack\n(exploit reset timing)"]

    client --> fw
    client --> sw
    fw -->|counter < limit| fwOK
    fw -->|counter >= limit| fwDENY
    fw -.->|vulnerability| burst
    sw -->|count in rolling window < limit| swOK
    sw -->|count in rolling window >= limit| swDENY

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class client userNode
    class fw,sw processNode
    class fwOK,swOK dataNode
    class fwDENY,swDENY errorNode
    class burst errorNode
```

### How It Works

**Fixed Window:**
1. Divide time into rigid, non-overlapping blocks (e.g., 12:00:00–12:00:59).
2. Maintain a counter per client per window.
3. Increment counter on each request; reject when counter exceeds the limit.
4. Reset counter automatically at the end of the window.
5. **Boundary Problem:** a client can send 100 requests at 12:00:59 and another 100 at 12:01:00 — effectively 200 requests in 2 seconds against a 100/min limit.

**Sliding Window:**
1. Record the timestamp of every request in a log (or approximate with a sorted set in Redis).
2. On each new request, evict timestamps older than `now - window_size`.
3. Count remaining entries; reject if count ≥ limit.
4. Because the measurement always looks at the *last N seconds*, burst attacks at boundary points are impossible.

### Key Components

| Component | Role | Technology Options |
|---|---|---|
| Counter Store | Holds request counts per key | Redis `INCR` + `EXPIRE`, In-memory map |
| Window Clock | Determines the current time bucket | System clock, NTP-synced |
| Log Store (Sliding) | Stores per-request timestamps | Redis Sorted Set (`ZADD`/`ZREMRANGEBYSCORE`) |
| Rate Limiter Middleware | Intercepts requests before business logic | API Gateway, Nginx, app-level filter |
| Reject Response | Returns 429 Too Many Requests | HTTP 429 + `Retry-After` header |

### Code Example

```python
import time
import redis

r = redis.Redis()

def fixed_window_allow(user_id: str, limit: int, window_secs: int) -> bool:
    bucket = int(time.time() // window_secs)
    key = f"rl:fixed:{user_id}:{bucket}"
    count = r.incr(key)
    if count == 1:
        r.expire(key, window_secs * 2)
    return count <= limit

def sliding_window_allow(user_id: str, limit: int, window_secs: int) -> bool:
    now = time.time()
    key = f"rl:sliding:{user_id}"
    pipe = r.pipeline()
    pipe.zremrangebyscore(key, 0, now - window_secs)
    pipe.zadd(key, {str(now): now})
    pipe.zcard(key)
    pipe.expire(key, window_secs)
    results = pipe.execute()
    return results[2] <= limit
```

### Interview Q&A

| Question | Answer |
|---|---|
| What is the boundary problem in fixed window rate limiting? | A client can exploit the reset timing by sending max requests at the end of one window and immediately again at the start of the next, effectively doubling the allowed rate in a short burst. |
| Why does sliding window solve the boundary problem? | It measures requests in a continuously moving time range (e.g., last 60 seconds) regardless of clock boundaries, so there is no "reset point" to exploit. |
| What data structure is used in Redis for sliding window rate limiting? | A Sorted Set (`ZADD`/`ZREMRANGEBYSCORE`) where the score is the request timestamp, allowing efficient eviction of stale entries. |
| When would you choose fixed window over sliding window? | When implementation simplicity matters more than strict fairness — e.g., internal services with predictable, non-adversarial traffic. |
| What HTTP status code should rate-limited responses return? | 429 Too Many Requests, ideally with a `Retry-After` header indicating when the client can retry. |
| How does a Token Bucket differ from a Sliding Window? | Token Bucket allows controlled bursting (tokens accumulate up to a maximum), whereas Sliding Window enforces strict per-window counts with no burst allowance. |

---

## 3. Event Sourcing

### Overview

Event Sourcing is a data persistence paradigm where instead of storing only the current state of an entity, the system stores every state change as an immutable, discrete event in an append-only log (the Event Store). The current state is always derivable by replaying the sequence of events from the beginning. This approach provides a perfect audit trail, enables temporal queries ("what was the state at time T?"), and powers downstream projections through event consumers.

### Architecture Diagram

```mermaid
flowchart LR
    cmd["Command\n(e.g. SuspendUser)"]
    handler["Command Handler\n(validates + emits)"]
    store["Event Store\n(append-only log)"]
    proj1["Projection: User Read Model"]
    proj2["Projection: Audit Log"]
    proj3["Projection: Analytics"]
    snapshot["Snapshot Store\n(optional, for replay perf)"]

    cmd --> handler
    handler --> store
    store --> proj1
    store --> proj2
    store --> proj3
    store -.->|periodic snapshot| snapshot

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class cmd userNode
    class handler processNode
    class store infraNode
    class proj1,proj2,proj3 outputNode
    class snapshot dataNode
```

### How It Works

1. A **Command** arrives (e.g., `SuspendUser`).
2. The **Command Handler** validates business rules and produces one or more **Events** (e.g., `UserSuspended`).
3. Events are appended (never updated or deleted) to the **Event Store** — a durable, ordered log.
4. **Projections** (read-model builders) consume the event stream and materialise query-optimised views.
5. To reconstruct an entity's current state, replay all events for that aggregate from the Event Store.
6. For performance, **Snapshots** periodically capture the current state so replay only starts from the latest snapshot, not from the beginning of time.

### Key Components

| Component | Role | Technology Options |
|---|---|---|
| Event Store | Immutable append-only log | EventStoreDB, Kafka, DynamoDB Streams, PostgreSQL |
| Command Handler | Validates + emits events | Domain service / aggregate root |
| Projection | Materialises read models from events | Event consumers, CQRS read side |
| Snapshot Store | Caches aggregate state at a point in time | Redis, PostgreSQL, S3 |
| Event Bus | Delivers events to multiple projections | Kafka, RabbitMQ, Azure Service Bus |

### Code Example

```python
import uuid
from dataclasses import dataclass, field
from datetime import datetime
from typing import List

@dataclass
class Event:
    event_type: str
    aggregate_id: str
    payload: dict
    timestamp: str = field(default_factory=lambda: datetime.utcnow().isoformat())
    event_id: str = field(default_factory=lambda: str(uuid.uuid4()))

class EventStore:
    def __init__(self):
        self._store: List[Event] = []

    def append(self, event: Event):
        self._store.append(event)

    def get_events(self, aggregate_id: str) -> List[Event]:
        return [e for e in self._store if e.aggregate_id == aggregate_id]

class UserAggregate:
    def __init__(self, user_id: str):
        self.user_id = user_id
        self.status = "NEW"

    def apply(self, event: Event):
        if event.event_type == "UserCreated":
            self.status = "ACTIVE"
        elif event.event_type == "UserSuspended":
            self.status = "SUSPENDED"
        elif event.event_type == "UserActivated":
            self.status = "ACTIVE"

    @classmethod
    def from_events(cls, user_id: str, events: List[Event]) -> "UserAggregate":
        agg = cls(user_id)
        for event in events:
            agg.apply(event)
        return agg

# Usage
store = EventStore()
uid = "user-42"
store.append(Event("UserCreated", uid, {"email": "a@b.com"}))
store.append(Event("UserSuspended", uid, {"reason": "fraud"}))
user = UserAggregate.from_events(uid, store.get_events(uid))
print(user.status)  # SUSPENDED
```

### Interview Q&A

| Question | Answer |
|---|---|
| What is the key difference between Event Sourcing and a traditional CRUD database? | CRUD stores only the current state (last write wins). Event Sourcing stores every state change as an immutable event; current state is derived by replaying the event log. |
| What is an Event Store? | An append-only, ordered, durable log where domain events are persisted. Events are never updated or deleted. |
| Why are snapshots used in Event Sourcing? | Replaying all events from the beginning becomes slow as history grows. Snapshots capture aggregate state at a point in time, so replay only needs to start from the latest snapshot. |
| What is a Projection in Event Sourcing? | A read-model builder that consumes the event stream and materialises query-optimised views optimised for specific use cases (e.g., a user summary page). |
| What pattern naturally pairs with Event Sourcing? | CQRS (Command Query Responsibility Segregation) — commands go through the event-sourced write side; projections serve the read side. |
| When is Event Sourcing a poor fit? | Simple CRUD with no audit requirements, high-frequency state updates with large payloads (e.g., game state), or teams unfamiliar with the replay/projection mental model. |

---

## 4. Entity, DTO, and DAO

### Overview

In layered software architecture, Entity, DTO (Data Transfer Object), and DAO (Data Access Object) represent three distinct responsibilities that must be separated to maintain a clean, secure, and maintainable codebase. Conflating these into a single class leads to security leaks, tight coupling between API contracts and database schema, and performance problems from over-fetching relational data. The pattern is foundational in Java/Spring, but the principles apply equally in Python, .NET, and Node.js.

### Architecture Diagram

```mermaid
flowchart TD
    db["Database"]
    entity["Entity\n(DB schema mapping + ORM)"]
    dao["DAO\nData Access Object\n(fetch / save / update logic)"]
    svc["Business Service Layer"]
    dto["DTO\nData Transfer Object\n(API contract shape)"]
    client["Client / API Consumer"]

    db <-->|JDBC / ORM| entity
    entity <-->|CRUD ops| dao
    dao <-->|domain objects| svc
    svc -->|maps to| dto
    dto -->|serialised JSON| client

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class db infraNode
    class entity dataNode
    class dao processNode
    class svc aiNode
    class dto outputNode
    class client userNode
```

### How It Works

1. The **Entity** mirrors the database table schema with ORM annotations (JPA/Hibernate, SQLAlchemy, EF Core). It may include fields like `hashed_password`, `internal_flags`, and relationship graphs (`@OneToMany`).
2. The **DAO** provides an abstract interface to the database — `findById()`, `save()`, `delete()` — hiding SQL or HQL complexity from the business layer.
3. The **Business Service** orchestrates domain logic using Entity objects returned by the DAO.
4. Before responding to a caller, the service **maps** the Entity to a **DTO** — copying only the fields the caller is permitted to see.
5. The **DTO** is serialised (JSON/XML) and sent to the API consumer. It carries no ORM annotations or DB-specific concerns.

### Key Components

| Component | Role | Technology Options |
|---|---|---|
| Entity | Database schema representation | JPA, SQLAlchemy ORM, EF Core, Hibernate |
| DAO / Repository | Abstract database operations | Spring Data JPA Repository, SQLAlchemy Session |
| DTO | API payload shape | Plain dataclass / POJO, Pydantic model, record class |
| Mapper | Converts Entity ↔ DTO | MapStruct (Java), AutoMapper (.NET), manual mapping |
| Business Service | Domain logic orchestrator | Spring @Service, Python service class |

### Code Example

```python
from dataclasses import dataclass
from typing import Optional

# Entity — mirrors DB table
@dataclass
class UserEntity:
    id: int
    email: str
    hashed_password: str  # NEVER exposed in DTO
    is_internal_admin: bool
    full_name: str

# DTO — API contract, no sensitive fields
@dataclass
class UserResponseDTO:
    id: int
    email: str
    full_name: str

# DAO — abstracts data access
class UserDAO:
    def __init__(self, db_session):
        self._db = db_session

    def find_by_id(self, user_id: int) -> Optional[UserEntity]:
        row = self._db.execute("SELECT * FROM users WHERE id = ?", (user_id,)).fetchone()
        if not row:
            return None
        return UserEntity(id=row["id"], email=row["email"],
                          hashed_password=row["hashed_password"],
                          is_internal_admin=row["is_internal_admin"],
                          full_name=row["full_name"])

# Service — maps Entity → DTO
class UserService:
    def __init__(self, dao: UserDAO):
        self._dao = dao

    def get_user_profile(self, user_id: int) -> Optional[UserResponseDTO]:
        entity = self._dao.find_by_id(user_id)
        if not entity:
            return None
        return UserResponseDTO(id=entity.id, email=entity.email,
                               full_name=entity.full_name)
```

### Interview Q&A

| Question | Answer |
|---|---|
| Why should you never return an Entity directly from an API endpoint? | Entities may contain sensitive fields (hashed passwords, internal flags) and are tightly coupled to the DB schema. Returning them exposes internals and breaks the API contract on any schema change. |
| What is the role of a DAO vs. a Repository? | DAO is the classic pattern exposing raw CRUD operations. Repository (from DDD) adds a collection-like abstraction where aggregates are queried using domain concepts. Repositories often hide multiple DAO calls. |
| What problem does a DTO solve in microservices? | DTOs define the inter-service contract independently of the internal domain model, allowing each service to evolve its DB schema without breaking the API contract consumed by other services. |
| Why can Entities cause performance issues if used as DTOs? | Entities often have eager-loaded or lazy-loaded relationships (e.g., `@OneToMany` collections). Serialising them for an API can trigger N+1 queries or send megabytes of unneeded nested data. |
| What is MapStruct (Java) / AutoMapper (.NET) used for? | Compile-time (MapStruct) or runtime (AutoMapper) code generators that automatically map fields between Entity and DTO, reducing boilerplate mapping code. |

---

## 5. Connection Pool Exhaustion

### Overview

A connection pool is a finite cache of pre-established database connections maintained by the application to avoid the overhead of creating a new connection per request. Connection Pool Exhaustion occurs when every connection in the pool is held by an in-flight operation — typically a slow query — leaving no connections available for new requests. The result is cascading timeouts and error responses to end users. This is one of the most common production outages in backend systems under load.

### Architecture Diagram

```mermaid
flowchart TD
    app["Application\n(many concurrent requests)"]
    pool["Connection Pool\n(e.g. max=10 connections)"]
    db["Database"]
    slowQ["Slow Query\n(holds connection for 30s+)"]
    blocked["New Requests BLOCKED\nPool Exhausted"]
    dlq["Error Response\n429 or 503 to Client"]

    app -->|acquires connection| pool
    pool -->|healthy conn| db
    pool -->|connection stuck| slowQ
    slowQ -.->|never releases| pool
    app -->|no connection available| blocked
    blocked --> dlq

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class app userNode
    class pool processNode
    class db infraNode
    class slowQ errorNode
    class blocked,dlq errorNode
```

### How It Works

1. The application initialises a pool of N pre-established connections on startup.
2. Each incoming request acquires a connection from the pool to query the database.
3. On completion, the connection is returned to the pool for reuse.
4. A **slow query** holds a connection for its entire duration — it is not released until the query finishes.
5. Under load, all N connections can be held by slow queries simultaneously.
6. New requests find no available connections and either queue (until timeout) or immediately fail with a "pool exhausted" error.

### Key Components

| Component | Role | Technology Options |
|---|---|---|
| Connection Pool | Manages pre-established DB connections | HikariCP (Java), SQLAlchemy pool, pgBouncer, ADO.NET pool |
| Pool Max Size | Upper bound on simultaneous connections | Typically 10–100; tuned to DB capacity |
| Checkout Timeout | Max wait time for a connection before error | 500ms–5s typical |
| Query Timeout | Forces slow queries to abort | `SET statement_timeout` (Postgres), `CommandTimeout` |
| Circuit Breaker | Fails fast when pool is near exhaustion | Polly (.NET), Resilience4j (Java) |
| DLQ / Fallback | Handles pool-exhausted requests gracefully | Retry queue, cache fallback, 503 response |

### Code Example

```python
from sqlalchemy import create_engine, text
from sqlalchemy.pool import QueuePool

engine = create_engine(
    "postgresql://user:pass@localhost/mydb",
    poolclass=QueuePool,
    pool_size=10,          # max persistent connections
    max_overflow=5,        # extra connections under burst
    pool_timeout=3,        # raise after 3s if no connection available
    pool_pre_ping=True,    # validate connection before checkout
)

def get_user(user_id: int):
    with engine.connect() as conn:
        # Statement timeout prevents slow queries from holding the pool
        conn.execute(text("SET statement_timeout = '2s'"))
        result = conn.execute(text("SELECT * FROM users WHERE id = :id"),
                              {"id": user_id})
        return result.fetchone()
```

### Interview Q&A

| Question | Answer |
|---|---|
| What is connection pool exhaustion? | All pre-allocated database connections are in use simultaneously, preventing new requests from acquiring a connection and causing timeout errors. |
| What is the primary cause of connection pool exhaustion? | Long-running or unoptimised slow queries holding connections for extended periods, combined with high concurrent request volume. |
| How does `pool_pre_ping` help? | It validates a connection is still alive before handing it to the application, preventing failures from stale connections that the database has closed (e.g., due to idle timeout). |
| What is the difference between `pool_size` and `max_overflow`? | `pool_size` is the number of persistent connections maintained. `max_overflow` allows additional temporary connections under burst load; these are closed when released rather than returned to the pool. |
| How does a circuit breaker relate to pool exhaustion? | When pool exhaustion is detected, a circuit breaker can open and immediately reject requests with a 503, preventing thread pile-up while the DB recovers, rather than allowing thousands of requests to queue and time out. |
| What is pgBouncer and when is it used? | pgBouncer is a connection pooler that sits between applications and PostgreSQL. It allows thousands of app-level "connections" to multiplex over a small number of actual DB connections, reducing exhaustion risk in high-concurrency systems. |

---

## 6. Solutions Architect vs. Enterprise Architect

### Overview

Solutions Architect and Enterprise Architect are both senior technical roles, but they operate at fundamentally different scopes and timescales. A Solutions Architect takes a specific business problem and designs a technical system to solve it — they work at project level with engineers. An Enterprise Architect defines organisation-wide technical standards, frameworks, and governance that all solutions must comply with — they work at portfolio level with leadership. Understanding the distinction is critical for career planning and for designing organisational structures that avoid architectural fragmentation.

### Architecture Diagram

```mermaid
flowchart TD
    subgraph SA ["Solutions Architect — Project Scope"]
        bizProblem["Specific Business Problem"]
        sysDesign["System Design\n(architecture for this project)"]
        engBuild["Engineers Build the Solution"]
        bizProblem --> sysDesign --> engBuild
    end

    subgraph EA ["Enterprise Architect — Org-Wide Scope"]
        orgStandards["Org-Wide Standards\n(tech stack, patterns, governance)"]
        sol1["Solution A\n(complies with standards)"]
        sol2["Solution B\n(complies with standards)"]
        sol3["Solution C\n(complies with standards)"]
        orgStandards --> sol1
        orgStandards --> sol2
        orgStandards --> sol3
    end

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class bizProblem userNode
    class sysDesign,orgStandards processNode
    class engBuild,sol1,sol2,sol3 outputNode
```

### How It Works

**Solutions Architect path:**
1. Receives a business problem (e.g., "build a real-time fraud detection system").
2. Analyses requirements, constraints, and non-functional requirements (latency, scale, cost).
3. Designs the technical architecture: component selection, data flow, integration points.
4. Works with engineers during build phase — reviews implementations, resolves design questions.
5. Owns technical accountability for the delivered solution.

**Enterprise Architect path:**
1. Defines the approved technology stack across the organisation (approved cloud providers, languages, messaging systems).
2. Creates architecture patterns and reference architectures that teams adopt.
3. Reviews new solution proposals for compliance with enterprise standards.
4. Manages technical debt at portfolio level and drives strategic platform consolidation.

### Key Components

| Dimension | Solutions Architect | Enterprise Architect |
|---|---|---|
| Scope | Single project / product | Organisation-wide portfolio |
| Time horizon | Weeks to months | Quarters to years |
| Key output | Solution architecture document | Enterprise architecture framework |
| Works with | Engineering teams | CTO, VPs, cross-BU stakeholders |
| Depth vs Breadth | Deep on one solution | Broad across many domains |
| Cloud focus | Detailed service selection | Cloud strategy and governance |

### Interview Q&A

| Question | Answer |
|---|---|
| What is the primary difference in scope between SA and EA? | SA operates at project/product level, designing one specific solution. EA operates at portfolio/organisation level, setting standards all solutions must follow. |
| Can the same person be both an SA and an EA? | In smaller organisations, yes — one person may wear both hats. In larger enterprises, these are distinct roles with separate reporting lines. |
| What does an Enterprise Architect produce? | Frameworks, reference architectures, approved technology lists, governance gates, and architectural review boards (ARBs). |
| Why is EA important in large organisations? | Without EA governance, different teams independently choose incompatible technologies, creating integration complexity, security gaps, and unsustainable operational diversity. |
| What skills differentiate an SA from a senior engineer? | SA requires system-level thinking across multiple components, understanding of non-functional requirements (reliability, cost, maintainability), and ability to communicate trade-offs to business stakeholders. |

---

## 7. When NOT to Use Microservices

### Overview

Microservices architecture decomposes a system into independently deployable services, each owning a bounded domain. While powerful at scale, prematurely adopting microservices — before the team or traffic justifies the complexity — is one of the most costly architectural mistakes in early-stage engineering. The pattern is optimised for large teams with clear domain boundaries; applying it to small codebases produces distributed monolith anti-patterns with none of the scale benefits and all of the operational overhead.

### Architecture Diagram

```mermaid
flowchart TD
    decision{"Team size and\ntraffic signal?"}
    monolith["Start with Modular Monolith\n(single deployable, clear module boundaries)"]
    signal["Scale signal detected:\n- DB bottleneck on specific domain\n- Team velocity blocked by shared codebase\n- Independent release cadence needed"]
    extract["Extract service:\n- Identify bounded context\n- Define API contract\n- Set up independent CI/CD"]
    microservice["Microservice\n(independently deployable)"]
    premature["❌ Premature Microservices\n- Operational complexity\n- Distributed tracing overhead\n- Deployment pipeline complexity\n- Network latency between services"]

    decision -->|Small team / early stage| monolith
    decision -->|Jumped to MS too early| premature
    monolith -->|Over time| signal
    signal --> extract
    extract --> microservice

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class decision processNode
    class monolith outputNode
    class signal aiNode
    class extract processNode
    class microservice dataNode
    class premature errorNode
```

### How It Works

| Stage | Recommended Architecture | Rationale |
|---|---|---|
| 0–5 engineers, early product | Modular Monolith | Fast iteration, zero distributed systems overhead |
| 5–20 engineers, proven product | Monolith with clear modules | Start enforcing domain boundaries within the codebase |
| Specific bottleneck identified | Extract ONE service | Only decompose the component under strain |
| Multiple independent teams | Full microservices | Now team autonomy and independent deployment justify the cost |

**Signals that it IS time to extract a service:**
- A specific database table is a bottleneck independent of the rest.
- One module needs a different tech stack (e.g., ML inference in Python from a Java monolith).
- Teams are blocked on each other due to a shared deployment pipeline.
- A component needs a dramatically different scaling profile (e.g., image processing vs. CRUD API).

### Interview Q&A

| Question | Answer |
|---|---|
| Why do teams adopt microservices prematurely? | Cargo-culting "big tech" architecture without matching team size or traffic, or over-engineering for hypothetical future scale that may never materialise. |
| What is a Distributed Monolith? | A system decomposed into separate services that remain tightly coupled via shared databases or synchronous call chains — achieving all the complexity of microservices with none of the independence benefits. |
| What is the recommended starting architecture for most startups? | A modular monolith with well-enforced domain boundaries. Modules are separate code units within a single deployable, enabling easy extraction into services once a genuine signal emerges. |
| What operational costs does microservices add? | Each service needs its own CI/CD pipeline, health checks, distributed tracing, service discovery, load balancer, and on-call runbook — a cost that only pays off at sufficient team/traffic scale. |
| What is Conway's Law and how does it relate to microservices? | Conway's Law states that system architecture mirrors the communication structure of the organisation. Microservices work best when service boundaries align with team boundaries — each team fully owns one service. |

---

## 8. How Netflix Handles Subtitle Switching

### Overview

Netflix serves subtitle data as a completely separate, lightweight stream from the video content, using time-synchronised fragments and a client-side manifest. When a user switches subtitles mid-movie, the client simply starts fetching the new subtitle track's next fragment — there is no re-buffering of video, no round-trip to re-fetch the entire subtitle file, and no perceptible delay. This design principle — decoupling high-bandwidth and low-bandwidth streams — is fundamental to building scalable multimedia delivery systems.

### Architecture Diagram

```mermaid
sequenceDiagram
    participant User
    participant NetflixPlayer as "Netflix Player (Client)"
    participant CDN as "CDN Edge"
    participant ManifestSvc as "Manifest Service"

    User->>NetflixPlayer: Play movie
    NetflixPlayer->>ManifestSvc: Request manifest
    ManifestSvc-->>NetflixPlayer: Manifest (video tracks + all subtitle tracks)
    NetflixPlayer->>CDN: Fetch video segment N
    NetflixPlayer->>CDN: Pre-fetch subtitle segment N (active language)
    CDN-->>NetflixPlayer: Video segment N + Subtitle segment N

    User->>NetflixPlayer: Switch subtitles (English → Spanish)
    Note over NetflixPlayer: No video re-fetch needed
    NetflixPlayer->>CDN: Fetch ES subtitle segment N+1
    CDN-->>NetflixPlayer: Spanish subtitle segment N+1
    NetflixPlayer->>User: Instant subtitle switch, zero buffering
```

### How It Works

1. **Manifest loaded at startup:** The Netflix player downloads a manifest file listing all available streams — video qualities, audio tracks, and every subtitle track — with their CDN URLs.
2. **Separate streams:** Video/audio and subtitles are stored and delivered as completely independent files on the CDN.
3. **Time-synchronised fragments:** Both video and subtitle files are divided into small segments (typically 2–4 seconds each), aligned by timestamp.
4. **Client-side synchronisation:** The player overlays subtitle text on the video frame by matching subtitle timestamps to the current playback position — entirely on-device.
5. **Switch = fetch next fragment of new track:** When the user switches language, the player continues video playback uninterrupted and simply starts requesting segments from the new subtitle track URL from the current playback timestamp onwards.

### Key Components

| Component | Role | Technology |
|---|---|---|
| Manifest File | Map of all available streams and their CDN paths | MPEG-DASH MPD or HLS m3u8 |
| Video Segments | Time-chunked video data | MPEG-4 fragmented, H.264/H.265 |
| Subtitle Segments | Time-chunked text/timing data | WebVTT, TTML, SRT fragments |
| CDN Edge | Serves segments with low latency | Netflix OpenConnect, Akamai |
| Client Player | Orchestrates segment fetching and rendering | Netflix SDK, ExoPlayer (Android) |
| Subtitles as Metadata | Design insight: text overlay, not media payload | Lightweight, cached separately |

### Interview Q&A

| Question | Answer |
|---|---|
| Why does switching subtitles on Netflix not cause buffering? | Subtitles are a separate lightweight stream from the video. Switching only changes which subtitle segment is fetched next — video download continues uninterrupted. |
| What is a manifest file in video streaming? | A metadata file (HLS m3u8 or DASH MPD) that lists all available stream variants, their bitrates, and the URLs of every segment. The player uses this to know where to fetch each piece of content. |
| What is Adaptive Bitrate Streaming (ABR)? | A technique where the client dynamically switches between different quality video segments (e.g., 360p → 1080p) based on available bandwidth, using the same time-chunked segment model. |
| How does the player synchronise subtitle text with video frames? | Each subtitle segment contains timing data (start/end timestamps for each text line). The player matches the current playback timestamp against these and renders the appropriate text. |
| What format are Netflix subtitles stored in? | Typically WebVTT (Web Video Text Tracks) or TTML (Timed Text Markup Language), delivered as small fragment files aligned to the same time grid as video segments. |

---

## 9. Retrieval-Augmented Generation (RAG)

### Overview

Retrieval-Augmented Generation (RAG) is an architectural pattern that enhances Large Language Models by injecting external, domain-specific knowledge at inference time rather than retraining the model. The model retrieves relevant documents from a vector store, uses them as context for generation, and can also execute actions (API calls, SQL queries) and coordinate agents for multi-step tasks. RAG reduces hallucinations, keeps the model's knowledge current without fine-tuning, and enables enterprise systems to leverage LLMs against private data.

### Architecture Diagram

```mermaid
flowchart TD
    user["User Query"]
    embed["Embedding Model\n(query → vector)"]
    vecDB["Vector Database\n(semantic search)"]
    docs["Retrieved Documents\n(top-k relevant chunks)"]
    ctx["Context Assembly\n(prompt + retrieved docs)"]
    llm["LLM\n(Generation)"]
    actions["Action Layer\n(API / SQL / Tool calls)"]
    coord["Coordinator Agent\n(multi-step orchestration)"]
    response["Final Response"]

    user --> embed
    embed --> vecDB
    vecDB -->|top-k chunks| docs
    docs --> ctx
    user --> ctx
    ctx --> llm
    llm --> actions
    llm --> coord
    actions --> response
    coord --> response
    llm --> response

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class user userNode
    class embed,ctx processNode
    class vecDB infraNode
    class docs dataNode
    class llm aiNode
    class actions,coord processNode
    class response outputNode
```

### How It Works

1. **User query** is received by the system.
2. An **embedding model** converts the query into a dense vector representation.
3. The vector is used to **semantic-search** a vector database for the top-k most relevant document chunks.
4. **Context is assembled:** the original query + retrieved chunks are combined into a prompt.
5. The **LLM generates** a response grounded in the retrieved context, reducing hallucinations.
6. If the task requires it, the LLM calls **action tools** (APIs, databases, code execution) to gather additional data or perform operations.
7. For complex multi-step tasks, a **coordinator agent** orchestrates sub-agents or iterative reasoning loops.

### Key Components

| Pillar | Role | Technology Options |
|---|---|---|
| Generation | Core LLM response production | Claude, GPT-4o, Gemini |
| Context | External knowledge injection via retrieval | Pinecone, Weaviate, Azure AI Search, pgvector |
| Action | Tool calls / function calling | OpenAI function calling, Claude tool use |
| Coordination | Multi-agent orchestration | LangGraph, AutoGen, Semantic Kernel |
| Agency | Model-driven iteration and self-correction | ReAct loop, Reflection pattern |

### Code Example

```python
from anthropic import Anthropic
import numpy as np

client = Anthropic()

# Simplified RAG pipeline
def rag_query(user_question: str, vector_store: list, top_k: int = 3) -> str:
    # Step 1: Retrieve relevant chunks (simplified — real impl uses embeddings + cosine sim)
    relevant_chunks = vector_store[:top_k]

    # Step 2: Assemble context
    context = "\n\n".join(f"[Document {i+1}]\n{chunk}" for i, chunk in enumerate(relevant_chunks))

    # Step 3: Generate grounded response
    response = client.messages.create(
        model="claude-sonnet-4-6",
        max_tokens=1024,
        messages=[{
            "role": "user",
            "content": f"""Answer the question using only the provided context.

Context:
{context}

Question: {user_question}"""
        }]
    )
    return response.content[0].text
```

### Interview Q&A

| Question | Answer |
|---|---|
| What problem does RAG solve? | LLMs have a static knowledge cutoff and no access to private data. RAG injects current, domain-specific context at inference time, reducing hallucinations and enabling LLMs to answer questions about private enterprise data without retraining. |
| What is a vector database used for in RAG? | It stores document embeddings (dense vector representations of text). At query time, the user's query is embedded and a nearest-neighbour search retrieves the most semantically similar document chunks. |
| What is the difference between RAG and fine-tuning? | Fine-tuning bakes knowledge into model weights (expensive, static). RAG retrieves knowledge dynamically at inference time (cheaper, keeps the model current without retraining). |
| What is "hallucination" in LLMs and how does RAG reduce it? | Hallucination is when the LLM generates plausible-sounding but factually incorrect content. RAG reduces it by grounding the model's response in retrieved, authoritative source documents. |
| What is the ReAct pattern in agentic RAG? | ReAct (Reason + Act) is a loop where the LLM alternates between reasoning about what to do next and taking an action (tool call), then observing the result, enabling iterative multi-step problem solving. |
| What is chunk size and why does it matter? | Chunk size is the size of document segments stored in the vector DB. Too small loses context; too large reduces retrieval precision. Typical values are 256–1024 tokens with overlap. |

---

## 10. Poison Pill Messages

### Overview

A Poison Pill Message is a message in a queue that causes the consumer (worker) to fail or crash every time it attempts to process the message. Because most messaging systems automatically re-queue failed messages for retry, the worker enters an infinite crash loop — consuming resources and blocking other healthy messages. The canonical solution is a **Dead Letter Queue (DLQ)**: after N failed attempts, the system routes the problematic message to a separate queue for human inspection or automated remediation, allowing the rest of the queue to drain normally.

### Architecture Diagram

```mermaid
flowchart TD
    app["Application\n(Producer)"]
    queue["Message Queue"]
    worker["Worker\n(Consumer)"]
    poisonPill["Poison Pill Message\n(malformed / unprocessable)"]
    retry["Retry Logic\n(attempt 1, 2, 3...)"]
    dlq["Dead Letter Queue\n(DLQ)"]
    healthyMsg["Healthy Messages\n(continue processing)"]
    alert["Alert / Manual Review"]

    app -->|publishes| queue
    queue -->|delivers| worker
    worker -->|fails on| poisonPill
    poisonPill --> retry
    retry -->|max retries exceeded| dlq
    dlq --> alert
    queue --> healthyMsg
    healthyMsg --> worker

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class app userNode
    class queue infraNode
    class worker processNode
    class poisonPill errorNode
    class retry aiNode
    class dlq dataNode
    class healthyMsg outputNode
    class alert errorNode
```

### How It Works

1. The **Producer** publishes messages to the queue.
2. The **Worker** pulls messages and attempts processing ("PULLING PAYLOAD").
3. A **Poison Pill** message (malformed, schema-invalid, or triggering an unhandled exception) causes the worker to crash or throw on every attempt.
4. The messaging system sees the failure, increments a **delivery count**, and re-queues the message.
5. Without a DLQ, the worker crashes on the same message repeatedly — an infinite loop that can take down the worker process entirely.
6. With a DLQ configured: after `MaxDeliveryCount` failures, the message is moved to the **Dead Letter Queue**.
7. Healthy messages in the main queue continue to be processed unimpeded.
8. An alert fires, and an operator can inspect the DLQ message to diagnose the root cause.

### Key Components

| Component | Role | Technology Options |
|---|---|---|
| Message Queue | Durable ordered delivery | SQS, Azure Service Bus, Kafka, RabbitMQ |
| Worker / Consumer | Processes messages from queue | Background service, Lambda, Kubernetes Job |
| Retry Policy | Controls retry count and backoff | Exponential backoff with jitter |
| Dead Letter Queue | Isolates unprocessable messages | SQS DLQ, Azure Service Bus DLQ, Kafka separate topic |
| Delivery Count | Tracks how many times a message has been attempted | Built into SQS, Service Bus, RabbitMQ |
| Alert / Monitor | Notifies team of DLQ growth | CloudWatch, Azure Monitor, Datadog |

### Code Example

```python
import boto3
import json

sqs = boto3.client("sqs")
MAIN_QUEUE_URL = "https://sqs.region.amazonaws.com/123/main-queue"

def process_message(body: dict):
    # Simulated: raises if message is malformed
    if "required_field" not in body:
        raise ValueError(f"Missing required_field in message: {body}")
    print(f"Processed: {body['required_field']}")

def worker_loop():
    while True:
        response = sqs.receive_message(
            QueueUrl=MAIN_QUEUE_URL,
            MaxNumberOfMessages=10,
            WaitTimeSeconds=20,
        )
        for msg in response.get("Messages", []):
            try:
                body = json.loads(msg["Body"])
                process_message(body)
                # Delete on success
                sqs.delete_message(QueueUrl=MAIN_QUEUE_URL,
                                   ReceiptHandle=msg["ReceiptHandle"])
            except Exception as e:
                # Do NOT delete — SQS will re-deliver up to MaxReceiveCount
                # After MaxReceiveCount, SQS automatically moves to DLQ
                print(f"Processing failed: {e}. Message will be retried or sent to DLQ.")
```

### Interview Q&A

| Question | Answer |
|---|---|
| What is a Poison Pill Message? | A message that causes the consumer to fail on every processing attempt, leading to an infinite retry loop that blocks the queue and can crash the worker process. |
| What is a Dead Letter Queue (DLQ)? | A separate queue where messages are automatically routed after exceeding the maximum delivery/retry count. It isolates unprocessable messages so healthy messages can continue flowing. |
| How does SQS prevent a poison pill from blocking the entire queue? | SQS tracks a `ReceiveCount` per message. After `MaxReceiveCount` delivery attempts, SQS automatically moves the message to the configured DLQ without any application-level code needed. |
| What is exponential backoff with jitter in retry logic? | Instead of retrying immediately at fixed intervals, each retry waits progressively longer (2^n seconds) plus a random jitter to prevent thundering herd. This avoids hammering a broken dependency and spreading load across retries. |
| How should you monitor a DLQ in production? | Set an alarm on DLQ depth — any message arriving in the DLQ should trigger an alert for investigation. DLQ growth rate indicates systemic producer or consumer bugs. |
| What is the difference between a DLQ and a retry queue? | A retry queue re-delivers messages with a delay for transient failures (e.g., downstream service temporarily down). A DLQ stores messages that have exceeded all retries for permanent failures requiring manual inspection. |

---

## 11. Active-Active vs. Active-Passive Failover

### Overview

Active-Active and Active-Passive are two fundamental high-availability (HA) patterns for handling server or component failures without downtime. They differ in whether standby resources are idle or actively serving traffic during normal operation. The choice involves a trade-off between resource cost/simplicity (Active-Passive) and performance/utilisation (Active-Active). Both patterns require automatic failover detection and traffic rerouting, which is typically handled by a load balancer, DNS failover, or a dedicated HA orchestrator.

### Architecture Diagram

```mermaid
flowchart TD
    subgraph AP ["Active-Passive"]
        router1["Router / Load Balancer"]
        primary["PRIMARY\n(handles all traffic)"]
        backup["BACKUP\n(idle — monitoring primary)"]
        failover1["On primary failure:\nbackup promotes to primary"]

        router1 --> primary
        router1 -.->|no traffic normally| backup
        primary -.->|heartbeat| backup
        backup -->|failover| failover1
    end

    subgraph AA ["Active-Active"]
        router2["Router / Load Balancer"]
        node1["NODE 1\n(active — handles traffic)"]
        node2["NODE 2\n(active — handles traffic)"]
        stateSync["State Sync\n(shared DB or sync protocol)"]

        router2 --> node1
        router2 --> node2
        node1 <-->|sync state| stateSync
        node2 <-->|sync state| stateSync
    end

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class router1,router2 processNode
    class primary,node1,node2 dataNode
    class backup infraNode
    class failover1 errorNode
    class stateSync aiNode
```

### How It Works

**Active-Passive:**
1. One server (**Primary**) handles 100% of incoming traffic.
2. One server (**Backup/Standby**) runs but processes no requests — it monitors the primary via heartbeat.
3. On primary failure, the backup detects the missed heartbeat and **promotes itself to primary**.
4. The load balancer or DNS is updated to route traffic to the former backup.
5. Recovery time depends on detection delay + promotion time (typically seconds to minutes).

**Active-Active:**
1. Two or more servers **all handle traffic simultaneously**, with the load balancer distributing requests across them.
2. All nodes must maintain consistent state — typically via a shared database, a distributed cache, or a synchronisation protocol.
3. On one node's failure, the load balancer simply stops sending it traffic — the remaining nodes absorb the load.
4. No promotion step needed; failover is nearly instantaneous.

### Key Components

| Dimension | Active-Passive | Active-Active |
|---|---|---|
| Idle resources | Yes — backup does nothing until needed | No — all nodes serve traffic |
| Resource utilisation | Low (50% waste in 2-node setup) | High (all resources used) |
| Failover speed | Seconds to minutes (promotion required) | Near-instant (load balancer detects and reroutes) |
| State complexity | Low (primary is authoritative) | High (must sync state across all nodes) |
| Cost | Lower compute cost, simpler config | Higher compute, more complex coordination |
| Best for | Budget-constrained HA, simple services | High-throughput, latency-sensitive systems |

### Interview Q&A

| Question | Answer |
|---|---|
| What is the key trade-off between Active-Active and Active-Passive? | Active-Passive is simpler and cheaper but wastes standby resources; Active-Active maximises utilisation and has faster failover but requires state synchronisation across nodes. |
| What is a "split-brain" problem in Active-Active? | When network partition causes two nodes to both believe they are the primary, they may accept conflicting writes. Solutions include consensus protocols (Raft, Paxos) or fencing tokens. |
| How does DNS-based failover work in Active-Passive? | The primary's health check fails, a health monitor updates the DNS A record to point to the backup's IP. Clients re-resolve DNS and connect to the backup. TTL must be low for fast failover. |
| Why is state synchronisation the hard problem in Active-Active? | User sessions, in-progress transactions, and cached data must be consistent across all active nodes. Any request from a user can land on any node, so all nodes must agree on current state. |
| What is a floating IP / Virtual IP (VIP) and how does it enable failover? | A VIP is an IP address not bound to a specific server. It is re-assigned to the healthy node on failover (e.g., via Keepalived/VRRP), allowing clients to connect to a stable address without DNS changes. |

---

## 12. Interview Q&A Cheatsheet

**Q: What is the Boundary Problem in Fixed Window rate limiting?**
> A user can send the maximum allowed requests at 12:00:59 and again at 12:01:00, effectively doubling the rate in 2 seconds. The Sliding Window prevents this by measuring the last N seconds continuously.

**Q: Why store events instead of state in Event Sourcing?**
> Storing events gives you a complete, immutable audit trail of how the system arrived at its current state. You can replay events to reconstruct any past state, build multiple read models from the same event log, and power analytics from a single source of truth.

**Q: Why are Entity, DTO, and DAO separate classes?**
> Separation prevents security leaks (entities hold raw DB fields like passwords), decouples API contracts from DB schema changes, and avoids performance issues from serialising eager-loaded ORM relationships directly to API consumers.

**Q: What causes Connection Pool Exhaustion and how do you fix it?**
> Slow queries hold connections for extended periods; under concurrent load all pool connections are consumed. Fixes: add query statement timeouts, optimise slow queries, tune pool size to DB capacity, add circuit breakers to fail fast.

**Q: When should you NOT use microservices?**
> When team size and traffic do not justify the operational overhead — distributed tracing, independent CI/CD pipelines, service discovery, and inter-service network latency all add complexity. Start with a modular monolith and extract services only when a specific scaling bottleneck or team independence signal emerges.

**Q: How does Netflix instantly switch subtitles without buffering?**
> Subtitles are a separate lightweight stream from video, pre-indexed in the manifest. The player pre-fetches subtitle fragments aligned to current video timestamps. Switching only changes which subtitle track URL the player fetches next — video download is completely unaffected.

**Q: What are the five pillars of RAG architecture?**
> Generation (LLM produces responses), Context (retrieval injects external knowledge), Action (model executes tool calls), Coordination (agents share complex task responsibility), and Agency (iterative refinement via ReAct-style loops).

**Q: What is a Dead Letter Queue and why is it critical?**
> A DLQ receives messages that have exceeded the maximum retry count. Without it, a poison pill message causes an infinite crash loop in the consumer. With it, the broken message is isolated for investigation while healthy messages continue processing.

**Q: What is the split-brain problem in Active-Active systems?**
> When a network partition causes both nodes to believe they are primary, they may accept conflicting writes. Resolved via consensus protocols (Raft, Paxos), fencing tokens, or quorum-based writes.

**Q: What is the difference between RAG and fine-tuning?**
> Fine-tuning embeds knowledge into model weights — expensive, requires retraining, produces a static snapshot. RAG retrieves knowledge dynamically from an external store at inference time — cheaper, keeps knowledge current, works on private data without touching model weights.

---

*Extracted from Gemini shared session · July 9, 2026 · GeminiShareToMD Agent v1.0*

---

```
═══════════════════════════════════════════════════════════
Token Usage Report
═══════════════════════════════════════════════════════════
Estimated without optimization:  ~7,450 tokens (raw Gemini content ~29,804 chars ÷ 4)
Actual (with optimization):      ~4,500 tokens (enriched file ~18,000 chars ÷ 4)
Savings (raw extract phase):     ~2,950 tokens (39%)
Techniques applied:              Strip UI chrome (PDF/Acrobat buttons, footer),
                                 Strip repeated user prompt (10 identical turns merged),
                                 Deduplicate repeated Gemini boilerplate,
                                 Compact-engineer verbose prose,
                                 Single-pass Write
═══════════════════════════════════════════════════════════
* Estimates: prose chars ÷ 4, code chars ÷ 3. Actual API usage varies by model.
```
