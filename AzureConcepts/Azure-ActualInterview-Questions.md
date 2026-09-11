# Azure Actual Interview Questions

---

## 1. How to do Dependency Injection on Azure Function App?

Azure Functions supports .NET's built-in DI via `Microsoft.Azure.Functions.Extensions`.

**Setup:**

1. Add NuGet: `Microsoft.Azure.Functions.Extensions` and `Microsoft.Extensions.DependencyInjection`
2. Create a `Startup` class that extends `FunctionsStartup`:

```csharp
[assembly: FunctionsStartup(typeof(MyApp.Startup))]

namespace MyApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register services
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IMyService, MyService>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            // Register options/config
            builder.Services.AddOptions<MyConfig>()
                .Configure<IConfiguration>((settings, config) =>
                {
                    config.GetSection("MyConfig").Bind(settings);
                });
        }
    }
}
```

3. Inject into the Function class constructor (NOT static — must be instance class):

```csharp
public class OrderFunction
{
    private readonly IMyService _myService;
    private readonly ILogger<OrderFunction> _logger;

    public OrderFunction(IMyService myService, ILogger<OrderFunction> logger)
    {
        _myService = myService;
        _logger = logger;
    }

    [FunctionName("ProcessOrder")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var result = await _myService.ProcessAsync();
        return new OkObjectResult(result);
    }
}
```

**Key points:**
- Function class must be **non-static** to use constructor injection
- Supports Singleton, Scoped, Transient lifetimes
- `ILogger<T>` is auto-registered by the host
- For .NET Isolated (v4+), DI is configured in `Program.cs` using `HostBuilder` — same pattern as ASP.NET Core

```csharp
// .NET Isolated (v4) style
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IMyService, MyService>();
    })
    .Build();

await host.RunAsync();
```

---

## 2. Logic App Types of Design Patterns

Logic Apps supports several integration design patterns:

### a) Request-Response (Synchronous)
- Triggered by HTTP request, returns response within the same call
- Use when caller needs an immediate result
- Timeout limit: 2 minutes (use async pattern for longer operations)

### b) Polling Pattern
- Logic App polls an endpoint/queue at scheduled intervals
- Built-in via Recurrence trigger or connector polling triggers (e.g., "When a message is available in Service Bus")

### c) Event-Driven / Push Pattern
- Triggered by events from Event Grid, Service Bus, Event Hubs, or webhooks
- Low latency; ideal for real-time processing

### d) Saga / Orchestration Pattern
- Coordinates multi-step transactions across services
- Each step has a compensating action for rollback on failure
- Logic Apps acts as the orchestrator; individual services are participants

### e) Scatter-Gather (Fan-Out / Fan-In)
- Splits a message into parallel branches (fan-out), collects results (fan-in)
- Implemented using parallel branches + Join action or SplitOn + aggregation

### f) Publish-Subscribe (Pub/Sub)
- Logic App subscribes to topics (Service Bus, Event Grid)
- Multiple Logic Apps can subscribe independently to the same event

### g) Content-Based Routing
- Routes messages to different backends based on message content
- Implemented with Switch/Condition actions or Service Bus topic filters

### h) Aggregator Pattern
- Collects multiple related messages and combines into a single output
- Implemented using stateful workflows + variables or storage

### i) Sequential Convoy (FIFO per Session)
- Processes ordered messages belonging to a session (e.g., all orders for a customer)
- Uses Service Bus message sessions + Logic App session-aware trigger

### j) Async Request-Reply
- HTTP trigger returns `202 Accepted` with a polling URL immediately
- Caller polls the URL until work completes
- Implemented via Logic App's built-in async HTTP pattern or with Durable Functions

### k) Adapter Pattern
- Logic App acts as a **translator** between two systems that speak different protocols or data formats
- The consumer calls Logic App with its native format/protocol; Logic App transforms and calls the target system in its format
- Example: REST client → Logic App (converts JSON to SOAP/XML) → SAP or legacy SOAP service
- Adapter hides the incompatibility — neither side needs to change
- Common use: bridging REST ↔ SOAP, JSON ↔ EDI, HTTP ↔ FTP, modern app ↔ on-premises system via data gateway

### l) Facade Pattern
- Logic App exposes a **simplified, unified interface** over multiple complex or granular backend services
- The caller makes one call to Logic App; internally it orchestrates multiple service calls and returns a composed response
- Example: single `/customer-summary` endpoint → Logic App calls CRM (Salesforce), ERP (SAP), and billing system in parallel, merges results
- Facade hides complexity, versioning, and backend topology from consumers
- Difference from Adapter: Adapter translates *one* system; Facade *aggregates and simplifies* multiple systems behind one interface
- Often paired with APIM: APIM is the gateway, Logic App is the facade orchestration layer behind it

---

## 3. Service Bus Filters

Filters on Service Bus **Topic Subscriptions** control which messages a subscription receives.

### Types of Filters:

#### a) SQL Filter (most common)
- Evaluates a SQL-like expression against message **user-defined properties** and **system properties**
- Syntax similar to SQL WHERE clause

```sql
-- Only receive messages where Region = 'US' and Priority > 2
Region = 'US' AND Priority > 2

-- System properties also supported
sys.Label = 'order' AND sys.To = 'billing'
```

#### b) Correlation Filter (most performant)
- Matches one or more well-known message properties by equality
- Evaluated server-side without SQL parsing — much faster than SQL filters
- Use when filtering on: `CorrelationId`, `MessageId`, `To`, `ReplyTo`, `Subject/Label`, `SessionId`, or custom properties

```csharp
var filter = new CorrelationRuleFilter
{
    Subject = "order",
    CorrelationId = "region-us",
    ApplicationProperties = { ["Priority"] = "High" }
};
```

#### c) True Filter
- Default filter on every new subscription — matches ALL messages
- Every message goes to this subscription

#### d) False Filter
- Matches NO messages — effectively disables a subscription without deleting it

### Filter Actions
Filters can optionally include an **action** to modify message properties before delivery:

```sql
-- Filter: Category = 'Electronics'
-- Action: SET DiscountRate = 0.15
SET user.DiscountRate = 0.15
```

### Key Points:
- A subscription can have **multiple filter rules** (evaluated with OR logic — if any rule matches, message is delivered)
- Default subscription has one True filter rule named `$Default`
- SQL filters support `IN`, `LIKE`, `IS NULL`, arithmetic, and boolean operators
- Correlation filters are preferred for high-throughput scenarios due to lower overhead
- Filters are evaluated against **message properties**, not the message body

---

## 4. Use of Products in APIM

**Products** in Azure API Management are a way to **bundle APIs and apply a shared access policy** to a group of consumers.

### What a Product Is:
- A logical container that groups one or more APIs
- Consumers (developers) subscribe to a **Product**, not directly to an API
- Subscription generates a **subscription key** used to call any API in that product

### Why Use Products:

#### a) Tiered Access / Monetization
- Create different products for different consumer tiers:
  - **Free Tier**: 100 calls/day, rate-limited
  - **Standard Tier**: 10,000 calls/day
  - **Premium Tier**: Unlimited, SLA-backed
- Apply different `rate-limit` and `quota` policies per product

#### b) Audience Segmentation
- **Internal Product**: Only internal developers, no approval needed
- **Partner Product**: Requires admin approval before access granted
- **Public Product**: Open subscription, broad API exposure

#### c) Policy Application
- Policies set at the **Product scope** apply to all APIs in that product
- Example: apply `validate-jwt` or `set-header` for all APIs under a product without repeating it per API

#### d) Subscription Key Isolation
- Each subscriber gets a unique key tied to the product
- Revoke one subscriber without affecting others
- Track usage per subscriber via Azure Monitor / Application Insights

### Product States:
- **Published**: Visible in Developer Portal, subscriptions allowed
- **Not Published**: Hidden from portal; existing subscriptions still work

### Approval Workflow:
- Products can require **admin approval** before a subscription key is issued
- Useful for partner/enterprise onboarding with controlled access

### Example Scenario:
```
APIM Instance
├── Product: "Internal" (no approval, unlimited quota)
│   ├── API: Customer API
│   └── API: Order API
├── Product: "Partner" (approval required, 5000 calls/day quota)
│   └── API: Order API (read-only subset)
└── Product: "Public" (no approval, 100 calls/day quota)
    └── API: Product Catalog API
```

**Key distinction:** APIs define *what* is exposed; Products define *who* can access it and *under what terms*.
