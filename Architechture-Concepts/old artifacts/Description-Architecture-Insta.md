# Architecture & Backend Concepts — Study Guide

Expanded notes from architecture interview prep covering authentication, databases, system design, JavaScript, and distributed systems. Written for college students preparing for technical interviews.

---

# Backend Authentication

## Session-Based Authentication

**What it is:** After a user logs in, the server creates a **session** — a record stored server-side (in memory or a database like Redis) — and sends the client a **session ID** as a cookie. On every subsequent request, the browser automatically sends that cookie, and the server looks up the session to identify the user.

**How it works:**
```
1. User submits username + password
2. Server validates credentials
3. Server creates session: { sessionId: "abc123", userId: 42, expires: ... }
4. Server stores session in memory / Redis
5. Server sends cookie: Set-Cookie: sessionId=abc123; HttpOnly; Secure
6. Browser stores cookie and sends it automatically with every request
7. Server reads cookie → looks up session → identifies user
```

**The CSRF Vulnerability:** Because browsers automatically attach cookies to every request to a domain, a malicious website can trick the user's browser into making a request to your site — carrying the session cookie without the user knowing. This is **Cross-Site Request Forgery (CSRF)**.

**CSRF mitigation:**
- **CSRF tokens** — Include a random token in every form that the server validates. A malicious site cannot read this token (same-origin policy).
- **SameSite cookie attribute** — `SameSite=Strict` or `SameSite=Lax` prevents cookies from being sent on cross-origin requests.

**Interview Language:**
- "Session-based auth is stateful — the server must store and look up session data on every request, which creates scaling challenges in distributed systems."
- "The primary risk is CSRF — mitigated with CSRF tokens or the SameSite cookie attribute."

---

## Token-Based Authentication (JWT)

**What it is:** After login, the server issues a **token** (typically a JWT) that the client stores and sends with each request. The server **validates the token cryptographically** — no database lookup needed. This makes it **stateless**.

**JWT Structure:** Three Base64URL-encoded parts separated by dots:
```
Header.Payload.Signature
eyJhbGciOiJSUzI1NiJ9.eyJ1c2VySWQiOjQyfQ.SflKxwRJSMeKKF2QT4...
```

- **Header** — `{ "alg": "RS256", "typ": "JWT" }` — algorithm used to sign
- **Payload** — `{ "userId": 42, "roles": ["admin"], "exp": 1753920000 }` — the claims
- **Signature** — HMAC or RSA signature over Header + Payload — verifies integrity

**JWT Security Best Practices:**
- **Never store PII** (names, emails, SSNs) in the payload — it is Base64-encoded, not encrypted. Anyone can decode it.
- **Use opaque tokens for external-facing APIs** — an opaque token is just a random string; only your server can validate it. JWTs expose their payload to anyone who intercepts them.
- **Short access token TTL** — 15 minutes is standard. If stolen, it expires quickly.
- **Use refresh tokens** — long-lived (days/weeks), stored securely server-side. Used only to obtain new access tokens.
- **Always verify the signature** using the server's public key — never trust a JWT just by decoding the payload.

**Interview Language:**
- "JWTs are stateless — the server validates the signature without a database lookup, making them ideal for distributed systems and microservices."
- "The trade-off is revocation: a valid JWT cannot be invalidated before expiry without a server-side blocklist, which reintroduces state."

---

## OAuth 2.0

**What it is:** An **authorization framework** that allows a user to grant a third-party application limited access to their resources on another service — without sharing their password.

**Common Flow (Authorization Code Grant):**
```
1. User clicks "Login with Google"
2. Browser redirects to Google's auth server
3. User authenticates with Google and approves permissions
4. Google redirects back with a short-lived authorization code
5. Your backend exchanges the code for an access token + refresh token
6. Your backend uses the access token to call Google APIs on the user's behalf
```

**OpenID Connect (OIDC)** adds an **ID token** on top of OAuth 2.0 — a JWT that contains the user's identity (name, email, sub/userId). This is what "Login with Google" uses.

---

## Access Tokens and Refresh Tokens

| Token | Lifetime | Stored | Purpose |
|---|---|---|---|
| **Access Token** | 15 min – 1 hour | Client memory / localStorage | Sent with every API request |
| **Refresh Token** | Days – weeks | HttpOnly cookie or server-side | Obtain a new access token when current one expires |

**Flow when access token expires:**
```
1. Client makes API call → 401 Unauthorized (token expired)
2. Client silently calls POST /auth/refresh with the refresh token
3. Server validates refresh token, issues a new access token
4. Client retries the original API call with the new token
5. User never sees a login prompt
```

---

## Single Sign-On (SSO)

**What it is:** A user logs in once and gains access to multiple services. Example: log in to Google and automatically be authenticated in Gmail, Drive, YouTube, and Google Meet.

**How it works:** Uses protocols like **SAML 2.0** (enterprise, XML-based) or **OpenID Connect** (modern, JSON-based). A central **Identity Provider (IdP)** (e.g., Okta, Azure AD) authenticates the user. Each **Service Provider (SP)** trusts the IdP's assertion.

**Interview Language:**
- "SSO improves UX by eliminating multiple login prompts and improves security by centralizing authentication — you manage password policies and MFA in one place."

---

# SQL: Finding the N-th Highest Salary

## Basic Approach (LIMIT / OFFSET)

```sql
-- 2nd highest salary
SELECT DISTINCT salary
FROM employees
ORDER BY salary DESC
LIMIT 1 OFFSET 1;

-- N-th highest (replace 1 with N-1)
SELECT DISTINCT salary
FROM employees
ORDER BY salary DESC
LIMIT 1 OFFSET N-1;
```

**Limitation:** Does not handle ties correctly if you want rank-based logic.

## Modern Approach: Window Functions (DENSE_RANK)

```sql
-- N-th highest salary, handling ties correctly
SELECT salary
FROM (
    SELECT salary,
           DENSE_RANK() OVER (ORDER BY salary DESC) AS rnk
    FROM employees
) ranked
WHERE rnk = 2;  -- replace 2 with N
```

**DENSE_RANK vs RANK vs ROW_NUMBER:**
| Function | Ties | Gap after tie |
|---|---|---|
| `ROW_NUMBER()` | No (each row unique) | No gap |
| `RANK()` | Yes (same rank for ties) | Gap (1,1,3) |
| `DENSE_RANK()` | Yes (same rank for ties) | No gap (1,1,2) |

**Interview Language:**
- "I'd use `DENSE_RANK()` with a subquery for the N-th highest salary — it correctly handles duplicate salaries, unlike a simple `LIMIT/OFFSET`."

---

# Microservice Performance

## Sidecar Logging Pattern in Kubernetes

**The Problem:** If the main application container handles log collection and shipping (to Elasticsearch, Splunk, etc.), it consumes CPU and memory that should be serving user traffic. A spike in log volume can slow down the application.

**The Sidecar Pattern:** Run a second container — the **sidecar** — in the same Kubernetes Pod as the application. The main container writes logs to a shared volume. The sidecar reads from that volume and ships logs to the central log store.

```
Pod
├── app-container        → writes logs to /var/log/app/
└── log-shipper-sidecar  → reads /var/log/app/ → ships to Elasticsearch
        (Fluentd / Filebeat)
```

**Why it matters:** The application container is completely decoupled from log shipping. A log backlog or network issue in the sidecar does not affect the main application's performance.

## Redis Cache Characteristics

**In-memory:** Data lives in RAM, not on disk. RAM access is ~100x faster than disk. A Redis `GET` typically takes 0.1–1ms.

**Single-threaded with event loop:** Redis uses a single thread to process commands. This eliminates mutex locking overhead. It handles thousands of concurrent connections by using an event loop (like Node.js) — it never blocks on I/O, just on CPU.

**Why single-threaded is fine:** Redis operations are so fast (microseconds) that a single thread can handle 100,000+ operations per second. The bottleneck is almost always the network, not CPU.

---

# Microservice Fault Tolerance

## Circuit Breaker

Prevents an application from repeatedly calling a failing service. Three states:

```
[CLOSED] → failure rate > threshold → [OPEN] → wait duration → [HALF-OPEN]
   ↑                                                                  |
   └──────── test requests succeed ──────────────────────────────────┘
                                         test requests fail → [OPEN]
```

- **CLOSED:** All requests pass through. Failure rate is tracked.
- **OPEN:** Requests fail immediately (no network call). Fast failure, no timeout waiting.
- **HALF-OPEN:** A few test requests are allowed. If they succeed, circuit closes. If they fail, it reopens.

## Bulkhead Pattern

**Analogy:** A ship's hull is divided into watertight compartments (bulkheads). If one compartment floods, the others stay dry — the ship doesn't sink.

**In software:** Isolate resources (thread pools, connection pools) per downstream dependency. If Service B is slow and exhausts its thread pool, Service C's thread pool is unaffected. One slow dependency cannot bring down the entire application.

## Timeout + Retry with Exponential Backoff

Every outbound call must have a **timeout**. Without it, slow services hold threads indefinitely, eventually exhausting the thread pool.

**Exponential Backoff:**
```
Attempt 1: wait 1s
Attempt 2: wait 2s
Attempt 3: wait 4s
Attempt 4: wait 8s → give up
+ random jitter (±500ms) to avoid synchronized retries
```

**Interview Language:**
- "Fault tolerance in microservices requires layered defenses: timeouts prevent thread starvation, circuit breakers prevent cascading failures, bulkheads isolate resource pools, and retries with exponential backoff handle transient faults."

---

# Cookies vs. Cache

| | **Cookies** | **Cache** |
|---|---|---|
| **Stored by** | Browser (client-side) | Server or CDN |
| **Purpose** | Persist user state / session / preferences | Speed up data retrieval |
| **Lifetime** | Can be long-lived (weeks/months) | Short-lived (seconds to hours via TTL) |
| **Contents** | Session IDs, user preferences, tracking data | Database query results, rendered HTML, API responses |
| **Set by** | Server via `Set-Cookie` header | Application code or CDN configuration |
| **Security concerns** | CSRF, XSS (mitigated with HttpOnly, SameSite) | Stale data, cache poisoning |

**Analogy:**
- **Cookie:** A loyalty card in your wallet — the store gave it to you, and you show it every time you visit so they remember who you are.
- **Cache:** A notepad on the counter where the store writes down the day's prices so they don't have to look them up in the database for every customer.

---

# Database Data Deletion

## Logical Layer: Soft Delete

A **soft delete** marks a record as deleted without removing it from the database. The row remains physically present.

```sql
-- Soft delete
UPDATE users SET deleted_at = NOW() WHERE id = 42;

-- Query that respects soft deletes
SELECT * FROM users WHERE deleted_at IS NULL;
```

**Why use soft delete:**
- Audit trails and compliance — you can prove a record existed.
- Accidental deletion recovery — just set `deleted_at = NULL`.
- Referential integrity — related records don't break.

**Downside:** The table grows unboundedly. Queries must always include `WHERE deleted_at IS NULL`.

## Physical Layer: Hard Delete

A **hard delete** permanently removes the row from the table. The space is marked as free at the page level but is not immediately returned to the OS.

```sql
DELETE FROM users WHERE id = 42;
```

After a hard delete, the database page has "dead tuples" (Postgres terminology) — the space is logically free but physically fragmented.

## Space Recovery After Deletion

```sql
-- PostgreSQL: reclaim space and rebuild indexes
VACUUM FULL users;

-- Or just clean dead tuples without locking the table
VACUUM users;

-- Rebuild a fragmented index
REINDEX TABLE users;
```

**DBA health metrics to track:**
- **Transaction log growth** — unusually fast growth signals long-running transactions.
- **Page fragmentation** — fragmented pages slow down range scans.
- **Dead tuple percentage** — high dead tuple ratio indicates VACUUM is not running frequently enough.
- **Auto-vacuum frequency** — ensure auto-vacuum is keeping up with delete/update rates.

## GDPR Compliance

GDPR gives users the **right to erasure** ("right to be forgotten"). When a user requests deletion:

1. **Hard delete** their PII from the primary database.
2. **Anonymize** records that must be retained for audit/legal purposes (replace name/email with a UUID).
3. **Purge backups** — harder; requires a retention policy that expires backups containing the user's data.
4. **Cascade deletion** across all services that hold the user's data (requires a coordinated deletion event across microservices).

---

# Data Deduplication

**What it is:** A storage optimization where identical data is stored only once. Multiple files that contain the same content each point to the same underlying storage block rather than storing separate copies.

**How it works (block-level dedup):**
```
File A: [Block1][Block2][Block3]
File B: [Block1][Block2][Block4]  ← Block1 and Block2 are shared
                                      only Block4 is new
```

Each block is identified by its hash (SHA-256). If the hash already exists in storage, the new file just gets a pointer to the existing block — no new data is written.

**Used in:** Cloud storage (Dropbox, OneDrive), backup systems (Veeam), virtual machine snapshot storage.

**Copy-on-write:** When a shared block is modified by one user, a new copy is made only for that user. The original shared block remains unchanged for other users.

---

# VM vs. Docker vs. Kubernetes

## Virtual Machines (VMs)

A VM is a fully emulated computer — it simulates CPU, memory, storage, and networking in software using a **hypervisor** (VMware ESXi, Microsoft Hyper-V, KVM).

Each VM runs a complete **guest OS** (Windows, Linux) on top of the hypervisor. VMs are completely isolated from each other and from the host.

```
Physical Server
└── Hypervisor (Type 1: bare metal / Type 2: on top of host OS)
    ├── VM 1: Guest OS + App A
    ├── VM 2: Guest OS + App B
    └── VM 3: Guest OS + App C
```

**Cost:** Each VM carries a full OS (1-2 GB RAM just for the OS). Boot time: 30-60 seconds.

## Docker Containers

Containers are **lightweight, isolated processes** that share the host OS kernel. They do not emulate hardware — they use Linux kernel features directly.

**Linux kernel features Docker uses:**
- **cgroups** — Limits CPU, memory, and I/O per container.
- **namespaces** — Isolates process IDs, network interfaces, file systems, and users per container.
- **Union File System (OverlayFS)** — Layers container images. Each layer is read-only; a writable layer is added on top when the container runs. Shared base layers are not duplicated.

```
Physical Server
└── Host OS (Linux kernel)
    ├── Container 1: App A (own filesystem, network, processes)
    ├── Container 2: App B (own filesystem, network, processes)
    └── Container 3: App C (own filesystem, network, processes)
```

**Advantage over VMs:** Containers start in milliseconds, use MBs of RAM instead of GBs, and share the OS kernel — far more efficient.

## Kubernetes (K8s)

Kubernetes is a **container orchestration platform** — it manages running, scaling, and healing containers across a cluster of machines.

```
Kubernetes Cluster
├── Control Plane (Master)
│   ├── API Server — receives all kubectl commands
│   ├── Scheduler — decides which node runs which pod
│   ├── Controller Manager — maintains desired state
│   └── etcd — distributed key-value store for cluster state
└── Worker Nodes
    ├── Node 1: [Pod: App A x2] [Pod: App B x1]
    ├── Node 2: [Pod: App A x1] [Pod: App C x2]
    └── Node 3: [Pod: App B x2] [Pod: App C x1]
```

**What Kubernetes handles:**
- **Scheduling** — places containers on the right node.
- **Auto-scaling** — HPA adds more pods when CPU exceeds threshold.
- **Self-healing** — restarts failed containers, replaces unhealthy nodes.
- **Service discovery** — stable DNS names for services regardless of pod IP changes.
- **Rolling updates** — updates containers with zero downtime.

**Interview Language:**
- "VMs provide strong isolation with full OS emulation — best for multi-tenant environments. Containers share the kernel, making them faster and lighter — best for microservices. Kubernetes orchestrates containers at scale across a cluster."

---

# Scaling a System — The Full Journey

When your system starts to struggle under load, you scale it in stages:

```
Stage 1: Single server (works for early-stage)
    ↓ Too slow
Stage 2: Vertical scaling — upgrade the server's CPU/RAM
    ↓ Hit hardware limits
Stage 3: Horizontal scaling — add more app servers
    ↓ Need traffic distribution
Stage 4: Load balancer — distribute requests across app servers
    ↓ Database becomes the bottleneck
Stage 5: Read replicas — master for writes, replicas for reads
    ↓ Too much data for one DB server
Stage 6: Database sharding — partition data across multiple DB servers
    ↓ Repeated DB queries slow things down
Stage 7: Cache layer (Redis) — serve frequent reads from memory
    ↓ Large files (images, video) consume bandwidth
Stage 8: CDN — serve static assets from edge servers near users
    ↓ Monolith is hard to scale individual features
Stage 9: Microservices — decompose by feature, scale independently
```

**Interview Language:**
- "When discussing scaling, I walk through the problem layer by layer — compute first, then the database, then introducing a cache for read-heavy data, and finally a CDN for static assets."

---

# Tesla's Data Architecture Case Study

**The challenge:** Tesla's factories, vehicles, and energy systems generate trillions of data points — time-series metrics like battery voltage, motor temperature, factory sensor readings. This data must be ingested, stored, and queried for real-time monitoring and historical analysis.

**Evolution:**

**Phase 1 — Prometheus:** Open-source time-series database. Good for short-term metrics, not designed for petabyte-scale historical storage. Hit scalability limits.

**Phase 2 — ClickHouse:** An **OLAP (Online Analytical Processing)** columnar database built for extreme read performance on large datasets. Optimized for:
- Aggregation queries over billions of rows.
- Column-oriented storage — reading only the columns you need (e.g., just `battery_temperature`) is far faster than row-oriented databases.
- Compression — repeated numeric values compress extremely well.

**Phase 3 — COMET:** A wrapper layer over ClickHouse that provides API stability and backward compatibility. Services that previously called Prometheus-compatible APIs continue to work without modification — COMET translates their requests to ClickHouse queries internally.

**Key lesson:** When migrating to a new data store, an abstraction layer (like COMET) allows gradual migration without forcing all consumers to change simultaneously.

**Interview Language:**
- "Tesla's architecture illustrates a key principle: when you outgrow a database, introduce an abstraction layer over the new system so existing consumers can migrate at their own pace."

---

# Webhook vs. WebSocket

| Feature | Webhook | WebSocket |
|---|---|---|
| **Triggered by** | Server event | Persistent connection (either side) |
| **Communication** | One-way (Server → Client) | Two-way (Client ↔ Server) |
| **Connection type** | Stateless HTTP POST | Persistent TCP connection |
| **Real-time** | Near real-time (depends on event) | True real-time |
| **Protocol** | HTTP/HTTPS | WS / WSS |
| **Client needs to run a server?** | Yes (must expose a URL) | No |
| **Use cases** | Payment alerts, CI/CD triggers, GitHub events | Chat, live dashboards, multiplayer games |

**Webhook analogy:** A webhook is like a doorbell — the server rings your bell when something happens. You (the client) must have a door (a public URL endpoint) for the server to ring.

**WebSocket analogy:** A WebSocket is like a phone call — both sides can talk and listen simultaneously over a persistent, open connection.

**Can you use both?** Yes. Example: a payment platform uses Webhooks to notify your server when a payment completes, while your frontend uses WebSockets to push the confirmation to the user's browser in real time.

---

# JavaScript: Promises and async/await

## Promises

A **Promise** is a JavaScript object representing the eventual result of an asynchronous operation. It can be in one of three states: **pending**, **fulfilled**, or **rejected**.

```javascript
// Creating a promise
const fetchUser = (id) => new Promise((resolve, reject) => {
  setTimeout(() => {
    if (id > 0) resolve({ id, name: "Alice" });
    else reject(new Error("Invalid ID"));
  }, 1000);
});

// Consuming with .then() / .catch()
fetchUser(1)
  .then(user => console.log(user.name))   // "Alice"
  .catch(err => console.error(err.message));
```

## async / await

`async/await` is **syntactic sugar** over Promises. It makes asynchronous code look and read like synchronous code.

```javascript
// Same logic as above, using async/await
async function loadUser(id) {
  try {
    const user = await fetchUser(id);   // pauses here until promise resolves
    console.log(user.name);              // "Alice"
  } catch (err) {
    console.error(err.message);
  }
}

loadUser(1);
```

**Rules:**
- `await` can only be used inside an `async` function.
- `await` pauses execution of that function until the Promise resolves — it does **not** block the JavaScript event loop or other code.
- An `async` function always returns a Promise, even if you return a plain value.

---

# JavaScript: call(), apply(), bind()

All three control the value of **`this`** inside a function.

```javascript
const user = { name: "Alice" };

function greet(greeting, punctuation) {
  console.log(`${greeting}, ${this.name}${punctuation}`);
}
```

## call()
Invokes the function **immediately**. Arguments passed **individually**.

```javascript
greet.call(user, "Hello", "!");   // "Hello, Alice!"
```

## apply()
Invokes the function **immediately**. Arguments passed as an **array**.

```javascript
greet.apply(user, ["Hi", "."]);   // "Hi, Alice."
```

**Memory trick:** `apply` → **A**rray.

## bind()
Returns a **new function** with `this` permanently bound. Does not call immediately.

```javascript
const boundGreet = greet.bind(user, "Hey");
boundGreet("?");   // "Hey, Alice?"
// Call it later, pass more args, this is always `user`
```

**When to use bind:** Event handlers where `this` context would otherwise be lost:
```javascript
class Button {
  constructor() { this.label = "Submit"; }
  handleClick() { console.log(this.label); }
  attach() {
    document.getElementById("btn")
      .addEventListener("click", this.handleClick.bind(this));
  }
}
```

---

# Username Availability Check — System Design

**Goal:** When a user types a username in a registration form, check instantly (< 100ms) whether it is already taken — even with millions of users.

## The Multi-Layer Strategy

```
User types username
        |
        v
[1. Load Balancer] → routes to nearest data center
        |
        v
[2. Bloom Filter] → probabilistic check — "definitely not taken" or "maybe taken"
        |
   "definitely not" → available ✓ (fast path, no DB hit)
   "maybe taken"   ↓
        v
[3. Redis Hashmap] → in-memory exact check (sub-millisecond)
        |
   Cache hit → taken or available ✓
   Cache miss ↓
        v
[4. Cassandra / DynamoDB] → authoritative distributed database check
```

## Key Data Structures

**Bloom Filter:** A probabilistic, space-efficient data structure. Can say "definitely not in the set" or "possibly in the set" — never has false negatives. Used as the first, fastest filter to eliminate most "not taken" cases without any DB or cache hit.

**Redis Hashmap:** An in-memory key-value store. After a Bloom Filter "maybe", check Redis for recently registered or recently queried usernames. Extremely fast (< 1ms).

**Trie (Prefix Tree):** A tree structure where each node represents a character. Enables prefix-based lookup — useful for autocomplete suggestions ("You searched for 'raj' — did you mean 'raja', 'rajesh', 'rajiv'?").

**B+ Tree:** The index structure used inside databases (MySQL, PostgreSQL). Keys are stored in sorted order in leaf nodes, enabling O(log n) point lookups and efficient range queries.

**Interview Language:**
- "For username lookups at scale, you layer data structures: a Bloom Filter eliminates definite non-matches instantly, Redis handles recent lookups in memory, and the distributed database is only hit on a cache miss."

---

# CAP Theorem

**What it is:** A fundamental theorem of distributed systems stating that a distributed database can only guarantee **two of three** properties simultaneously:

- **C — Consistency:** Every read returns the most recent write (or an error). All nodes see the same data at the same time.
- **A — Availability:** Every request to a non-failing node receives a response (though it may be stale).
- **P — Partition Tolerance:** The system continues operating even when network partitions occur (messages between nodes are lost or delayed).

**The catch:** In a real distributed system, **network partitions are unavoidable** — they happen due to hardware failures, network congestion, or misconfiguration. So **P is non-negotiable**. The real choice is always **CP vs. AP**.

| Choice | Behavior during partition | Examples |
|---|---|---|
| **CP** | Refuse requests to guarantee consistency | Google Spanner, HBase, Zookeeper |
| **AP** | Continue serving (possibly stale) responses | DynamoDB, Cassandra, CouchDB |

**Example:**
- **CP (bank account balance):** During a partition, the bank refuses to show your balance rather than risk showing a stale number. Consistency > availability.
- **AP (social media likes):** During a partition, your post might show 1,000 likes on one server and 1,001 on another for a few seconds. Availability > strict consistency.

**Interview Language:**
- "The CAP theorem tells us we can't have all three guarantees in a distributed system. Since network partitions are unavoidable, the real design choice is between CP systems (consistency-first) and AP systems (availability-first)."

---

# Consistent Hashing

**The problem it solves:** In a distributed cache with N servers, you map keys to servers using `hash(key) % N`. When you add or remove a server, N changes, and almost every key remaps to a different server — invalidating the entire cache.

**The solution:** Place both servers and keys on a **virtual circular ring** (hash ring, 0 to 2^32). Each key is assigned to the first server clockwise from its position on the ring.

```
Ring (0 → 2^32 → wraps back to 0)
        Server A (hash = 100)
       /
      /
  ← Key3 (90)    Ring    Key1 (200) →
      \                 /
       \               /
        Server B (hash = 250)
        Key2 is at 240 → assigned to Server B (next clockwise)
```

**When a server is added or removed:** Only the keys between the new/removed server and its predecessor on the ring need to be remapped — typically `1/N` of all keys, not all of them.

**Virtual nodes:** Each server is assigned multiple positions on the ring (virtual nodes) to ensure even distribution, especially when servers have different capacities.

**Used in:** Distributed caches (Redis Cluster), distributed databases (Cassandra, DynamoDB), load balancing.

---

# Authorization Models

## RBAC — Role-Based Access Control

Users are assigned **roles**. Roles have **permissions**. Users inherit permissions through their roles.

```
User: Alice  →  Role: Editor  →  Permissions: read, write
User: Bob    →  Role: Viewer  →  Permissions: read
User: Carol  →  Role: Admin   →  Permissions: read, write, delete, manage_users
```

**Best for:** Most applications — straightforward, easy to audit.
**Examples:** GitHub repository permissions, AWS IAM roles.

## ABAC — Attribute-Based Access Control

Access is granted based on a combination of **user attributes**, **resource attributes**, and **environmental conditions**. More flexible than RBAC but more complex.

```
ALLOW access IF:
  user.department == resource.department
  AND user.clearanceLevel >= resource.sensitivityLevel
  AND environment.time BETWEEN 09:00 AND 18:00
```

**Best for:** Fine-grained, dynamic access control in complex enterprises.
**Examples:** Healthcare systems (doctor can access patient records in their ward), financial systems.

## ACL — Access Control Lists

Each **resource** has its own list of who can access it and with what permissions.

```
File: report.pdf
  → Alice: read, write
  → Bob: read
  → Everyone else: no access
```

**Best for:** File systems and document sharing where permissions vary per item.
**Examples:** Google Drive (share with specific people), S3 bucket policies, Linux file permissions.

**Interview Language:**
- "RBAC scales well for most applications — easy to manage roles centrally. ACLs are more granular but harder to manage at scale. ABAC is the most flexible but also the most complex to implement and audit."

---

# Event Storming

**What it is:** A collaborative **Domain-Driven Design (DDD)** workshop technique for understanding a complex business domain. A group of developers, domain experts, and product owners map out the system using coloured sticky notes on a large board.

**Why it matters:** Event Storming bridges the gap between business requirements and technical design. By modeling the domain collaboratively, the team discovers bounded contexts, aggregates, and service boundaries — the foundation of a good microservices decomposition.

**The process (using an e-commerce example):**

1. **Events (orange)** — Start with past-tense domain events: `OrderPlaced`, `PaymentProcessed`, `ItemShipped`, `ItemReturned`.
2. **Commands (blue)** — Actions that trigger events: `PlaceOrder`, `ProcessPayment`.
3. **Actors (yellow)** — Who issues commands: `Customer`, `Payment Gateway`, `Warehouse System`.
4. **Policies (lilac)** — Business rules: "When `PaymentProcessed`, then trigger `ReserveInventory`."
5. **Hotspots (red)** — Uncertainties, risks, and questions to resolve.
6. **Aggregates (yellow, large)** — Clusters of events/commands around a consistent concept: the `Order` aggregate, the `Inventory` aggregate.
7. **Bounded Contexts** — Draw service boundaries around related aggregates — these become your microservices.

---

# Database Multi-Tenancy

A **multi-tenant** application serves multiple customers (tenants) from a single deployed instance. Each tenant's data must be isolated from others.

## Approach 1: Shared Database, Shared Schema

All tenants share the same tables. A `tenant_id` column in every table identifies which tenant owns each row.

```sql
SELECT * FROM orders WHERE tenant_id = 'acme_corp' AND status = 'pending';
```

| Pros | Cons |
|---|---|
| Simplest to implement and manage | Risk of data leakage if `tenant_id` filter is missed |
| Cheapest — one database for all | Noisy neighbor — one tenant's heavy queries slow others |
| Easiest to scale (single database) | Harder to comply with data residency requirements |

## Approach 2: Shared Database, Separate Schemas

Each tenant has their own **database schema** (namespace) within a single database. Tables are identical but isolated.

```
Database: saas_app
  ├── Schema: acme_corp  → orders, users, products
  ├── Schema: beta_inc   → orders, users, products
  └── Schema: gamma_co   → orders, users, products
```

| Pros | Cons |
|---|---|
| Stronger isolation than shared schema | Schema migrations must run across all tenant schemas |
| Easy per-tenant backups | Performance degrades with many tenants (thousands of schemas) |
| Easier customization per tenant | Connection pooling is more complex |

## Approach 3: Dedicated Database per Tenant

Each tenant gets their own separate database server.

| Pros | Cons |
|---|---|
| Maximum isolation and security | Most expensive — database costs multiply with tenants |
| No noisy neighbor problem | Complex provisioning — create DB for every new customer |
| Easy data residency compliance | Harder to manage migrations at scale |

**Decision guide:**
- **Startup / SMB SaaS:** Shared schema with `tenant_id` — simplest, cheapest.
- **Mid-market SaaS:** Separate schemas — balance of isolation and cost.
- **Enterprise SaaS with compliance requirements:** Dedicated databases.

---

# API Styles

| Style | Protocol | Format | Best for |
|---|---|---|---|
| **REST** | HTTP/1.1+ | JSON/XML | General-purpose CRUD APIs, public APIs |
| **GraphQL** | HTTP | JSON | Flexible queries, mobile (reduce over-fetching) |
| **gRPC** | HTTP/2 | Protocol Buffers (binary) | High-performance inter-service communication |
| **SOAP** | HTTP/SMTP | XML | Legacy enterprise systems, financial/banking |
| **WebSockets** | WS/WSS | Any | Real-time bidirectional (chat, live data) |
| **SSE** | HTTP | Text/event-stream | Server-to-client real-time push (notifications) |

**REST vs GraphQL:**
- REST: fixed endpoints, may over-fetch (get whole user object when you only need the name) or under-fetch (need multiple requests).
- GraphQL: client specifies exactly what fields it needs in a single query — no over/under fetching.

**gRPC advantages over REST:**
- Binary Protocol Buffers are 5-10x smaller than JSON.
- HTTP/2 multiplexing — multiple requests over one connection.
- Strongly typed contracts from `.proto` files — compile-time safety.

---

# Database Patterns in Microservices

## Anti-Pattern: Shared Database

Multiple services query the same database directly. This is the most common microservices mistake.

**Problems:**
- Any service can bypass another service's business logic by writing directly to its tables.
- Schema changes require coordination across all teams.
- One service's slow query locks tables and impacts other services.
- You cannot scale services' databases independently.

## Separate Databases per Service

Each service owns its data. The only way to get another service's data is through its API.

## API Composition

For read operations that need data from multiple services, an **Aggregator** service calls each service and combines the results.

```
GET /order-summary/{orderId}
        ↓
[Aggregator Service]
    ├── GET /orders/{id}      → Order Service
    ├── GET /users/{id}       → User Service
    └── GET /products/{ids}   → Product Service
        ↓
Combined response to client
```

## CQRS (Command Query Responsibility Segregation)

Separate write operations (Commands) from read operations (Queries) using different data models — and often different databases.

- **Write side:** Normalized SQL — consistent, transactional.
- **Read side:** Denormalized projections (Elasticsearch, read replicas) — optimized for specific query patterns.

## SAGA Pattern

For distributed transactions spanning multiple services, use a SAGA: a sequence of local transactions with compensating transactions for rollback.

- **Choreography:** Services emit events and react to each other's events. Decentralized but hard to trace.
- **Orchestration:** A central Orchestrator directs each service step by step. Clear flow, single point of control.

## Event Sourcing

Store every change to application state as an **immutable event** in an append-only log. The current state is derived by replaying all events.

```
Events: [AccountCreated] → [Deposited $500] → [Withdrew $200] → [Deposited $100]
Current balance = $400 (derived by replaying all events)
```

**Benefits:** Full audit trail, temporal queries ("what was the state on Tuesday?"), replay to rebuild projections.

**Interview Language:**
- "In a microservices architecture, each service must own its data — no shared databases. For cross-service reads I'd use API Composition, for cross-service writes I'd use the SAGA pattern with either orchestration or choreography depending on the complexity of the workflow."
