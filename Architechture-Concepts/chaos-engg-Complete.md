# Chaos Engineering — Complete Reference (.NET)

---

## Table of Contents

1. [What is Chaos Engineering?](#1-what-is-chaos-engineering)
2. [The Core Methodology — Scientific Chaos Experiment](#2-the-core-methodology--scientific-chaos-experiment)
3. [Principles of Chaos Engineering](#3-principles-of-chaos-engineering)
4. [Fault Injection Categories](#4-fault-injection-categories)
5. [Chaos Engineering Tools](#5-chaos-engineering-tools)
6. [Implementing Chaos Engineering in .NET & Azure](#6-implementing-chaos-engineering-in-net--azure)
7. [Observability & Metrics During Chaos](#7-observability--metrics-during-chaos)
8. [Cross-Cutting Themes](#cross-cutting-themes)

---

## 1. What is Chaos Engineering?

### Overview

Chaos Engineering is the discipline of experimenting on a distributed system to build confidence in its capability to withstand turbulent, unpredictable conditions. Popularized by Netflix's "Chaos Monkey" in 2011, it shifted the industry from reactive incident response to proactive resilience validation. Unlike load testing or unit testing, chaos experiments target production-like conditions with real traffic, deliberately introducing failures to surface hidden weaknesses before they cascade into outages. The core insight is that systems fail in ways that cannot be predicted by reading code — they must be observed under stress.

### Architecture Diagram

```mermaid
flowchart TD
    subgraph Inputs["Chaos Inputs"]
        SS["Steady State\nBaseline"]
        HYP["Hypothesis\nFormation"]
    end

    subgraph Injection["Fault Injection Layer"]
        NET["Network Latency\nPartition"]
        CPU["CPU / Memory\nExhaustion"]
        SVC["Service Crash\nKill Pod"]
        DEP["Dependency\nFailure"]
    end

    subgraph Observe["Observation Layer"]
        MTR["Metrics\nPrometheus / Azure Monitor"]
        LOG["Logs\nApp Insights / ELK"]
        TRC["Traces\nOpenTelemetry"]
    end

    subgraph Outcome["Outcome"]
        PASS["System Maintains\nSteady State"]
        FAIL["Degradation\nDetected → Fix"]
    end

    SS --> HYP
    HYP --> NET & CPU & SVC & DEP
    NET & CPU & SVC & DEP --> MTR & LOG & TRC
    MTR & LOG & TRC --> PASS & FAIL
    FAIL -->|"Fix & Repeat"| HYP

    classDef input fill:#0f172a,color:#fff
    classDef inject fill:#ef4444,color:#fff
    classDef observe fill:#8b5cf6,color:#fff
    classDef pass fill:#22c55e,color:#fff
    classDef fail fill:#f59e0b,color:#fff

    class SS,HYP input
    class NET,CPU,SVC,DEP inject
    class MTR,LOG,TRC observe
    class PASS pass
    class FAIL fail
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What is Chaos Engineering? | Proactively injecting controlled failures into production-like systems to discover resilience gaps before real incidents occur. |
| How does it differ from load testing? | Load testing measures performance under scale; chaos engineering tests structural resilience under failure scenarios like crashes, partitions, and dependency outages. |
| Why was Chaos Monkey invented? | Netflix needed confidence that their globally distributed system could survive hardware failures without human intervention at 3am. |
| What is the key cultural shift it drives? | From "our system is resilient" as an assumption to "our system is resilient" as a verified, continuously tested fact. |
| When is chaos engineering NOT appropriate? | Early-stage systems with no redundancy, systems without observability, or environments where blast radius cannot be controlled. |

---

## 2. The Core Methodology — Scientific Chaos Experiment

### Overview

A chaos experiment is not random destruction — it follows a rigorous scientific method with four phases: define steady state, form a hypothesis, inject a variable, and verify the outcome. This discipline prevents chaos experiments from causing unplanned outages and ensures every failure discovered has a traceable fix. The key differentiator from "just breaking things" is the pre-defined steady-state metric that objectively determines whether the system passed or failed.

### Experiment Lifecycle Flowchart

```mermaid
flowchart TD
    START(["Begin Chaos Experiment"]) --> SS

    subgraph Phase1["Phase 1 — Define Steady State"]
        SS["Identify SLI / SLO\np99 latency < 200ms\nerror rate < 0.1%"]
        SS --> BASELINE["Capture Baseline\n5-min rolling average\nfrom Prometheus"]
    end

    subgraph Phase2["Phase 2 — Hypothesize"]
        BASELINE --> HYP["Write Hypothesis:\n'If service B fails,\nservice A degrades gracefully\nvia circuit breaker'"]
    end

    subgraph Phase3["Phase 3 — Inject"]
        HYP --> SCOPE["Define Blast Radius\n1 pod / 1 AZ / canary only"]
        SCOPE --> INJECT["Inject Fault:\nKill pod / add 500ms latency\n/ exhaust CPU"]
        INJECT --> MONITOR["Monitor in Real-Time\nAbort conditions active"]
    end

    subgraph Phase4["Phase 4 — Verify"]
        MONITOR --> CHECK{Steady State\nMaintained?}
        CHECK -->|Yes| PASS["Hypothesis Confirmed\nSystem is Resilient"]
        CHECK -->|No| FAIL["Hypothesis Violated\nDocument Weakness"]
        FAIL --> FIX["Fix: Implement Polly Retry\nCircuit Breaker / Bulkhead"]
        FIX --> START
    end

    PASS --> EXPAND["Expand Blast Radius\nor Design Next Experiment"]

    style START fill:#0f172a,color:#fff
    style PASS fill:#22c55e,color:#fff
    style FAIL fill:#ef4444,color:#fff
    style FIX fill:#f59e0b,color:#fff
    style Phase1 fill:#eff6ff,stroke:#1e40af
    style Phase2 fill:#f0fdf4,stroke:#22c55e
    style Phase3 fill:#fef2f2,stroke:#ef4444
    style Phase4 fill:#fefce8,stroke:#f59e0b
```

### Experiment Execution Sequence

```mermaid
sequenceDiagram
    participant Eng as Engineer
    participant Tool as Chaos Tool
    participant SUT as System Under Test
    participant OBS as Observability Stack

    Eng->>OBS: Capture steady state baseline
    OBS-->>Eng: p99=180ms, error=0.02%

    Eng->>Tool: Define experiment (kill 1 of 3 pods)
    Eng->>Tool: Set abort condition (error > 5%)

    Tool->>SUT: Inject fault (SIGKILL pod-2)
    activate SUT
    SUT->>OBS: Metrics stream continues
    OBS-->>Eng: p99=220ms, error=0.04% (within SLO)

    Tool->>SUT: Restore pod-2
    deactivate SUT

    SUT->>OBS: Return to baseline
    OBS-->>Eng: p99=182ms, error=0.02%
    Eng->>Eng: Hypothesis CONFIRMED
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What is "steady state" in chaos engineering? | A measurable signal proving normal system behavior — e.g., p99 latency < 200ms, error rate < 0.1%, throughput > 1000 rps over a rolling window. |
| Why define steady state before injecting? | Without a baseline, you cannot detect degradation — you need a comparison point to determine if the system changed. |
| What is blast radius? | The scope of impact of a chaos experiment. Always start small (1 pod, canary) and expand only after each level passes. |
| How do you write a hypothesis? | "If [failure condition], then [expected system behavior] because [mechanism — e.g., circuit breaker, retry]." |
| What happens when a hypothesis fails? | A real weakness is discovered. Document it, implement a fix (Polly retry, circuit breaker, bulkhead), and re-run until the experiment passes. |
| How long should an experiment run? | Long enough for statistically significant data — typically 5–30 minutes, not instantaneous. |

---

## 3. Principles of Chaos Engineering

### Overview

The [Principles of Chaos Engineering](https://principlesofchaos.org/) define five core tenets that distinguish disciplined chaos engineering from reckless failure injection. The three most critical are: target real production traffic (not synthetic), automate experiments to run continuously, and always minimize blast radius. These principles ensure chaos experiments generate actionable signal without causing unnecessary user impact.

### Principles Architecture

```mermaid
flowchart LR
    subgraph P1["Principle 1"]
        PROD["Build on\nProduction Traffic\nReal users, real load"]
    end

    subgraph P2["Principle 2"]
        AUTO["Automate\nContinuously\nCI/CD pipeline integration"]
    end

    subgraph P3["Principle 3"]
        BLAST["Minimize\nBlast Radius\nCanary → Region → Global"]
    end

    subgraph P4["Principle 4"]
        VARY["Vary Real-World\nEvents\nNetwork, hardware, app-layer"]
    end

    subgraph P5["Principle 5"]
        HYP["Hypothesize\nSteady State\nSLI / SLO driven"]
    end

    PROD -->|"Builds real confidence"| AUTO
    AUTO -->|"Feedback loop"| BLAST
    BLAST -->|"Expands safely"| VARY
    VARY -->|"New scenarios"| HYP
    HYP -->|"Validated resilience"| PROD

    classDef principle fill:#1e40af,color:#fff
    class PROD,AUTO,BLAST,VARY,HYP principle
```

### Blast Radius Expansion Strategy

```mermaid
stateDiagram-v2
    [*] --> LocalDev : Start Here
    LocalDev --> StagingEnv : Experiment passes locally
    StagingEnv --> CanaryProd : Passes in staging
    CanaryProd --> SingleRegion : Canary unaffected
    SingleRegion --> MultiRegion : Single region validated
    MultiRegion --> Global : Full confidence

    LocalDev : Local Dev\nNo real traffic risk
    StagingEnv : Staging Environment\nProduction-like, no real users
    CanaryProd : Canary Production\n1-5 percent of real traffic
    SingleRegion : Single Region\nEast US only
    MultiRegion : Multi-Region\nEast US plus West US
    Global : Global Production\nFull blast radius
```

### Interview Talking Points

| Question | Answer |
|---|---|
| Why must chaos experiments use real production traffic? | Staging environments behave differently under real load; synthetic traffic misses CDN behavior, DB connection pool exhaustion, and cache warming effects. |
| What does "automate continuously" mean in practice? | Chaos experiments run automatically in CI/CD pipelines or on a schedule — not just once per quarter. Netflix ran Chaos Monkey daily. |
| How do you minimize blast radius? | Scope to 1 pod, then 1 AZ, then 1 region. Use feature flags or canary deployments to limit scope. Never start globally. |
| What is the difference between chaos engineering and fault injection? | Fault injection is a technique; chaos engineering is a discipline with hypotheses, steady-state definitions, and continuous automation. |
| How do you get organizational buy-in? | Run a GameDay in non-production. Show the team what their system does under failure — things they assumed worked often do not. |

---

## 4. Fault Injection Categories

### Overview

Real-world failures cluster into four categories: infrastructure failures (hardware, VMs, pods), network failures (latency, packet loss, partitions), application failures (crashes, memory leaks, slow dependencies), and data failures (corruption, inconsistency). A mature chaos engineering program targets all four systematically, ensuring no failure class is left untested.

### Fault Taxonomy Diagram

```mermaid
flowchart TD
    ROOT(["Fault Injection\nCategories"]) --> INFRA & NET & APP & DATA

    subgraph INFRA["Infrastructure Failures"]
        I1["Pod / VM Kill\nSIGKILL process"]
        I2["CPU Exhaustion\n100% CPU burn"]
        I3["Memory Pressure\nOOM killer trigger"]
        I4["Disk Exhaustion\nFill /var/log"]
    end

    subgraph NET["Network Failures"]
        N1["Latency Injection\n+500ms or +2000ms"]
        N2["Packet Loss\n10% or 50% drop rate"]
        N3["Network Partition\nBlock service-to-service"]
        N4["DNS Failure\nBad DNS resolution"]
    end

    subgraph APP["Application Failures"]
        A1["Dependency Timeout\nUpstream 30s timeout"]
        A2["Exception Storm\nForce 500 errors"]
        A3["Thread Pool Starvation\nExhaust HttpClient pool"]
        A4["Cache Miss Storm\nFlush Redis / invalidate"]
    end

    subgraph DATA["Data / State Failures"]
        D1["DB Connection Drop\nClose SQL connections"]
        D2["Slow Query\nArtificial DB delay"]
        D3["Message Queue Lag\nStop consumers artificially"]
        D4["Clock Skew\nAdvance system time"]
    end

    classDef infra fill:#ef4444,color:#fff
    classDef net fill:#f59e0b,color:#fff
    classDef app fill:#8b5cf6,color:#fff
    classDef data fill:#1e40af,color:#fff

    class I1,I2,I3,I4 infra
    class N1,N2,N3,N4 net
    class A1,A2,A3,A4 app
    class D1,D2,D3,D4 data
```

### Thread Pool Starvation — The Silent Killer

```mermaid
sequenceDiagram
    participant Client as Client
    participant API as .NET API
    participant Pool as ThreadPool
    participant Dep as Slow Dependency

    loop 100 concurrent requests
        Client->>API: HTTP GET /orders
        API->>Pool: Acquire thread
        Pool->>Dep: Call downstream (awaiting)
        Note over Dep: Dependency slow (2s latency injected)
        Note over Pool: Threads exhausted — queue depth grows
    end

    Client->>API: HTTP GET /healthz
    API->>Pool: Acquire thread
    Note over Pool: No threads available — health check times out
    API-->>Client: 503 Service Unavailable
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What types of faults should chaos experiments cover? | Infrastructure (CPU/memory/disk), network (latency/partition/packet loss), application (exceptions/timeouts), and data (DB drops, cache misses, clock skew). |
| Why is network latency more dangerous than a service crash? | A crash triggers circuit breakers immediately; latency causes thread pool starvation as requests pile up waiting, producing cascading failures. |
| What is a "clock skew" fault? | Advancing or skewing system time to test time-sensitive logic: JWT expiry, cache TTL, distributed lock timeouts, and scheduled job windows. |
| Which fault is hardest to detect? | Memory leaks and thread pool starvation — they are gradual and only manifest under sustained load, making them hard to reproduce in short experiments. |
| How do you test DB failover? | Inject a DB connection drop or kill the primary, then verify the system fails over to the replica within the expected RTO. |

---

## 5. Chaos Engineering Tools

### Overview

Three major tools dominate the chaos engineering landscape: Chaos Mesh (Kubernetes-native, CNCF graduated), Gremlin (commercial SaaS with pre-built attack scenarios), and LitmusChaos (open-source, Kubernetes-focused, ChaosCenter UI). Azure-native workloads additionally use Azure Chaos Studio, which targets ARM resources directly. Tool selection depends on environment: Kubernetes shops default to Chaos Mesh or LitmusChaos; teams needing enterprise pre-built scenarios use Gremlin; Azure-native teams use Azure Chaos Studio.

### Tool Comparison Diagram

```mermaid
flowchart LR
    subgraph Tools["Chaos Engineering Tools"]
        CM["Chaos Mesh\nCNCF Graduated\nKubernetes-native\nCRD-based experiments"]
        GR["Gremlin\nCommercial SaaS\nPre-built attack templates\nEnterprise support"]
        LM["LitmusChaos\nOpen Source\nChaosCenter UI\nWorkflow orchestration"]
        ACS["Azure Chaos Studio\nCloud-native\nARM-integrated\nAzure services targeting"]
    end

    subgraph Targets["What They Target"]
        K8S["Kubernetes\nPods / Nodes / Services"]
        VM["VMs / VMSS\nAzure Compute"]
        NET2["Network Layer\nVNet / NSG / DNS"]
        SVC["Azure Services\nAKS / Service Bus / Cosmos DB"]
    end

    CM -->|"CRD experiments"| K8S
    LM -->|"ChaosEngine CR"| K8S
    GR -->|"Agent-based"| K8S & VM
    ACS -->|"ARM resource targeting"| VM & NET2 & SVC

    classDef tool fill:#0078D4,color:#fff
    classDef target fill:#1e40af,color:#fff
    class CM,GR,LM,ACS tool
    class K8S,VM,NET2,SVC target
```

### Azure Chaos Studio — .NET Integration

**Tech Stack:** Azure Chaos Studio, Azure SDK for .NET, Azure Resource Manager, DefaultAzureCredential

```csharp
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Chaos;

public class ChaosExperimentOrchestrator
{
    private readonly ArmClient _armClient;
    private readonly ILogger<ChaosExperimentOrchestrator> _logger;

    public ChaosExperimentOrchestrator(ILogger<ChaosExperimentOrchestrator> logger)
    {
        _armClient = new ArmClient(new DefaultAzureCredential());
        _logger = logger;
    }

    public async Task StartExperimentAsync(
        string subscriptionId,
        string resourceGroupName,
        string experimentName,
        CancellationToken ct = default)
    {
        var resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(
            subscriptionId, resourceGroupName);

        var resourceGroup = _armClient.GetResourceGroupResource(resourceGroupId);
        var experiments = resourceGroup.GetChaosExperiments();
        var experiment = await experiments.GetAsync(experimentName, ct);

        await experiment.Value.StartAsync(Azure.WaitUntil.Started, ct);
        _logger.LogInformation("Chaos experiment {ExperimentName} started", experimentName);
    }
}
```

### Chaos Mesh — Network Fault Targeting .NET Service

**Tech Stack:** Chaos Mesh, Kubernetes CRD, YAML

```yaml
# NetworkChaos — inject 500ms latency to the .NET Order API
apiVersion: chaos-mesh.org/v1alpha1
kind: NetworkChaos
metadata:
  name: order-api-latency-test
  namespace: production
spec:
  action: delay
  mode: one                         # Target 1 random pod
  selector:
    namespaces:
      - production
    labelSelectors:
      app: "order-api"
  delay:
    latency: "500ms"
    correlation: "25"
    jitter: "50ms"
  duration: "10m"
  direction: to
```

### LitmusChaos — Pod Delete on .NET Deployment

```yaml
# ChaosEngine — pod-delete experiment on .NET Order API
apiVersion: litmuschaos.io/v1alpha1
kind: ChaosEngine
metadata:
  name: order-api-pod-delete
  namespace: production
spec:
  appinfo:
    appns: production
    applabel: "app=order-api"
    appkind: deployment
  engineState: active
  experiments:
    - name: pod-delete
      spec:
        components:
          env:
            - name: TOTAL_CHAOS_DURATION
              value: "30"           # 30 seconds total
            - name: CHAOS_INTERVAL
              value: "10"           # Kill pod every 10s
            - name: FORCE
              value: "false"        # Graceful SIGTERM
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What is Chaos Mesh? | A CNCF-graduated, Kubernetes-native chaos platform using Custom Resource Definitions to define and schedule fault injection experiments declaratively. |
| When would you choose Gremlin over Chaos Mesh? | Gremlin is preferred when you need enterprise support, pre-built scenario libraries, or chaos targeting bare-metal/VM environments outside Kubernetes. |
| What is LitmusChaos ChaosCenter? | A web UI and API server providing workflow orchestration for multi-step chaos scenarios, result dashboards, and experiment scheduling with a GitOps approach. |
| What is Azure Chaos Studio? | Microsoft's managed chaos engineering service targeting Azure ARM resources (AKS pods, VMs, Service Bus, Cosmos DB) with built-in Azure Monitor integration. |
| How do chaos tools enforce blast radius limits? | Via selectors (namespace, label, pod percentage), duration caps, and abort conditions that halt the experiment if SLO breach thresholds are crossed. |
| What is a GameDay? | A planned event where engineers intentionally run chaos experiments in staging or production to test runbooks, incident response, and system behavior together. |

---

## 6. Implementing Chaos Engineering in .NET & Azure

### Overview

In .NET and Azure environments, chaos engineering integrates at three layers: application-level resilience patterns (Polly v8), infrastructure-level experiments (Azure Chaos Studio / Chaos Mesh), and pipeline-level automation (Azure DevOps chaos gates). The goal is a continuous feedback loop where resilience gaps are discovered automatically in CI/CD before reaching production, and production experiments run on a schedule with automatic rollback on SLO breach.

### .NET Resilience Pipeline Architecture

```mermaid
flowchart TD
    subgraph AppLayer["Application Layer — Polly v8 Resilience"]
        REQ["Incoming Request"] --> PIPE["Polly ResiliencePipeline"]
        PIPE --> RET["Retry\n3 attempts, exponential backoff"]
        RET --> CB["Circuit Breaker\nOpen after 50% failure / 30s"]
        CB --> BH["Bulkhead\nMax 10 concurrent calls"]
        BH --> TM["Timeout\n5s per attempt"]
        TM --> SVC["Downstream Service"]
    end

    subgraph ChaosLayer["Chaos Injection Layer — Polly Simmy"]
        SVC -->|"SimulateLatency"| LAT["500ms added\nInjection rate: 30%"]
        SVC -->|"SimulateException"| EXC["HttpRequestException\nInjection rate: 10%"]
        SVC -->|"Normal path"| OK["200 OK"]
    end

    subgraph ObsLayer["Observability — Azure Application Insights"]
        LAT & EXC & OK --> AI["Azure Application Insights\nDependencies, exceptions, traces"]
        AI --> DASH["Live Metrics Dashboard\nSLO breach alerting"]
    end

    classDef app fill:#8b5cf6,color:#fff
    classDef chaos fill:#ef4444,color:#fff
    classDef obs fill:#0078D4,color:#fff
    classDef ok fill:#22c55e,color:#fff

    class REQ,PIPE,RET,CB,BH,TM app
    class LAT,EXC chaos
    class AI,DASH obs
    class OK ok
```

### Polly v8 Resilience Pipeline (.NET 8)

**Tech Stack:** Polly v8, Microsoft.Extensions.Http.Resilience, ASP.NET Core Minimal API

```csharp
// Program.cs — full resilience pipeline for chaos testing
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>()
    .AddResilienceHandler("order-service-pipeline", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.TooManyRequests } => PredicateResult.True(),
                { Result.StatusCode: >= HttpStatusCode.InternalServerError } => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(15),
            OnOpened = args =>
            {
                logger.LogWarning("Circuit breaker OPENED for order-service at {Time}", DateTimeOffset.UtcNow);
                return ValueTask.CompletedTask;
            }
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(5));
    });
```

### In-Process Chaos with Polly Simmy (.NET 8)

**Tech Stack:** Polly.Simmy (Chaos strategies), IConfiguration, Feature flags

```csharp
// Polly v8 built-in chaos strategies — inject faults in dev/staging
using Polly.Simmy;
using Polly.Simmy.Latency;
using Polly.Simmy.Fault;

public static class ChaosExtensions
{
    public static IHttpClientBuilder AddChaosForTesting(
        this IHttpClientBuilder builder,
        IConfiguration config)
    {
        if (!config.GetValue<bool>("Chaos:Enabled")) return builder;

        return builder.AddResilienceHandler("chaos-layer", pipeline =>
        {
            // Inject 500ms latency on 30% of requests
            pipeline.AddChaosLatency(new ChaosLatencyStrategyOptions
            {
                EnabledGenerator = _ => ValueTask.FromResult(true),
                InjectionRateGenerator = _ => ValueTask.FromResult(0.30),
                Latency = TimeSpan.FromMilliseconds(500)
            });

            // Inject HttpRequestException on 10% of requests
            pipeline.AddChaosFault(new ChaosFaultStrategyOptions
            {
                EnabledGenerator = _ => ValueTask.FromResult(true),
                InjectionRateGenerator = _ => ValueTask.FromResult(0.10),
                FaultGenerator = new FaultGenerator()
                    .AddException<HttpRequestException>("Simmy: simulated network failure")
            });
        });
    }
}
```

```json
// appsettings.Development.json
{
  "Chaos": {
    "Enabled": true
  }
}
```

### Steady-State Health Check as SLO Gate (.NET)

**Tech Stack:** ASP.NET Core Health Checks, AspNetCore.HealthChecks.UI

```csharp
// Expose SLO metrics as a health check endpoint for chaos tools to poll
builder.Services.AddHealthChecks()
    .AddCheck<LatencySloHealthCheck>("latency-slo", tags: ["chaos-slo"])
    .AddCheck<ErrorRateSloHealthCheck>("error-rate-slo", tags: ["chaos-slo"])
    .AddSqlServer(
        builder.Configuration.GetConnectionString("Default")!,
        tags: ["chaos-slo", "dependencies"]);

app.MapHealthChecks("/healthz/chaos-slo", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("chaos-slo"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

public class LatencySloHealthCheck : IHealthCheck
{
    private readonly IMetricsCollector _metrics;

    public LatencySloHealthCheck(IMetricsCollector metrics) => _metrics = metrics;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        var p99 = await _metrics.GetP99LatencyAsync(ct);

        return p99 < TimeSpan.FromMilliseconds(200)
            ? HealthCheckResult.Healthy($"p99={p99.TotalMilliseconds}ms")
            : HealthCheckResult.Unhealthy($"SLO BREACH: p99={p99.TotalMilliseconds}ms > 200ms threshold");
    }
}
```

### Interview Talking Points

| Question | Answer |
|---|---|
| How do you implement chaos engineering in .NET microservices? | Three layers: Polly for application-level resilience, Chaos Mesh or Azure Chaos Studio for infrastructure faults, and CI/CD chaos gates polling `/healthz/chaos-slo` to fail a deployment on SLO breach. |
| What is Polly Simmy? | Polly's built-in chaos engineering extension (ChaosStrategy in Polly v8) for injecting latency, exceptions, and custom outcomes into HTTP pipelines with configurable injection rates. |
| How do you integrate chaos experiments into Azure DevOps? | Add an Azure Chaos Studio pipeline task — inject fault, wait for experiment duration, poll `/healthz/chaos-slo`, and fail the release gate if Unhealthy is returned. |
| What observability is required before running chaos experiments? | Full stack: distributed tracing (OpenTelemetry), metrics (Prometheus or Azure Monitor), and structured logs (Application Insights). Without observability, chaos experiments are blind. |
| How do you protect real users during production experiments? | Limit blast radius to canary pods, run during low-traffic windows, set abort conditions (error > 1% → auto-stop), and use feature flags to target only internal/synthetic traffic first. |

---

## 7. Observability & Metrics During Chaos

### Overview

Observability is the prerequisite for chaos engineering — without it, you cannot define steady state, detect degradation, or verify recovery. The three pillars (metrics, logs, traces) must all be in place before running any experiment. In .NET and Azure, this means OpenTelemetry for traces and metrics, Prometheus or Azure Monitor as the backend, and Application Insights or ELK for structured logs. Chaos tools poll the observability stack as their abort condition.

### Observability Stack Architecture

```mermaid
flowchart TD
    subgraph NetApp[".NET Application"]
        API["ASP.NET Core\nMinimal API"]
        OT["OpenTelemetry SDK\nMetrics + Traces + Logs"]
        API --> OT
    end

    subgraph Backends["Observability Backends"]
        PROM["Prometheus\nMetrics scrape via /metrics"]
        AI["Azure Application Insights\nTraces + Logs + Metrics"]
        GRAF["Grafana\nDashboards + Alert rules"]
    end

    subgraph ChaosMonitor["Chaos Monitoring Gate"]
        SLO["SLO Dashboard\np99 Latency / Error Rate\nThroughput / Circuit State"]
        ABORT["Abort Controller\nAuto-halt on SLO breach"]
    end

    OT -->|"OTLP export"| PROM & AI
    PROM --> GRAF
    AI --> GRAF
    GRAF --> SLO
    SLO -->|"Breach detected"| ABORT
    ABORT -->|"Stop experiment signal"| API

    classDef app fill:#8b5cf6,color:#fff
    classDef backend fill:#1e40af,color:#fff
    classDef monitor fill:#f59e0b,color:#fff
    classDef abort fill:#ef4444,color:#fff

    class API,OT app
    class PROM,AI,GRAF backend
    class SLO monitor
    class ABORT abort
```

### OpenTelemetry Setup for Chaos Observability (.NET 8)

**Tech Stack:** OpenTelemetry .NET SDK, Prometheus exporter, OTLP exporter, Azure Monitor

```csharp
// Program.cs — full observability for chaos experiments
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("order-api", serviceVersion: "2.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()   // GC, thread pool, memory pressure
        .AddPrometheusExporter());     // /metrics for Prometheus scrape

// Custom SLO histogram — the source of truth for chaos abort conditions
builder.Services.AddSingleton<RequestLatencyTracker>();

public class RequestLatencyTracker
{
    private readonly Histogram<double> _latencyHistogram;
    private readonly Counter<long> _errorCounter;

    public RequestLatencyTracker(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("order-api");

        _latencyHistogram = meter.CreateHistogram<double>(
            "http.server.request.duration",
            unit: "ms",
            description: "Request duration for SLO tracking during chaos experiments");

        _errorCounter = meter.CreateCounter<long>(
            "http.server.error.count",
            description: "5xx error count for error-rate SLO tracking");
    }

    public void RecordRequest(double latencyMs, string endpoint, int statusCode)
    {
        _latencyHistogram.Record(latencyMs,
            new TagList
            {
                { "endpoint", endpoint },
                { "status_code", statusCode }
            });

        if (statusCode >= 500)
            _errorCounter.Add(1, new TagList { { "endpoint", endpoint } });
    }
}
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What observability must be in place before running chaos experiments? | Metrics (p99 latency, error rate, throughput), distributed traces (request flows through all services), and structured logs with correlation IDs. |
| What is a "steady state indicator"? | A specific, measurable metric representing normal system behavior — e.g., "p99 latency < 200ms measured over a 5-minute rolling window from Prometheus." |
| How do you auto-halt a chaos experiment on SLO breach? | Configure the chaos tool's abort condition to poll `/healthz/chaos-slo`. If it returns Unhealthy, the tool automatically stops the experiment. |
| What runtime metrics are critical during chaos experiments? | Thread pool queue depth, GC Gen2 collection frequency, HTTP client connection pool exhaustion count, circuit breaker state transitions, and Polly retry attempt counts. |
| Why is distributed tracing critical for chaos experiments? | It lets you trace exactly which service call failed and which retry or circuit breaker fired — without it, you see only aggregate degradation, not the root cause path. |

---

## Cross-Cutting Themes

### Pattern Selection Guide

```mermaid
flowchart TD
    FAIL(["System Degradation\nDetected"]) --> Q1{What type of failure?}

    Q1 -->|"Transient network error\nor 5xx response"| RET["Polly Retry\nExponential backoff + jitter\nMax 3 attempts"]
    Q1 -->|"Repeated failures from\none downstream service"| CB["Polly Circuit Breaker\nOpen after 50% failure rate\nHalf-open probe after 15s"]
    Q1 -->|"Slow dependency causing\nthread starvation"| BH["Polly Bulkhead\nMax 10 concurrent calls\nQueue 20, reject rest"]
    Q1 -->|"Dependency always fails\nbut non-critical path"| FF["Fallback Strategy\nReturn cached result\nor safe default value"]
    Q1 -->|"Complex resilience needed\nfor critical dependency"| PIPE["Full Resilience Pipeline\nRetry → CB → Bulkhead\n→ Timeout → Fallback"]

    RET -->|"Still failing after\n3 retries"| CB
    CB -->|"Circuit open > 30s\nno recovery"| ALERT["Alert Ops Team\nPage on-call engineer"]
    PIPE --> KAFKA["Kafka Buffer\nPersist failed events\nReplay on recovery"]

    classDef pattern fill:#8b5cf6,color:#fff
    classDef action fill:#22c55e,color:#fff
    classDef escalate fill:#ef4444,color:#fff

    class RET,CB,BH,FF,PIPE pattern
    class KAFKA,ALERT action
    style ALERT fill:#ef4444,color:#fff
```

### Common Interview Red Flags to Avoid

| Red Flag | Why It's Wrong | Correct Answer |
|---|---|---|
| "We retry in a tight loop immediately" | Thundering herd — hammers a recovering service with all queued requests simultaneously | Exponential backoff with jitter: `Delay * (2^attempt) + random(0, 100ms)` via Polly |
| "We run chaos experiments only in staging" | Staging behaves differently — real load, CDN, DNS, and connection pool exhaustion only appear in production | Run in production at canary blast radius after staging validation passes |
| "Chaos engineering is just random failure injection" | Random failures without hypotheses provide no signal — you won't know what to fix or verify | Follow the scientific method: steady state → hypothesis → inject → verify |
| "We will add observability after chaos experiments" | Without metrics and traces, you cannot detect degradation or verify recovery | Full observability is a prerequisite, not an afterthought — chaos experiments are blind without it |
| "Our tests pass so we don't need chaos engineering" | Unit/integration tests cannot replicate network partitions, hardware failures, or cascading dependency timeouts | Tests verify correctness; chaos engineering verifies resilience under real failure conditions |
| "We run chaos experiments for 30 seconds" | Too short to collect statistically significant data or observe cascading effects like circuit breaker state transitions | Minimum 5–10 minutes; long enough for retry storms and circuit breakers to develop |
| "We start the experiment at full system scope" | Risk of widespread user impact before any confidence is established | Always: 1 pod → 1 AZ → 1 region → global. Expand only when each level passes |
| "Kubernetes env vars are safe for secrets" | Visible in pod spec, etcd, and `kubectl describe` — exposed to any cluster reader | Azure Key Vault with Managed Identity + CSI Secrets Store Driver; `DefaultAzureCredential` in .NET |

---

*Generated by ConceptToMD Agent v1.0 | Source: chaos-engg.txt | 2026-07-05*
