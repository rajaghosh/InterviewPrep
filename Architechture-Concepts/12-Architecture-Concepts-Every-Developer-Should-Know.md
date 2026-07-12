# 12 Architecture Concepts Every Developer Should Know

## Why Architecture Concepts Matter

Modern applications must handle:
- Millions of users
- Real-time data
- Distributed services
- Cloud-native infrastructure
- AI & data pipelines

Good architecture determines:
- **Scalability** — ability to grow with demand
- **Reliability** — system stays up under failure
- **Performance** — fast response times
- **Cost efficiency** — optimal resource usage

> These 12 concepts form the foundation of scalable systems.

---

## 1. Load Balancing

**What it does:** Distributes incoming traffic across multiple servers to prevent overload.

### How it works

```
Client sends request → Load balancer selects healthy server → Traffic distributed evenly
```

### Why it matters
- Prevents single server failure
- Improves availability
- Enables horizontal scaling

### Common Algorithms
| Algorithm | Description |
|-----------|-------------|
| Round Robin | Requests distributed sequentially |
| Least Connections | Routes to server with fewest active connections |
| IP Hash | Same client always hits same server |

**Used by:** AWS ELB, NGINX, HAProxy

---

## 2. Caching

**What it does:** Stores frequently accessed data in fast memory instead of recomputing.

### Workflow

```
Request arrives → Check cache first → Cache miss → Query database → Store result in cache
```

### Benefits
- Reduces database load
- Faster response times
- Lower infrastructure cost

### Popular Tools
- **Redis** — in-memory key-value store
- **Memcached** — distributed memory caching
- **CDN cache** — edge caching for static assets

**Example use cases:** Product pages, user sessions, API responses.

---

## 3. Content Delivery Network (CDN)

**What it does:** Serves content from servers closest to users geographically.

**How:** Static assets replicated globally across edge nodes.

### Advantages
- Reduced latency
- Faster page load
- Lower origin server traffic

### Best for
- Images, videos, JS files, AI model assets

### Providers
- Cloudflare
- Akamai
- AWS CloudFront

---

## 4. Auto Scaling

**What it does:** Automatically adjusts compute resources based on demand.

### Scaling Types

| Type | Description |
|------|-------------|
| Horizontal | Add/remove servers |
| Vertical | Increase machine size |

### Triggers
- CPU usage
- Traffic volume
- Queue size
- Latency

### Benefits
- Cost optimization
- High availability
- Handles traffic spikes automatically

### Platforms
- **AWS ASG** — Auto Scaling Groups
- **Kubernetes HPA** — Horizontal Pod Autoscaler

---

## 5. Sharding

**What it does:** Splits large databases into smaller partitions.

**Why needed:** Single database cannot scale infinitely.

### How
Data divided using a **shard key**:
- User ID
- Region
- Hash value

### Benefits
- Parallel processing
- Improved performance
- Horizontal scaling

### Challenge
- **Cross-shard queries** — queries spanning multiple shards are expensive and complex

---

## 6. Consistent Hashing

**Problem:** Adding/removing servers causes massive data reshuffling.

**Solution:** Consistent hashing distributes data intelligently — only a fraction of keys need to be remapped when nodes change.

### Advantages
- Minimal rebalancing
- High scalability
- Fault tolerance

### Used In
- Distributed caches
- CDNs
- NoSQL systems

### Examples
- **Cassandra** — uses consistent hashing for partition routing
- **DynamoDB** — uses virtual nodes on the ring

---

## 7. Rate Limiting

**Purpose:** Controls how many requests a client can send.

### Why critical
- Prevents abuse
- Protects backend systems
- Controls API cost

### Techniques

| Technique | How it works |
|-----------|-------------|
| Token Bucket | Tokens replenished at fixed rate; request consumes a token |
| Leaky Bucket | Requests drain at constant rate regardless of burst |
| Fixed Window | Count requests in fixed time window; reset at interval |

**Used by:** Public APIs, AI APIs, payment platforms.

---

## 8. Service Discovery

**Challenge:** In cloud environments, service locations constantly change (dynamic IPs, scaling events).

**Solution:** Automatic service registration & discovery.

### Process
1. Service registers itself with the registry on startup
2. Registry tracks all live instances
3. Clients query registry to find services

### Benefits
- Dynamic scaling support
- No hardcoded IPs

### Tools
- **Consul** — service mesh + health checking
- **Eureka** — Netflix OSS service registry
- **Kubernetes DNS** — built-in cluster DNS resolution

---

## 9. Circuit Breaker

**Problem:** Failing services can cascade failures across the system.

**Solution:** Circuit Breaker Pattern — wraps calls to downstream services and trips open on failure.

### States

```
Closed → Open → Half-Open → Closed
```

| State | Behavior |
|-------|----------|
| Closed | Normal operation — requests pass through |
| Open | Stop requests — fail fast, don't call downstream |
| Half-Open | Test recovery — allow limited requests to check if downstream recovered |

### Benefits
- Prevents system collapse
- Improves resilience
- Enables graceful degradation

### Libraries
- **Resilience4j** — Java/Kotlin
- **Hystrix** — Netflix OSS (now in maintenance)

---

## 10. API Gateway

**What it does:** Single entry point for all client requests.

**Why needed:** Microservices shouldn't expose themselves directly to clients.

### Responsibilities
- Authentication & authorization
- Routing to downstream services
- Rate limiting
- Logging & monitoring
- Request aggregation

### Benefits
- Simplified client communication
- Security control layer

### Examples
- **Kong** — open-source, plugin-based
- **AWS API Gateway** — fully managed
- **Apigee** — Google Cloud API management

---

## 11. Message Queue

**What it does:** Decouples services using asynchronous communication.

### Flow

```
Producer → Queue → Consumer
```

### Why important
- Services don't wait for each other
- Handles traffic spikes (buffer between producer and consumer)
- Improves resilience

### Real Example
```
Order placed → email service + billing service + analytics service
              (all processed separately and independently)
```

### Tools
- **RabbitMQ** — AMQP-based message broker
- **SQS** — AWS managed queue
- **Kafka** — high-throughput distributed log

---

## 12. Publish-Subscribe (Pub/Sub)

**Concept:** Multiple consumers receive events from a shared topic.

### Difference from Message Queue

| Pattern | Consumers |
|---------|-----------|
| Queue | One consumer per message |
| Pub/Sub | Many consumers receive the same message |

### Benefits
- Event-driven architecture
- Loose coupling between producers and consumers
- Real-time updates

### Use Cases
- Notifications
- Analytics pipelines
- AI event streaming

### Tools
- **Kafka** — distributed event streaming
- **Google Pub/Sub** — managed pub/sub service
- **NATS** — lightweight, cloud-native messaging

---

## Summary Table

| Concept | Problem Solved | Key Tools |
|---------|---------------|-----------|
| Load Balancing | Traffic overload | AWS ELB, NGINX, HAProxy |
| Caching | Slow repeated queries | Redis, Memcached |
| CDN | Geographic latency | Cloudflare, Akamai, CloudFront |
| Auto Scaling | Demand fluctuation | AWS ASG, K8s HPA |
| Sharding | Database scale limits | PostgreSQL, MongoDB, Cassandra |
| Consistent Hashing | Reshuffling on node change | Cassandra, DynamoDB |
| Rate Limiting | Abuse & cost control | API Gateways, custom middleware |
| Service Discovery | Dynamic service locations | Consul, Eureka, K8s DNS |
| Circuit Breaker | Cascading failures | Resilience4j, Hystrix |
| API Gateway | Direct microservice exposure | Kong, AWS API GW, Apigee |
| Message Queue | Synchronous coupling | RabbitMQ, SQS, Kafka |
| Pub/Sub | Single consumer bottleneck | Kafka, Google Pub/Sub, NATS |
