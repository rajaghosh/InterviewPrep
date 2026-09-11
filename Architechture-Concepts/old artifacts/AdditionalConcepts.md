# Additional Architecture Concepts — System Design Guide

**Audience:** College students and early-career engineers preparing for system design interviews.
**Goal:** Understand not just *what* these patterns are, but *why* they exist and *how* to explain them clearly in an interview.

---

## Table of Contents

1. [Robust Retry Mechanism for Queue-Based Jobs (Java)](#1-robust-retry-mechanism-for-queue-based-jobs-java)
2. [Microservice Resilience Patterns](#2-microservice-resilience-patterns)
3. [URL Shortener System Design at 1 Million RPS](#3-url-shortener-system-design-at-1-million-rps)
4. [Flash Sale System Design — Flipkart Case Study](#4-flash-sale-system-design--flipkart-case-study)
5. [Kubernetes Secrets Management](#5-kubernetes-secrets-management)

---

## 1. Robust Retry Mechanism for Queue-Based Jobs (Java)

### What Is It?

In any distributed system, failures are not the exception — they are the norm. Networks time out, downstream APIs return 500s, databases become temporarily unreachable. When your system processes work asynchronously via a queue (Kafka, RabbitMQ), you need a strategy for what happens when a job fails mid-processing. A Robust Retry Mechanism is a multi-layer architectural pattern that ensures no job is silently dropped, every failure is retried intelligently, and permanently failed jobs are preserved for manual review.

### The Analogy

Think of a postal delivery system. When a letter cannot be delivered (nobody home, address unclear), the postman does not throw it in the bin. Instead:

1. He tries the next day (first retry).
2. If that fails, he tries again in a few days (exponential backoff).
3. After three failed attempts, the letter goes to a "returned mail" holding office (Dead Letter Queue).
4. A supervisor at the holding office reviews it and decides whether to re-send it or mark it undeliverable.

That is exactly the pattern we build in software.

---

### Full Architecture: The 5-Service Model

```
+-------------------+       +-------------------+
|                   |       |                   |
|  Job Producer     +------>|  Message Queue    |
|  Service          |       |  (Kafka/RabbitMQ) |
|                   |       |                   |
+-------------------+       +--------+----------+
                                      |
                                      v
                            +---------+----------+
                            |                    |
                            |  Worker Service    |
                            |                    |
                            +-+--------+---------+
                              |        |
                     [Success]|        |[Failure]
                              v        v
                           Done   +----+----------------+
                                  |                     |
                                  |  Failure Handler    |
                                  |  Service (Retry)    |
                                  |                     |
                                  +----+----------------+
                                       |
                          [Max retries exceeded]
                                       |
                                       v
                            +----------+---------+
                            |                    |
                            |  Dead Letter Queue |
                            |  (DLQ)             |
                            |                    |
                            +----------+---------+
                                       |
                                       v
                            +----------+---------+
                            |                    |
                            |  Retry Dashboard   |
                            |  & Trigger Service |
                            |                    |
                            +--------------------+
```

---

### Service 1: Job Producer Service

**Goal:** Publish jobs (messages) to the queue so workers can pick them up asynchronously.

**Why it matters:** Decoupling the producer from the worker is the entire point of queue-based architecture. If the producer called the worker directly (synchronous HTTP), a slow or failing worker would block the producer. By publishing to a queue, the producer returns instantly and the work happens in the background.

**Tech Stack:** Java + Spring Boot + Apache Kafka (or RabbitMQ)

**Real-world example:** After a user completes a purchase on an e-commerce platform, the Order Service publishes an `OrderConfirmationEmailJob` message to the `email-jobs` Kafka topic. The HTTP response returns to the user immediately. The email is sent asynchronously by a separate service.

```java
@Service
public class OrderService {

    private final KafkaTemplate<String, OrderConfirmationEvent> kafkaTemplate;

    public OrderService(KafkaTemplate<String, OrderConfirmationEvent> kafkaTemplate) {
        this.kafkaTemplate = kafkaTemplate;
    }

    public void placeOrder(Order order) {
        // Save order to DB
        orderRepository.save(order);

        // Publish job to Kafka topic (fire-and-forget)
        OrderConfirmationEvent event = new OrderConfirmationEvent(
            order.getId(),
            order.getUserEmail(),
            order.getTotalAmount()
        );
        kafkaTemplate.send("email-jobs", order.getId().toString(), event);
        // Producer returns here — user gets HTTP 200 immediately
    }
}
```

**What breaks without it:** Without a queue, all downstream work must complete before the user gets a response. This couples unrelated services, slows response times, and causes cascading failures when any dependency is slow.

---

### Service 2: Worker Service

**Goal:** Consume messages from the queue and process them (send the email, charge the card, update inventory, etc.).

**Why it matters:** Workers are the engines of async processing. They run independently, can be scaled horizontally (more workers = more throughput), and consume jobs at their own pace without overwhelming downstream systems.

**Tech Stack:** Java + Spring Boot + `@KafkaListener`

```java
@Service
public class EmailWorkerService {

    private static final Logger log = LoggerFactory.getLogger(EmailWorkerService.class);

    private final EmailSenderClient emailClient;
    private final FailureHandlerService failureHandler;

    @KafkaListener(topics = "email-jobs", groupId = "email-worker-group")
    public void processEmailJob(OrderConfirmationEvent event) {
        try {
            log.info("Processing email job for orderId={}", event.getOrderId());
            emailClient.sendConfirmationEmail(event.getUserEmail(), event.getOrderId());
            log.info("Email sent successfully for orderId={}", event.getOrderId());
        } catch (Exception ex) {
            log.error("Email job failed for orderId={}, reason={}", event.getOrderId(), ex.getMessage());
            // Hand off to failure handler — do NOT re-throw or Kafka will keep replaying
            failureHandler.handleFailure(event, ex);
        }
    }
}
```

**Key design note:** The worker catches the exception explicitly and hands it to the Failure Handler. If you let an exception propagate uncaught from a Kafka listener, Kafka will replay the same message indefinitely — that is not intelligent retry, that is an infinite loop.

**What breaks without it:** Without a dedicated worker, either the producer has to do all the work synchronously (couples everything) or jobs sit in the queue and are never processed.

---

### Service 3: Failure Handler Service — Exponential Backoff Retry

**Goal:** When a job fails, do not immediately give up and do not hammer the failing service with retries every millisecond. Use exponential backoff: wait a little, then more, then more.

**Why exponential backoff?** If your email gateway is down and you retry every second, you will send thousands of requests to a service that is already struggling, making recovery harder. Exponential backoff gives the downstream system time to recover. The sequence of waits might be: 1s, 2s, 4s, 8s, 16s... doubling each time.

**Tech Stack:** Spring Retry or Resilience4j + Redis (for tracking retry count) + PostgreSQL (for persistence)

**Retry configuration with Resilience4j:**

```java
@Configuration
public class RetryConfig {

    @Bean
    public RetryRegistry retryRegistry() {
        RetryConfig config = RetryConfig.custom()
            .maxAttempts(3)
            .waitDuration(Duration.ofSeconds(2))
            .intervalFunction(IntervalFunction.ofExponentialBackoff(2000, 2.0))
            // Start at 2s, double each attempt: 2s, 4s, 8s
            .retryExceptions(SmtpTimeoutException.class, IOException.class)
            .ignoreExceptions(InvalidEmailException.class) // Don't retry permanent errors
            .build();
        return RetryRegistry.of(config);
    }
}
```

```java
@Service
public class FailureHandlerService {

    private final Retry retry;
    private final DlqService dlqService;
    private final FailedJobRepository failedJobRepository;

    public void handleFailure(OrderConfirmationEvent event, Exception originalError) {
        // Record the failure attempt
        FailedJob job = failedJobRepository.findByOrderId(event.getOrderId())
            .orElse(new FailedJob(event, 0));
        job.incrementRetryCount();
        job.setLastError(originalError.getMessage());
        failedJobRepository.save(job);

        if (job.getRetryCount() >= 3) {
            // Exceeded max retries — send to DLQ
            dlqService.sendToDlq(job);
        } else {
            // Schedule a retry with exponential backoff
            long delaySeconds = (long) Math.pow(2, job.getRetryCount());
            scheduleRetry(event, delaySeconds);
        }
    }
}
```

**What breaks without it:** Without intelligent retry, a transient failure (network hiccup, brief database unavailability) results in permanently lost jobs. Users never receive their confirmation emails and nobody knows why.

---

### Service 4: Dead Letter Queue (DLQ) Service

**Goal:** After exhausting all retries, preserve the failed job in a separate location for human review. Do not silently discard it.

**Why it matters:** Some failures are not transient — they are caused by bugs in your code, corrupt data, or permanently unavailable external services. These jobs will never succeed no matter how many times you retry them. The DLQ is a quarantine zone: jobs go there when the system gives up, but they are never deleted. Engineers can inspect them, fix the root cause, and re-enqueue them.

**In Kafka:** A DLQ is simply a separate Kafka topic (e.g., `email-jobs-dlq`). The DLQ consumer persists these messages to PostgreSQL for human review.

**In RabbitMQ:** A Dead Letter Exchange (DLX) is a special exchange. When a message is rejected or expires in a normal queue, RabbitMQ automatically routes it to the DLX, which forwards it to a dead letter queue.

```java
@Service
public class DlqService {

    private final KafkaTemplate<String, FailedJob> kafkaTemplate;
    private final DlqRepository dlqRepository;

    public void sendToDlq(FailedJob job) {
        // Write to Kafka DLQ topic for audit trail
        kafkaTemplate.send("email-jobs-dlq", job.getOrderId(), job);

        // Also persist to database for dashboard queries
        DlqRecord record = new DlqRecord();
        record.setJobId(job.getOrderId());
        record.setPayload(job.getPayload());
        record.setRetryCount(job.getRetryCount());
        record.setLastError(job.getLastError());
        record.setArrivedAt(Instant.now());
        record.setStatus(DlqStatus.PENDING_REVIEW);
        dlqRepository.save(record);
    }
}
```

**What breaks without it:** Without a DLQ, permanently failed jobs are silently dropped. You have no way to know that a user never got their order confirmation, and no way to recover the data to retry it after fixing the underlying bug.

---

### Service 5: Retry Dashboard and Trigger Service

**Goal:** Give operators and support engineers a UI to see all failed jobs in the DLQ, understand why they failed, and manually re-enqueue them after the root cause is resolved.

**Why it matters:** Automation handles most retries, but some situations require human judgment. A support engineer might see that 500 jobs failed because of a third-party API outage that lasted 2 hours. Now that the API is back, they can select all 500 jobs and re-trigger them with one click.

**Tech Stack:** Spring Boot REST API + PostgreSQL + React.js frontend

```java
@RestController
@RequestMapping("/api/dlq")
public class DlqController {

    private final DlqService dlqService;

    // GET all failed jobs with pagination
    @GetMapping("/jobs")
    public Page<DlqRecord> getFailedJobs(
        @RequestParam(defaultValue = "0") int page,
        @RequestParam(defaultValue = "50") int size) {
        return dlqService.getFailedJobs(PageRequest.of(page, size));
    }

    // POST to manually re-trigger a specific job
    @PostMapping("/jobs/{jobId}/retry")
    public ResponseEntity<String> retryJob(@PathVariable String jobId) {
        dlqService.reEnqueueJob(jobId);
        return ResponseEntity.ok("Job " + jobId + " re-enqueued successfully");
    }

    // POST to retry all failed jobs in bulk
    @PostMapping("/jobs/retry-all")
    public ResponseEntity<String> retryAllJobs() {
        int count = dlqService.reEnqueueAllPendingJobs();
        return ResponseEntity.ok(count + " jobs re-enqueued");
    }
}
```

---

### Interview Language: Retry Mechanisms

- "We use a multi-layer retry strategy. The worker service handles transient failures using exponential backoff via Resilience4j, starting at 2 seconds and doubling up to a maximum of 3 attempts. Jobs that exceed the retry limit are routed to a Dead Letter Queue, which is a separate Kafka topic, and persisted to PostgreSQL for operator review."
- "The key insight is that not all failures are the same. A network timeout is transient — worth retrying. A malformed message will never succeed — we want to isolate it in the DLQ immediately. Resilience4j lets us configure which exception types trigger retries and which are ignored."
- "We chose exponential backoff rather than fixed-interval retries because retrying too aggressively can worsen an outage. If the SMTP gateway is already overloaded, hammering it every second will delay its recovery."
- "The DLQ is not just a garbage bin — it is an audit log. Every job that ends up there has its full payload, error message, and retry history preserved. This is critical for debugging, compliance, and data recovery."

---

## 2. Microservice Resilience Patterns

### What Is It?

Resilience patterns are defensive architectural strategies that prevent a failure in one microservice from cascading into a system-wide outage. In a distributed system with dozens of services, any individual service can fail at any time. These patterns are your safety nets.

### The Analogy

Think of the electrical systems in a modern building. If one appliance short-circuits, the circuit breaker for that room trips — it does not black out the entire building. The lights in the hallway still work. Other rooms are unaffected. Microservice resilience patterns work on exactly this principle: contain the blast radius of any individual failure.

---

### Pattern 1: Service Health Guard

**What is it?** A continuous monitoring system that tracks the health and performance metrics of every microservice and fires alerts the moment something degrades.

**How it works technically:**

Every Spring Boot service exposes two key endpoints via Spring Boot Actuator:

- `/actuator/health` — returns `{"status": "UP"}` or `{"status": "DOWN"}` with details about database connections, disk space, and custom health checks.
- `/actuator/metrics` — exposes JVM metrics, HTTP request counts, response times, error rates, memory usage, etc.

Prometheus is a time-series database that periodically *scrapes* these endpoints (e.g., every 15 seconds) and stores the numeric values. Grafana connects to Prometheus and renders dashboards showing trends over time.

Alertmanager is configured with rules like: "If `cart-service /actuator/health` returns DOWN for more than 2 consecutive scrapes, send a PagerDuty alert and post to the #alerts Slack channel."

```yaml
# prometheus-alert-rule.yml
groups:
  - name: service-health
    rules:
      - alert: CartServiceDown
        expr: up{job="cart-service"} == 0
        for: 30s
        labels:
          severity: critical
        annotations:
          summary: "Cart Service is DOWN"
          description: "Cart service has been unreachable for more than 30 seconds."
```

**Real-world example:** The Flipkart Cart Service goes down during a Big Billion Day sale. Within 30 seconds, Prometheus detects the health check failure, Alertmanager sends a PagerDuty alert, and the on-call engineer gets woken up — all before any user has opened a support ticket.

**Interview Language:**
- "We use Spring Boot Actuator to expose health and metrics endpoints on every service. Prometheus scrapes these at a 15-second interval. We define alerting rules in Alertmanager that trigger Slack and PagerDuty notifications when any service is unreachable or when error rates exceed a threshold."
- "The health check endpoint doesn't just say UP or DOWN — it checks all dependencies: database connectivity, Redis connectivity, disk space, and any custom health indicators we define."

---

### Pattern 2: Circuit Breaker with Fallback

**What is it?** A mechanism that detects when a downstream service is failing and automatically stops sending requests to it, instead returning a pre-defined fallback response.

**The 3 States:**

```
CLOSED (normal)
    |
    | [failure rate exceeds threshold]
    v
OPEN (rejecting all calls, returning fallback)
    |
    | [wait duration expires]
    v
HALF-OPEN (testing with limited calls)
    |              |
 [success]     [failure]
    |              |
    v              v
 CLOSED          OPEN
```

- **CLOSED:** Normal operation. All calls pass through. The circuit breaker counts failures.
- **OPEN:** Failure rate exceeded the threshold (e.g., 50% of calls failed in the last 10 seconds). The circuit breaker stops all calls and returns the fallback immediately without even attempting the request. This protects the failing service from being overwhelmed further.
- **HALF-OPEN:** After a configured wait period (e.g., 60 seconds), the circuit breaker allows a small number of test calls through. If they succeed, it returns to CLOSED. If they fail, it goes back to OPEN.

**Java example with Resilience4j:**

```java
@Service
public class CartService {

    private final InventoryServiceClient inventoryClient;

    @CircuitBreaker(name = "inventoryService", fallbackMethod = "getInventoryFallback")
    public InventoryResponse checkInventory(String productId) {
        return inventoryClient.getStock(productId);
    }

    // Fallback is called when circuit is OPEN or call throws exception
    public InventoryResponse getInventoryFallback(String productId, Exception ex) {
        // Return a safe default — don't crash the user's cart experience
        return InventoryResponse.builder()
            .productId(productId)
            .available(true)    // Optimistic: assume available, validate at checkout
            .message("Inventory service temporarily unavailable. Please try again.")
            .build();
    }
}
```

**Resilience4j configuration:**

```yaml
resilience4j:
  circuitbreaker:
    instances:
      inventoryService:
        failure-rate-threshold: 50          # Open after 50% failure rate
        wait-duration-in-open-state: 60s    # Stay open for 60s before testing
        permitted-number-of-calls-in-half-open-state: 5
        sliding-window-size: 10             # Track last 10 calls
        minimum-number-of-calls: 5          # Need at least 5 calls to calculate rate
```

**Interview Language:**
- "We use Resilience4j circuit breakers on all inter-service HTTP calls. If a downstream service's failure rate exceeds 50% over a sliding window of 10 requests, the circuit trips to OPEN state. While open, calls fail fast with a fallback response rather than waiting for a timeout. This prevents thread pool exhaustion in the calling service."
- "The fallback strategy depends on the use case. For inventory checks, we return an optimistic response and validate at checkout. For recommendations, we return a pre-cached default list. The key is that the user still gets a response — they just see degraded functionality rather than an error page."

---

### Pattern 3: Request Queue Buffer

**What is it?** Instead of failing user-initiated actions when a downstream service is down, buffer those actions in a durable message queue and replay them once the service recovers.

**Why this matters:** Without buffering, if your Cart Service is down for 5 minutes, every "Add to Cart" click during that window returns an error. The user sees a failure, gives up, and you lose the sale. With buffering, the action is durably stored in Kafka. When the Cart Service comes back online, it processes all the queued events in order. The user sees their item in the cart, slightly delayed, but the action was never lost.

**How Kafka enables this:**

Kafka stores messages durably on disk across multiple brokers. Consumer groups track their position (offset) in each topic partition. When a consumer goes down and comes back, it resumes from where it left off — it does not miss any messages.

```java
@Component
public class AddToCartHandler {

    private final KafkaTemplate<String, AddToCartEvent> kafkaTemplate;
    private final CartService cartService;

    // Try to process immediately, but buffer if service is unavailable
    public void addToCart(String userId, String productId, int quantity) {
        AddToCartEvent event = new AddToCartEvent(userId, productId, quantity, Instant.now());
        try {
            cartService.addItem(userId, productId, quantity);
        } catch (ServiceUnavailableException e) {
            // Buffer the action — it will be processed when cart service recovers
            kafkaTemplate.send("add-to-cart-buffer", userId, event);
        }
    }

    // This listener drains the buffer when the cart service is healthy
    @KafkaListener(topics = "add-to-cart-buffer", groupId = "cart-buffer-consumer")
    public void replayBufferedAction(AddToCartEvent event) {
        cartService.addItem(event.getUserId(), event.getProductId(), event.getQuantity());
    }
}
```

**Interview Language:**
- "For user-initiated write operations, we use Kafka as a durability layer. If the target service is temporarily unavailable, we publish the action to a buffer topic. Spring Cloud Stream binds our handler to this topic. When the service recovers, the consumer group replays all buffered events from its last committed offset."
- "This pattern trades strong consistency for availability. The user's action is acknowledged immediately (it is in Kafka, which is durable), but may not be reflected until the downstream service processes it. We communicate this to the user with messages like 'Your item has been saved to cart and will appear shortly.'"

---

### Pattern 4: Canary and Blue-Green Deployment

**What is it?** Deployment strategies that allow you to ship new code to production without risking a full-scale outage if the new version has bugs.

**Blue-Green Deployment:**

Maintain two identical production environments called Blue and Green. At any point, one is live and the other is idle.

```
Internet Traffic
      |
      v
+-----+------+
| Load       |
| Balancer   |
+--+---------+
   |          \
   v            v
+-------+    +--------+
| BLUE  |    | GREEN  |
| (live)|    | (idle) |
+-------+    +--------+
```

To deploy a new version: deploy to Green, run smoke tests, then switch the load balancer to point to Green. Blue becomes the idle standby. If something goes wrong, one command switches traffic back to Blue (instant rollback). Zero downtime.

**Canary Deployment:**

Rather than switching all traffic at once, gradually shift a small percentage to the new version and monitor metrics before proceeding.

```
Stage 1:  95% → v1 (old),  5% → v2 (new) — monitor for 10 min
Stage 2:  75% → v1,       25% → v2        — monitor for 20 min
Stage 3:  25% → v1,       75% → v2        — monitor for 30 min
Stage 4:   0% → v1,      100% → v2        — complete
```

At each stage, you check: error rate, p99 latency, business metrics (conversion rate, checkout success rate). If any metric degrades, the deployment is automatically halted and traffic rolls back to v1. Only 5% of users were ever exposed to the bad version.

**ArgoCD Rollout configuration (Kubernetes):**

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: cart-service
spec:
  replicas: 20
  strategy:
    canary:
      steps:
        - setWeight: 5      # 5% canary
        - pause: {duration: 10m}
        - setWeight: 25     # 25% canary
        - pause: {duration: 20m}
        - setWeight: 50
        - pause: {duration: 20m}
      analysis:             # Auto-rollback if error rate > 1%
        templates:
          - templateName: error-rate-check
        args:
          - name: service-name
            value: cart-service
```

**Interview Language:**
- "We use canary deployments managed by ArgoCD for all services. New versions start at 5% traffic weight. If the error rate or p99 latency from the canary pods exceeds our SLO thresholds, ArgoCD automatically aborts the rollout and shifts traffic back to the stable version."
- "Blue-green is our preferred strategy for high-risk releases or database schema migrations — where we need the ability to instantly switch back. The idle environment also serves as a warm DR standby."

---

### Pattern 5: Feature Toggles and Kill Switches

**What is it?** The ability to enable or disable specific application features at runtime, without deploying new code.

**Why this matters:** Imagine you deploy a new coupon engine. Two hours later, you discover it has a bug that lets users stack unlimited coupons. Without a feature toggle, your only option is to emergency-deploy a revert — which takes 15-30 minutes, all while users exploit the bug. With a feature toggle (kill switch), you flip one flag in Redis and the coupon feature is disabled across all pods within seconds, without any deployment.

**How it works:**

FF4J (Feature Flipping for Java) is a library that reads feature flag state from a store (Redis for low-latency, database for persistence). Spring Cloud Config serves configuration from a Git repository, so flag changes are pulled automatically by all service instances.

```java
@Service
public class CartCheckoutService {

    @Autowired
    private FF4j ff4j;

    public CheckoutResponse checkout(Cart cart) {
        CheckoutResponse response = new CheckoutResponse();
        response.setSubtotal(cart.getSubtotal());

        // Feature toggle: only apply coupons if feature is enabled
        if (ff4j.check("coupon-engine-enabled")) {
            CouponResult coupon = couponService.applyCoupons(cart);
            response.setDiscount(coupon.getDiscount());
        } else {
            response.setDiscount(0); // Kill switch engaged — no coupons
        }

        response.setTotal(response.getSubtotal() - response.getDiscount());
        return response;
    }
}
```

**Spring Cloud Config with Redis for fast reads:**

```yaml
# application.yml
spring:
  cloud:
    config:
      uri: http://config-server:8888
ff4j:
  store:
    type: redis
  cache:
    ttl: 5  # Re-read toggle state from Redis every 5 seconds
```

**Interview Language:**
- "We wrap every major feature in an FF4J toggle backed by Redis. This gives us runtime control without deployment. During incidents, we can disable a broken feature within seconds. The toggle check is a single Redis GET, so it adds less than 1ms overhead to each request."
- "Feature toggles also serve as a tool for A/B testing and gradual rollouts. We can enable a feature for 10% of users, measure conversion metrics, and progressively roll it out — independent of the deployment cycle."

---

## 3. URL Shortener System Design at 1 Million RPS

### What Is It?

A URL shortener takes a long URL (e.g., `https://www.example.com/products/category/shoes/nike-air-max-2024?color=black&size=10`) and produces a short, shareable URL (e.g., `https://short.ly/xK7mP2q`). When a user visits the short URL, they are redirected to the original.

Designing one that handles 1 million requests per second requires careful thinking about encoding, caching, rate limiting, storage, and horizontal scaling.

### The Analogy

Think of a library catalog system. Each book has a long, descriptive title. The catalog gives every book a short call number (e.g., "QA76.9"). When you give a librarian that short number, they can find the exact book in seconds — because the call number maps uniquely to a shelf location. URL shortening is the same: a compact, unique code maps to the full resource location.

---

### Full Architecture Diagram

```
User Browser / Mobile App
         |
         v
+--------+---------+
|   CDN (Cloudflare|
|   / Akamai)      |
|   Edge Cache     |
+--------+---------+
         |  [cache miss]
         v
+--------+---------+
|  NGINX / HAProxy |
|  L7 Load Balancer|
+--------+---------+
         |
    +----+----+
    |         |
    v         v
+-------+ +-------+
|Shorten| |Redirect|    <- Two separate service pools
|Service| |Service |       (different load profiles)
+---+---+ +---+---+
    |           |
    v           v
+-------+   +--+----+
| Kafka |   | Redis |    <- Async write | Hot URL cache
|(async)|   | Cache |
+---+---+   +--+----+
    |           |  [cache miss]
    v           v
+---+--+    +--+-----+
|Cassandra/  |Cassandra|  <- Persistent storage
|ScyllaDB|   |ScyllaDB|
+--------+   +--------+
    |
    v
+---+------+
| Analytics|
| Pipeline |
| (Kafka → |
| Flink →  |
| Elastic) |
+----------+
```

---

### Component 1: URL Shortening — Base62 Encoding

**What is Base62?** It is an encoding scheme that uses 62 characters: lowercase letters (a-z = 26), uppercase letters (A-Z = 26), and digits (0-9 = 10). This means each character in a short code can represent one of 62 values.

**Why 7 characters?** With 7 characters in Base62, the number of unique combinations is:

```
62^7 = 3,521,614,606,208 ≈ 3.5 trillion unique short URLs
```

That is far more than any URL shortener will ever need.

**How to generate a short code without collisions:**

Option 1 — Hash and check (simple but slow at scale):

```java
public String generateShortCode(String longUrl) {
    String hash = DigestUtils.md5Hex(longUrl).substring(0, 7);
    String base62 = encodeToBase62(hash);
    // Check if this code already exists (collision)
    while (urlRepository.existsByShortCode(base62)) {
        base62 = encodeToBase62(hash + UUID.randomUUID().toString().substring(0, 4));
    }
    return base62;
}
```

Option 2 — Counter-based with Snowflake ID (preferred at scale):

Use a distributed ID generator (like Twitter's Snowflake) to produce a globally unique 64-bit integer for each new URL. Convert that integer to Base62. No collision possible because each ID is unique by construction.

```java
public String generateShortCode(long snowflakeId) {
    StringBuilder code = new StringBuilder();
    String BASE62_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    long remaining = snowflakeId;
    while (remaining > 0) {
        code.append(BASE62_CHARS.charAt((int)(remaining % 62)));
        remaining /= 62;
    }
    return code.reverse().toString();
}
```

**Async writes with Kafka:**

Writing to Cassandra on every shorten request adds latency. Instead, return the short code to the user immediately (it is deterministic from the Snowflake ID) and publish an event to Kafka. A background consumer writes to Cassandra asynchronously. The URL is stored in Redis immediately so redirects work right away.

---

### Component 2: Rate Limiting — Token Bucket Algorithm

**What is it?** Each user or IP gets a "bucket" that holds a maximum of N tokens. Every API call costs 1 token. The bucket refills at a fixed rate (e.g., 10 tokens per second). If the bucket is empty, the request is rejected with HTTP 429.

**Why token bucket (not fixed window)?** Fixed window rate limiting has a burst problem: a user can make 10 requests at 11:59:59 and 10 more at 12:00:00, effectively making 20 requests in 2 seconds while staying within the "10 per minute" rule. Token bucket handles bursts gracefully because it limits instantaneous rate.

**Redis + Lua script for atomic token bucket:**

```lua
-- rate_limit.lua
-- Keys: {bucket_key}
-- Args: {max_tokens, refill_rate, current_timestamp, cost}
local key = KEYS[1]
local max_tokens = tonumber(ARGV[1])
local refill_rate = tonumber(ARGV[2])    -- tokens per second
local now = tonumber(ARGV[3])
local cost = tonumber(ARGV[4])

local bucket = redis.call("HMGET", key, "tokens", "last_refill")
local tokens = tonumber(bucket[1]) or max_tokens
local last_refill = tonumber(bucket[2]) or now

-- Add tokens based on time elapsed
local elapsed = now - last_refill
local new_tokens = math.min(max_tokens, tokens + (elapsed * refill_rate))

if new_tokens >= cost then
    redis.call("HMSET", key, "tokens", new_tokens - cost, "last_refill", now)
    redis.call("EXPIRE", key, 3600)
    return 1  -- allowed
else
    return 0  -- rejected
end
```

**Java integration with Bucket4j:**

```java
@Component
public class RateLimitFilter implements HandlerInterceptor {

    private final RedisTemplate<String, String> redisTemplate;

    @Override
    public boolean preHandle(HttpServletRequest request, HttpServletResponse response, Object handler) {
        String clientIp = request.getRemoteAddr();
        boolean allowed = checkRateLimit(clientIp, 10, 10); // 10 tokens, 10/sec refill
        if (!allowed) {
            response.setStatus(429);
            return false;
        }
        return true;
    }
}
```

---

### Component 3: Redirection Flow — Achieving Sub-20ms Redirects

The redirection path is the hot path. It runs at 1 million RPS. Every millisecond matters.

**Layered cache strategy:**

1. **CDN Edge Cache (Cloudflare/Akamai):** For extremely popular short URLs (e.g., a viral link shared by a celebrity), CDN PoPs around the world cache the `301`/`302` redirect response. The request never reaches your servers. Response in ~5ms.

2. **Redis Cache:** For warm URLs (accessed in the last hour), Redis serves the lookup in memory. No database query needed. Response in ~15ms including network.

3. **Cassandra (Source of Truth):** Cache miss falls through to Cassandra. Wide-column store with the short code as partition key gives single-partition reads. Response in ~30-50ms.

```java
@RestController
public class RedirectController {

    @GetMapping("/{shortCode}")
    public ResponseEntity<Void> redirect(@PathVariable String shortCode) {
        // Layer 1: Redis cache
        String longUrl = redisCache.get("url:" + shortCode);

        if (longUrl == null) {
            // Layer 2: Cassandra source of truth
            longUrl = urlRepository.findByShortCode(shortCode)
                .orElseThrow(() -> new NotFoundException("Short URL not found"));
            // Populate Redis cache (TTL: 1 hour for popular URLs)
            redisCache.set("url:" + shortCode, longUrl, Duration.ofHours(1));
        }

        // Publish analytics event asynchronously
        analyticsKafkaTemplate.send("url-click-events",
            new UrlClickEvent(shortCode, Instant.now()));

        // 301 = permanent redirect (browser caches it)
        // 302 = temporary redirect (browser always re-requests — needed for analytics)
        return ResponseEntity.status(HttpStatus.FOUND)
            .header("Location", longUrl)
            .build();
    }
}
```

**301 vs 302:** Use `302 (Found)` rather than `301 (Moved Permanently)` if you want to track every click in your analytics. With 301, the browser caches the redirect and never calls your server again on subsequent visits — you lose click data.

---

### Component 4: Analytics Pipeline

```
User clicks short URL
        |
        v
Redirect Service publishes UrlClickEvent to Kafka
        |
        v
Kafka Topic: url-click-events
        |
        v
Flink Streaming Job:
  - Count clicks per URL per minute
  - Count unique visitors by IP hash
  - Detect rate-limit breach patterns
        |
        v
Elasticsearch (indexed by shortCode + timestamp)
        |
        v
Kibana Dashboard: "Top 100 links this hour"
```

---

### Component 5: Auto-Scaling

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: redirect-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: redirect-service
  minReplicas: 10
  maxReplicas: 200
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 60   # Scale up when CPU > 60%
    - type: Pods
      pods:
        metric:
          name: http_requests_per_second
        target:
          type: AverageValue
          averageValue: "5000"     # Scale up when each pod handles > 5K RPS
```

### Interview Language: URL Shortener

- "The core design decision is separating the write path (shortening) from the read path (redirecting). Redirects outnumber shortenings by orders of magnitude, so the redirect service needs to be independently scalable."
- "For short code generation, I use a distributed counter via a Snowflake ID generator rather than hash-based generation. This guarantees uniqueness without any collision checks or retry loops."
- "The hot path for redirects goes: CDN edge cache, then Redis, then Cassandra. Popular URLs rarely reach the database. We target under 20ms p99 latency for redirects."
- "We use a token bucket algorithm implemented as a Lua script in Redis for rate limiting. The Lua script is atomic at the Redis level, which prevents race conditions in a distributed environment where multiple application pods share the same Redis instance."

---

## 4. Flash Sale System Design — Flipkart Case Study

### What Is It?

A flash sale (like Flipkart's Big Billion Day or Amazon's Prime Day) is a planned, time-bounded event that creates massive, sudden traffic spikes — sometimes 10x or 100x normal load — sustained for hours. The system must handle millions of concurrent users browsing, clicking, adding to cart, and checking out, all at the same time, without downtime, data corruption, or overselling.

### The Analogy

Imagine Black Friday at a physical store, but all 10 million customers arrive at exactly 12:00 AM simultaneously and the store is trying to sell 1,000 limited-edition items. The challenge is: how do you let everyone browse freely, ensure only 1,000 people get the limited item, process everyone's payment securely, and not have the store collapse under the crowd?

---

### Layer 1: Frontend — CDN and Edge Caching

**The problem:** At 12:00:00 AM, 5 million users simultaneously request the Flipkart homepage. If every request hits your origin servers, they would collapse instantly.

**The solution:** Pre-warm your CDN before the sale starts. All static assets (product images, JavaScript bundles, CSS files, pre-rendered HTML for product pages) are pushed to CDN PoPs (Points of Presence) around the world hours before the sale. When users load the page, they are served by the nearest CDN node — not your origin servers.

**What still hits origin:** Dynamic content — live stock counts, user-specific recommendations, cart state, personalized pricing. This is the subset of requests that require real-time data.

**WebSockets vs HTTP Polling for live updates:**

During a flash sale, product prices and stock counts change every few seconds. If each of 5 million users polls `/api/stock?productId=123` every 2 seconds, that is 2.5 million requests per second just for stock polling. This would overwhelm any system.

Instead, use WebSockets or Server-Sent Events (SSE):
- The server pushes updates to connected clients when stock changes, rather than clients asking repeatedly.
- One stock update for product #123 triggers a single broadcast to all subscribed clients.
- This turns an O(users × polling_frequency) problem into an O(update_frequency) problem.

```
Without WebSockets: 5M users × 1 poll/2sec = 2.5M RPS just for stock updates
With WebSockets:    1 DB change event → 1 broadcast to all subscribed clients
```

---

### Layer 2: Inventory Management — The Hardest Problem

**The core challenge:** You have 1,000 units of a product. At 12:00:00 AM, 50,000 users click "Buy Now" simultaneously. How do you ensure exactly 1,000 purchases go through and 49,000 get "Out of Stock" — without selling 1,200 units (overselling) or only selling 800 (underselling due to race conditions)?

**Approach 1: Redis Atomic Decrement (preferred for flash sales)**

Store the available stock count in Redis. Use the atomic `DECR` command. Redis processes commands sequentially — it is single-threaded for command execution — so `DECR` on a key is guaranteed to be atomic without locks.

```java
@Service
public class InventoryService {

    private final RedisTemplate<String, Integer> redisTemplate;

    public boolean reserveStock(String productId) {
        String key = "stock:" + productId;
        Long remaining = redisTemplate.opsForValue().decrement(key);
        if (remaining != null && remaining >= 0) {
            return true;  // Stock reserved successfully
        } else {
            // Undo the decrement — we went below zero
            redisTemplate.opsForValue().increment(key);
            return false; // Out of stock
        }
    }
}
```

This is fast (single Redis operation, sub-millisecond) and serializes all stock operations without database locks.

**Approach 2: Database Optimistic Locking (for non-flash sale scenarios)**

```java
// JPA entity with version field
@Entity
public class Product {
    @Id
    private Long id;
    private int stockCount;

    @Version
    private Long version;   // Optimistic lock version
}

// Service layer
@Transactional
public boolean purchaseProduct(Long productId) {
    Product product = productRepository.findById(productId).orElseThrow();
    if (product.getStockCount() <= 0) return false;
    product.setStockCount(product.getStockCount() - 1);
    // If another transaction modified this row since we read it,
    // @Version mismatch throws OptimisticLockException — we retry
    productRepository.save(product);
    return true;
}
```

**Why not pessimistic locking for flash sales?** Pessimistic locking (`SELECT FOR UPDATE`) puts a row-level lock on the product record. With 50,000 concurrent users competing for the same lock, you create a massive queue of waiting database connections, overwhelming the connection pool and causing timeouts. Redis atomic operations are orders of magnitude faster.

---

### Layer 3: Payment Processing

**Asynchronous order processing flow:**

```
User clicks "Pay Now"
        |
        v
Payment Service: tokenized charge via Razorpay/PayU
        |
        v
Payment Gateway returns: PENDING (async)
        |
        v
Order Service publishes OrderPlacedEvent to Kafka
        |
        +--> Inventory Service: deduct confirmed stock
        +--> Fulfillment Service: create shipment record
        +--> Notification Service: send confirmation email/SMS
        +--> Fraud Detection Service: score the transaction
```

This event-driven approach means the user gets an immediate response ("Your order has been placed") rather than waiting for inventory deduction, fulfillment record creation, and email sending to all complete synchronously.

**Tokenized payments (PCI-DSS compliance):**

Never store raw card numbers. When a user saves their card, the payment gateway stores the card data and returns a token (a random string). All subsequent charges use the token. Even if your database is compromised, the attacker only gets tokens, which are useless without the payment gateway's private key.

**Fraud detection with ML scoring:**

Every transaction is scored by a fraud model in real-time (under 200ms). Features fed to the model include:
- Is this IP address associated with previous fraud?
- Is the shipping address consistent with the user's usual location?
- Is this the user's first purchase on this device?
- Is the order value unusually high for this user's history?
- Is there an unusual velocity of orders from this account (account takeover)?

High-risk transactions are flagged for manual review or challenged with 3D Secure.

---

### Layer 4: Database Strategy

```
Write Path:
  Orders, Payments → MySQL (ACID transactions, strong consistency)
  Inventory changes → Redis first, async sync to MySQL

Read Path:
  Product search → Elasticsearch (inverted index, full-text + faceted search)
  Recommendations → Redis (pre-computed, low latency reads)
  Product catalog → Redis cache → DynamoDB (high read throughput, low latency)
  User session data → Redis (fast key-value, auto-expiry)

Reporting / Analytics:
  Materialized views or read replicas of MySQL
  Kafka → Data warehouse (Redshift / BigQuery) for BI queries
```

**Why different databases for different use cases?** No single database is optimal for all access patterns. MySQL gives you ACID guarantees for financial transactions. Elasticsearch gives you full-text search with faceting (filter by brand, price range, rating). Redis gives you microsecond reads for session data and pre-computed results. DynamoDB gives you horizontal scale for the product catalog.

---

### Layer 5: Auto-Scaling for Planned Traffic Spikes

Unlike organic traffic growth (which HPA handles reactively), flash sales have a known start time. This means you can pre-scale:

```bash
# Pre-scale pods 30 minutes before the sale starts
kubectl scale deployment product-service --replicas=500
kubectl scale deployment cart-service --replicas=300
kubectl scale deployment payment-service --replicas=200
```

HPA handles reactive scaling after the event starts if load exceeds predictions. Cluster Autoscaler provisions new EC2/GCE nodes when existing nodes run out of capacity.

### Interview Language: Flash Sale

- "The key insight is that inventory management is the hardest part of a flash sale. We use Redis atomic DECR as the authoritative stock counter during the sale window because it provides atomic decrement without the lock contention of database-level pessimistic locking."
- "We separate the concern of payment authorization (synchronous, real-time) from fulfillment processing (asynchronous, event-driven via Kafka). Users get an immediate order confirmation, and downstream services process the event at their own pace."
- "For live price and stock updates, we use Server-Sent Events rather than HTTP polling. This inverts the request pattern — the server pushes changes to subscribed clients rather than each client independently polling. At 5 million concurrent users polling every 2 seconds, polling would generate 2.5 million RPS on stock endpoints alone."

---

## 5. Kubernetes Secrets Management

### What Is It?

A Kubernetes Secret is a first-class API object for storing sensitive configuration data: database passwords, API keys, TLS certificates, OAuth tokens, and any other credential that should not be embedded in your application code or Docker image.

### The Problem with Hardcoded Secrets

**Scenario:** A junior developer writes:

```java
// DO NOT DO THIS
String dbPassword = "MyProductionPassword123!";
DataSource ds = DataSourceBuilder.create()
    .url("jdbc:postgresql://prod-db:5432/myapp")
    .password(dbPassword)
    .build();
```

This code is committed to Git. Now:
- Every developer who has ever cloned the repo has the production password.
- The password exists in every Git clone's history forever, even after you delete the line.
- Every Docker image built from this code contains the password in its layers.
- Rotating the password requires a code change and full redeploy.

This is why Kubernetes Secrets exist.

---

### The 3 Types of Kubernetes Secrets

**Type 1: Opaque**

The general-purpose secret type for arbitrary key-value data. The name "opaque" means Kubernetes does not validate or interpret the contents.

```bash
kubectl create secret generic db-credentials \
  --from-literal=username=admin \
  --from-literal=password=SuperSecret123
```

Equivalent YAML:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-credentials
  namespace: production
type: Opaque
data:
  username: YWRtaW4=         # base64 of "admin"
  password: U3VwZXJTZWNyZXQxMjM=  # base64 of "SuperSecret123"
```

**Type 2: kubernetes.io/dockerconfigjson**

Used for authenticating with private Docker registries. When Kubernetes pulls your container image from a private registry (like AWS ECR, Google Artifact Registry, or a private Harbor instance), it uses this secret.

```bash
kubectl create secret docker-registry registry-credentials \
  --docker-server=my-registry.example.com \
  --docker-username=myuser \
  --docker-password=mypassword
```

**Type 3: kubernetes.io/tls**

For storing TLS certificate and private key pairs used by Ingress controllers for HTTPS termination.

```bash
kubectl create secret tls my-tls-cert \
  --cert=path/to/tls.crt \
  --key=path/to/tls.key
```

---

### Consuming Secrets in Pods

**Method 1: Environment variable (secretKeyRef)**

Inject individual secret values as environment variables. Simple and widely compatible.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: my-app-pod
spec:
  containers:
    - name: my-app
      image: my-app:1.0
      env:
        - name: DB_USERNAME
          valueFrom:
            secretKeyRef:
              name: db-credentials   # Secret name
              key: username           # Key within the secret
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: password
```

**Method 2: envFrom (inject all keys from a secret)**

```yaml
spec:
  containers:
    - name: my-app
      image: my-app:1.0
      envFrom:
        - secretRef:
            name: db-credentials    # All keys become environment variables
```

**Method 3: Volume mount (secret as files)**

Best for TLS certificates or config files that the application reads from the filesystem.

```yaml
spec:
  volumes:
    - name: db-creds-volume
      secret:
        secretName: db-credentials
  containers:
    - name: my-app
      image: my-app:1.0
      volumeMounts:
        - name: db-creds-volume
          mountPath: /etc/secrets    # Files appear at /etc/secrets/username and /etc/secrets/password
          readOnly: true
```

---

### CRITICAL: Base64 is NOT Encryption

This is one of the most important points to understand and communicate in an interview.

The values in a Kubernetes Secret are stored as base64-encoded strings. Base64 is an *encoding*, not *encryption*. Anyone who can read the Secret object can decode it in one command:

```bash
echo "U3VwZXJTZWNyZXQxMjM=" | base64 --decode
# Output: SuperSecret123
```

By default, Kubernetes stores all objects (including Secrets) in etcd as plain JSON. This means:
- Anyone with direct access to etcd (the Kubernetes datastore) can read all your secrets.
- etcd backups contain all secrets in plaintext.
- Kubernetes API access with the right RBAC permissions exposes secrets.

**Kubernetes Secrets are a convenience API for distributing configuration, not a security vault.** Security requires additional layers.

---

### Best Practice 1: Encryption at Rest with KMS

Configure Kubernetes to encrypt Secret objects before writing them to etcd using a Key Management Service (KMS).

```yaml
# encryption-config.yaml (applied to kube-apiserver)
apiVersion: apiserver.config.k8s.io/v1
kind: EncryptionConfiguration
resources:
  - resources:
      - secrets
    providers:
      - kms:
          name: aws-kms
          endpoint: unix:///var/run/kmsplugin/socket.sock
          cachesize: 1000
          timeout: 3s
      - identity: {}   # Fallback (unencrypted) — should be last
```

With this configuration, when Kubernetes writes a Secret to etcd, it first encrypts the value using the KMS key. Even if an attacker gets a copy of your etcd database, they cannot read the secrets without the KMS key — which is managed separately by AWS KMS or Azure Key Vault.

**Why this matters:** If your Kubernetes cluster is on a cloud provider, etcd is managed for you, but you should still enable KMS envelope encryption. This is a compliance requirement for PCI-DSS, HIPAA, and SOC 2.

---

### Best Practice 2: External Secrets Operator

The External Secrets Operator (ESO) is a Kubernetes operator that automatically syncs secrets from external secret managers (HashiCorp Vault, AWS Secrets Manager, Azure Key Vault, GCP Secret Manager) into Kubernetes Secrets.

**The problem it solves:** Kubernetes Secrets are cluster-local. You have to manually create them, and they get out of sync when you rotate secrets in your external vault. ESO automates this sync.

```yaml
# ExternalSecret: tells ESO where to pull the secret from
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: db-credentials
spec:
  refreshInterval: 1h         # Re-sync with AWS Secrets Manager every hour
  secretStoreRef:
    name: aws-secrets-manager
    kind: ClusterSecretStore
  target:
    name: db-credentials      # Name of the Kubernetes Secret to create/update
    creationPolicy: Owner
  data:
    - secretKey: password     # Key in the Kubernetes Secret
      remoteRef:
        key: prod/myapp/db    # Key in AWS Secrets Manager
        property: password    # Property within the Secrets Manager JSON
```

With ESO, your secrets live in your enterprise vault (which has audit logging, fine-grained access control, and rotation policies). Kubernetes Secrets become ephemeral — they are automatically recreated from the vault source of truth.

---

### Best Practice 3: RBAC for Secrets

By default in many Kubernetes setups, service accounts have broad permissions. You must explicitly restrict which service accounts can read which secrets.

```yaml
# Role: grant read access to only the db-credentials secret
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: db-credentials-reader
  namespace: production
rules:
  - apiGroups: [""]
    resources: ["secrets"]
    resourceNames: ["db-credentials"]   # Only this specific secret
    verbs: ["get"]                        # Only read, not list/watch/update

---
# RoleBinding: bind the role to the application's service account
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: my-app-db-credentials-binding
  namespace: production
subjects:
  - kind: ServiceAccount
    name: my-app-service-account
    namespace: production
roleRef:
  kind: Role
  name: db-credentials-reader
  apiGroup: rbac.authorization.k8s.io
```

**Why this matters:** The principle of least privilege. If your application pod is compromised, the attacker can only read the specific secrets that pod's service account has access to — not every secret in the cluster.

---

### Best Practice 4: Secret Rotation Without Downtime

Rotating secrets (changing passwords periodically or after a suspected breach) should not require downtime. The recommended approach:

1. Create the new credential in your external vault.
2. The External Secrets Operator detects the change and updates the Kubernetes Secret.
3. Trigger a rolling restart of affected deployments:

```bash
kubectl rollout restart deployment/my-app
```

Kubernetes performs a rolling update: new pods start with the new secret, old pods are terminated gradually. Zero downtime if you have `minReadySeconds` and appropriate `PodDisruptionBudget` configured.

**Never store secrets in:**
- Source code or Git repositories
- Docker image layers (`RUN export PASSWORD=...` in Dockerfile history)
- Application logs (avoid logging configuration objects that might contain credentials)
- CI/CD environment variables that are printed to build logs

---

### Interview Language: Kubernetes Secrets

- "Kubernetes Secrets provide a way to decouple sensitive configuration from application code and container images. They are mounted into pods as environment variables or volume files at runtime, so the secret never needs to be baked into the image."
- "An important caveat: Kubernetes Secrets are base64 encoded, not encrypted. To actually secure them at rest, you need to configure envelope encryption with a KMS provider — this means the kube-apiserver encrypts the secret payload before writing to etcd."
- "In production, we use the External Secrets Operator to sync secrets from AWS Secrets Manager into Kubernetes. This gives us centralized secret management with rotation, audit logging, and versioning — and the Kubernetes Secrets are automatically kept in sync."
- "We enforce least-privilege access using RBAC. Each service account has a Role that grants access only to the specific Secrets it needs, scoped to its namespace. This limits the blast radius if any pod is compromised."
- "Secret rotation is handled by updating the value in AWS Secrets Manager. ESO picks up the change within the configured refresh interval and updates the Kubernetes Secret. A rolling restart of the deployment picks up the new value without any downtime."

---

## Summary Reference Card

| Concept | Core Problem Solved | Key Technologies |
|---|---|---|
| Retry Mechanism | Transient failures silently drop jobs | Kafka, Spring Retry, Resilience4j, DLQ |
| Health Guard | Failures go undetected until users complain | Spring Actuator, Prometheus, Grafana, Alertmanager |
| Circuit Breaker | One failing service cascades into full outage | Resilience4j, `@CircuitBreaker`, fallback methods |
| Request Buffer | User actions lost during outages | Kafka, consumer group offsets, replay semantics |
| Canary Deploy | Bugs in new releases impact all users at once | ArgoCD Rollouts, Kubernetes, metric gates |
| Feature Toggle | Fixing bugs requires full redeploy under pressure | FF4J, Spring Cloud Config, Redis |
| URL Shortener | Scale short URL generation to 1M RPS | Base62, Snowflake ID, Redis, Cassandra, CDN |
| Flash Sale | Overselling, downtime under 100x traffic spike | Redis DECR, Kafka, WebSockets, pre-scaling |
| K8s Secrets | Credentials hardcoded in source or images | Secret types, RBAC, KMS, External Secrets Operator |

---

*Last updated: 2026-07-31 — For interview preparation purposes.*
