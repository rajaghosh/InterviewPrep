# Observability: Metrics, Logs & Traces

> Source: AlgoMap.io

---

## Monitoring vs. Observability

| | Monitoring | Observability |
|---|---|---|
| **Focus** | Pre-defined dashboards | Any question about system state |
| **Handles** | "Known Unknowns" | "Unknown Unknowns" |
| **Example** | Is CPU usage > 90%? | Why is checkout slow for some users? |
| **Tells you** | That something is wrong | **Why** it is wrong |

**Monitoring** tracks "Known Unknowns" — metrics you anticipated in advance.  
**Observability** is the ability to answer **any** question about your system by looking at the data it produces. It's for **"Unknown Unknowns."**

> Monitoring tells you that something is wrong. Observability tells you **why.**

```mermaid
flowchart LR
    subgraph Monitoring["Monitoring (Known Unknowns)"]
        M1["CPU > 90%?"]
        M2["Error rate > 5%?"]
        M3["Latency > 500ms?"]
    end
    subgraph Observability["Observability (Unknown Unknowns)"]
        O1["Why is checkout slow?"]
        O2["Which service caused the spike?"]
        O3["What changed at 10:02 AM?"]
    end
    Monitoring -->|"alerts when threshold breached"| Alert["Alert Fired"]
    Observability -->|"drill down with data"| RCA["Root Cause Analysis"]
```

---

## The Three Pillars of Observability

```mermaid
flowchart TD
    OBS["Observability"] --> M["Pillar 1: Metrics"]
    OBS --> L["Pillar 2: Logs"]
    OBS --> T["Pillar 3: Traces"]

    M --> M1["Numeric time-series data\nCPU, Memory, RPS, Error Rates"]
    L --> L1["Immutable timestamped events\nDetailed play-by-play of what happened"]
    T --> T1["Request journey across microservices\nShows exactly where lag occurred"]
```

---

## Pillar 1: Metrics

**Metrics** are numeric data measured over intervals of time.

- **Examples:** CPU usage, Memory, Requests per second, Error rates
- Great for **spotting trends** and **triggering alerts**

```mermaid
flowchart LR
    SVC["Service"] -->|"emit"| METRIC["Metric\n(numeric + timestamp)"]
    METRIC --> TSDB["Time-Series DB\n(Prometheus, InfluxDB)"]
    TSDB --> DASH["Dashboard\n(Grafana)"]
    TSDB --> ALERT["Alerting\n(PagerDuty)"]
```

**Interview Q: What is a metric and when would you use it?**  
**A:** A metric is a numeric measurement sampled at regular intervals (e.g., CPU %, requests/sec). Use metrics to track system health trends over time, set alert thresholds, and detect anomalies like traffic spikes or resource exhaustion. They are cheap to store and fast to query but lack context about *why* a value changed.

---

## Pillar 2: Logs

An **immutable, timestamped record of discrete events.**

- A detailed play-by-play of what happened: `"User 123 clicked 'Buy' at 10:01:05 AM."`
- Great for **deep-diving into a specific error** after it happens

```mermaid
flowchart LR
    APP["Application\nEvent occurs"] -->|"structured log"| LOGAGG["Log Aggregator\n(Fluentd / Logstash)"]
    LOGAGG --> STORE["Log Store\n(Elasticsearch / CloudWatch)"]
    STORE --> QUERY["Query & Search\n(Kibana / Splunk)"]
```

**Interview Q: What is a log and how does it differ from a metric?**  
**A:** A log is an immutable, timestamped record of a discrete event (errors, state changes, user actions). Unlike metrics which are aggregated numbers, logs capture rich context — the exact error message, user ID, stack trace. The trade-off: logs are verbose and expensive to store at scale. Use structured logging (JSON) to make them queryable efficiently.

---

## Pillar 3: Traces

**Tracing** is following a single request as it travels through multiple microservices.

- In a big system, a "Slow Checkout" could be caused by the Database, the Payment API, or the Shipping service
- A Trace shows you **exactly** where the "lag" happened in the chain

```mermaid
sequenceDiagram
    participant U as User
    participant API as API Gateway
    participant AUTH as Auth Service
    participant CART as Cart Service
    participant PAY as Payment API
    participant DB as Database

    U->>API: POST /checkout (Trace ID: abc123)
    API->>AUTH: Validate token [5ms]
    AUTH-->>API: OK
    API->>CART: Get cart items [10ms]
    CART->>DB: SELECT cart [8ms]
    DB-->>CART: rows
    CART-->>API: items
    API->>PAY: Charge card [350ms ⚠️]
    PAY-->>API: Success
    API-->>U: Order confirmed [Total: 375ms]

    Note over PAY: Bottleneck identified!
```

**Interview Q: What is distributed tracing and why is it important in microservices?**  
**A:** Distributed tracing assigns a unique Trace ID to each request and propagates it through every service call. Each service records a "span" with its start time and duration. Combining all spans gives a complete timeline showing exactly which service introduced latency. Without tracing, isolating performance issues in a 10+ service system is nearly impossible — you'd only see the total end-to-end time at the API gateway.

---

## Why Do We Need All Three?

| Pillar | Tells You |
|---|---|
| **Metrics** | There's a spike in errors |
| **Traces** | Which specific service is failing |
| **Logs** | The exact code error in that service |

Together, you can determine **exactly why** any complex issue is happening.

```mermaid
flowchart TD
    INC["Incident: Checkout is Slow"] --> M["Metrics\nError rate spiked at 10:02 AM"]
    M --> T["Traces\nPayment service taking 2s\n(normal: 50ms)"]
    T --> L["Logs\nTimeoutException: DB connection pool\nexhausted at 10:02:03.412"]
    L --> RCA["Root Cause Found!\nDB connection pool too small\nunder Black Friday load"]
```

**Interview Q: How do metrics, logs, and traces work together for incident response?**  
**A:**
1. **Metrics alert** you that error rate crossed 5% — the *signal* that something is wrong
2. **Traces** let you drill into affected requests to find *which service* in the call chain is slow or erroring
3. **Logs** from that specific service show the *exact error*, stack trace, and context (user ID, request payload)

This three-step drill-down from "something is wrong" → "where" → "why" is the core observability workflow. Tools like Datadog, New Relic, and Honeycomb unify all three pillars on a single platform for faster MTTR (Mean Time To Resolution).

---

## Key Tooling

| Pillar | Open Source | Cloud / SaaS |
|---|---|---|
| Metrics | Prometheus + Grafana | Datadog, CloudWatch |
| Logs | ELK Stack (Elasticsearch, Logstash, Kibana) | Splunk, CloudWatch Logs |
| Traces | Jaeger, Zipkin, OpenTelemetry | Datadog APM, AWS X-Ray, Honeycomb |
| All-in-one | OpenTelemetry (collection standard) | Datadog, New Relic, Dynatrace |

> **OpenTelemetry** is the CNCF standard for instrumenting code — it provides vendor-neutral SDKs to emit metrics, logs, and traces, and you route them to any backend.
