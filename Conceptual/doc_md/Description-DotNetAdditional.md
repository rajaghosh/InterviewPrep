# .NET Additional Topics

---

## .NET Questions Checklist

1. How to call an API from another API? — HttpClient
2. Difference between .NET Core and .NET Framework
3. Difference between IIS and Kestrel
4. How Kestrel is platform independent
5. How to read configs from `appsettings.json`
6. How to cancel any ongoing task — using CancellationToken
7. Cache mechanism in .NET Core
8. Design Patterns: Factory, Abstract Factory, CQRS, Repository, SAGA, Singleton
9. Model validation
10. API rate limiting on individual APIs
11. Migration from .NET Core 3.1 to 6
12. State Management
13. Security: SSL, SAML, OpenId, CORS
14. Docker concepts
15. MVC related questions
16. SOA — Service Oriented Architecture
17. Yield keyword
18. `Task.WhenAll` vs `Task.WaitAll`
19. 4 pillars of OOP
20. EF Core — Unit of Work, Navigation
21. `Func<>` vs `Action<>`
22. Types of APIs
23. Hosted Service in .NET Core
24. OAuth
25. Covariance vs Contravariance
26. JWT structure and parts

---

## CORS — Cross-Origin Resource Sharing

CORS works at the **browser** level to control access between different origins.

---

## Cross-Site Request Forgery (CSRF)

An attack that forces an authenticated user to execute unwanted actions on a web application.

- Attacker tricks users via social engineering (links via email/chat).
- Can force state-changing actions: fund transfers, email changes.
- If victim is an admin, can compromise the entire application.

---

## Private Class

In C#, a private class is only possible as a **nested class**.

```csharp
public class A {
  class B { // private nested class
  }
}
```

---

## Static Constructor

```csharp
class Test {
  private static int id;

  static Test() {
    if (OtherClass.Id < 10) id = 20;
    else id = 100;
  }
}
```

- Called at most once — before first instance creation or any static member reference.
- No access modifiers or parameters.
- Executed after static field initializers.

---

## Short-Circuit Middleware

When a middleware **short-circuits**, it stops further middleware from processing the request. Also called **terminal middleware**.

---

## Advantages of SPA (Single Page Application)

- **Faster** after initial load — no full page reloads.
- Better user experience — smooth navigation.

---

## State Management in ASP.NET Core

HTTP is stateless — techniques for persisting data:

| Mechanism | Scope |
|-----------|-------|
| `Hidden Field` | Form submission |
| `Cookies` | Browser-level persistence |
| `Query String` | URL parameters |
| `ViewData` | Controller → View (Dictionary) |
| `ViewBag` | Controller → View (Dynamic) |
| `TempData` | Survives a single redirect |

---

## .NET Core MVC Request Lifecycle

```
HTTP Request
  → Middleware
  → Controller
  → Action Method Execution
  → Result Execution
  → Data Result / View Rendering
  → Response
```

Filters can be applied at the Action Method / Controller level.

### Action Result Types

| Type | Method | Description |
|------|--------|-------------|
| `ViewResult` | `View()` | Renders an HTML view |
| `PartialViewResult` | `PartialView()` | Returns a partial view |
| `ContentResult` | `Content()` | Returns plain text |
| `RedirectResult` | `Redirect()` | Redirect to URL |
| `RedirectToRouteResult` | `RedirectToAction()` | Redirect to action |
| `JsonResult` | `Json()` | Returns serialized JSON |
| `FileResult` | `File()` | Returns a file |
| `HttpNotFoundResult` | `HttpNotFound()` | Returns 404 |
| `EmptyResult` | — | No return value |

> **Note:** Action method overloading is only allowed if methods are associated with **different HTTP verbs**.

---

## Routing in MVC .NET Core

### A. Conventional Routing

```csharp
app.UseRouting();
app.UseEndpoints(endpoints => {
  endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
});
```

### B. Attribute Routing

```csharp
app.UseEndpoints(endpoints => {
  endpoints.MapControllers();
});

// In controller
[Route("api/products")]
[HttpGet("{id}")]
public IActionResult Get(int id) { }
```

---

## Return Types for .NET Core Web API

### 1. Specific Type
Returns a primitive or complex object. Cannot return status codes like `Ok()` or `NotFound()`.

### 2. `IActionResult`
Allows multiple return types with built-in methods.

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id) {
  if (!_repository.TryGetEmployee(id, out var employee))
    return NotFound();
  return Ok(employee);
}
```

### 3. `ActionResult<T>`
Combines specific type and ActionResult — can return both the object directly or status codes.

```csharp
[HttpGet("{id}")]
public ActionResult<Employee> GetById(int id) {
  if (!_repository.TryGetEmployee(id, out var employee))
    return NotFound();
  return employee;
}
```

---

## Model Binding in .NET Core

Maps client request data to action method parameters automatically.

### Binding Sources

| Attribute | Source |
|-----------|--------|
| `[FromBody]` | Request body (JSON/XML) |
| `[FromForm]` | HTML form fields |
| `[FromHeader]` | HTTP headers |
| `[FromRoute]` | URL route data |
| `[FromQuery]` | Query string parameters |

```csharp
[HttpPost]
public IActionResult Create([FromBody] Employee employee) { }

public IActionResult Search([FromQuery] string query) { }
```

**Benefits:**
- Type safety — converts client data to .NET types.
- Automatic validation with `ModelState.IsValid`.
- Eliminates repetitive manual parsing code.

---

## Kubernetes (K8s)

Kubernetes is an open-source **container orchestration platform** that automates deployment, scaling, and management of containerized applications.

> **Why K8s?** KUBERNETES → K + UBERNETES (8 chars) = K8S

### Architecture

```
CLUSTER
├── CONTROL PLANE
│   ├── API Server      — Primary interface; exposes RESTful API
│   ├── etcd            — Distributed key-value store for cluster state
│   ├── Scheduler       — Places pods on worker nodes
│   └── Controller Mgr  — Manages cluster state (Replication, Deployment controllers)
│
└── WORKER NODES
    ├── Kubelet         — Communicates with control plane; runs pods
    ├── Container Runtime — Pulls images, starts/stops containers
    └── Kube Proxy      — Routes traffic to pods; load balancing
```

### Key Concepts
- **Cluster** — Set of machines (nodes) running containerized apps.
- **Node** — Individual machine (worker).
- **Pod** — Smallest deployable unit; hosts one or more containers.

### Pros
- Scalable and highly available.
- Self-healing, automatic rollbacks, horizontal scaling.
- Portable — runs on-premises, public cloud, or hybrid.

### Cons
- Highly complex to maintain and deploy.
- High upfront setup cost.

### Managed K8s Services
Cloud providers (Azure AKS, AWS EKS, GCP GKE) manage the control plane, reducing operational overhead.

---

## SOLID Principles

| Principle | Letter | Short Definition |
|-----------|--------|-----------------|
| Single Responsibility | S | A class should have only one reason to change |
| Open/Closed | O | Open for extension, closed for modification |
| Liskov Substitution | L | Subtypes must be substitutable for their base types |
| Interface Segregation | I | Classes should not be forced to implement interfaces they don't use |
| Dependency Inversion | D | High-level modules should depend on abstractions, not concretions |

### S — Single Responsibility
Each interface and implementation serves one entity.  
`IEmployee` and `IDepartment` are separate — not mixed into one interface.

### O — Open/Closed
Extend behavior by inheritance or new implementations rather than modifying existing code.  
Add an overloaded method in a child class instead of changing the base class.

### L — Liskov Substitution
Implementations can be substituted without changing the calling code.

```csharp
// Swap implementation via DI registration
// Before:
services.AddTransient<IDBContext, SqlServerDBContext>();
// After:
services.AddTransient<IDBContext, MongodbDBContext>();
```

### I — Interface Segregation
Break large interfaces into smaller, focused ones.

```csharp
public class PWC : ICorporate, IPWC { }   // only implements what it needs
public class Deloitte : ICorporate { }
```

### D — Dependency Inversion
High-level modules → depend on **interfaces**, not concrete implementations.

**Three types of Dependency Injection:**
1. **Constructor Injection** (most common)
2. **Property Injection**
3. **Method Injection**

---

## Response Caching

Stores request-response results to improve performance.

```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
public IActionResult Index() => View();
```

### Cache-Control Directives

| Directive | Meaning |
|-----------|---------|
| `public` | Any cache can store the response |
| `private` | Single user only — not for shared caches |
| `no-cache` | Must validate with origin server before using cache |
| `no-store` | Do not cache any part of request/response |

### Benefits
- Reduced server load.
- Improved response times.
- Better scalability.

---

## HTTP Status Codes Reference

| Code | Meaning |
|------|---------|
| `102` | Processing |
| `200` | OK |
| `201` | Created |
| `202` | Accepted |
| `400` | Bad Request |
| `401` | Unauthorized |
| `403` | Forbidden |
| `404` | Not Found |
| `429` | Too Many Requests (rate limiting) |
| `500` | Internal Server Error |
| `501` | Not Implemented |
| `502` | Bad Gateway |
| `503` | Service Unavailable |
| `504` | Gateway Timeout |

---

## .NET Core vs .NET Framework

| Feature | .NET Core | .NET Framework |
|---------|-----------|----------------|
| Platform | Cross-platform (Windows, Linux, macOS) | Windows only |
| Open Source | Yes | Partial |
| Deployment | Self-contained executable | Requires installed runtime |
| Performance | Faster, optimized HTTP pipeline | Slower |
| CLI | `dotnet` CLI | MSBuild only |
| Side-by-side versioning | Yes | Limited |
| Modularity | NuGet packages | Monolithic |
| ASP.NET | ASP.NET Core | ASP.NET (WebForms, MVC 5) |
| Current status | Active (renamed ".NET 5+") | Legacy (LTS: .NET Framework 4.8) |

> Both merged under the name **.NET** starting with .NET 5.

---

## IIS vs Kestrel

| Feature | IIS | Kestrel |
|---------|-----|---------|
| Type | Full-featured web server | Lightweight HTTP server |
| Platform | Windows only | Cross-platform |
| Bundled with ASP.NET Core | No | Yes (default) |
| Direct internet exposure | Recommended | Behind reverse proxy |
| HTTPS termination | Yes | Yes (with certificate) |
| Process management | Windows service | Manual / systemd |

### How Kestrel is Platform Independent

Kestrel uses `System.Net.Sockets` (and historically `libuv`) — both are cross-platform async I/O libraries with no Windows-specific dependencies.

```
Client
  ↓
[Nginx / IIS reverse proxy]   ← optional, recommended for production
  ↓
Kestrel (ASP.NET Core app)
```

- **Windows**: run Kestrel behind IIS using `AspNetCoreModule`.
- **Linux/macOS**: run Kestrel behind Nginx or Apache.
- **Development**: run Kestrel directly (`dotnet run`).

---

## Reading Config from appsettings.json

```json
{
  "AppSettings": {
    "ApiKey": "my-secret-key",
    "MaxItems": 100
  },
  "ConnectionStrings": {
    "Default": "Server=.;Database=MyDb;"
  }
}
```

### Method 1 — IConfiguration (direct key access)

```csharp
public class MyService {
  private readonly IConfiguration _config;
  public MyService(IConfiguration config) => _config = config;

  public void Run() {
    var key = _config["AppSettings:ApiKey"];
    var connStr = _config.GetConnectionString("Default");
  }
}
```

### Method 2 — Options Pattern (strongly typed)

```csharp
public class AppSettings {
  public string ApiKey { get; set; }
  public int MaxItems { get; set; }
}

// Program.cs
builder.Services.Configure<AppSettings>(
  builder.Configuration.GetSection("AppSettings"));

// Service
public class MyService {
  private readonly AppSettings _settings;
  public MyService(IOptions<AppSettings> opts) => _settings = opts.Value;
}
```

---

## CancellationToken

Used to **cooperatively cancel** long-running or async operations.

```csharp
// Controller — ASP.NET Core injects HttpContext.RequestAborted automatically
[HttpGet("data")]
public async Task<IActionResult> GetData(CancellationToken ct) {
  var result = await _service.FetchDataAsync(ct);
  return Ok(result);
}

// Service
public async Task<Data> FetchDataAsync(CancellationToken ct) {
  ct.ThrowIfCancellationRequested();     // check before heavy work
  await Task.Delay(5000, ct);            // cancellable delay
  return new Data();
}

// Manual control
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await FetchDataAsync(cts.Token);
cts.Cancel(); // cancel early
```

- `ThrowIfCancellationRequested()` — throws `OperationCanceledException` if token is signaled.
- `CancellationTokenSource` — creates and signals the token.
- ASP.NET Core passes `HttpContext.RequestAborted` when the client disconnects.

---

## Cache Mechanism in .NET Core

### 1. IMemoryCache (in-process, single server)

```csharp
builder.Services.AddMemoryCache();

public class ProductService {
  private readonly IMemoryCache _cache;
  public ProductService(IMemoryCache cache) => _cache = cache;

  public Product GetProduct(int id) {
    return _cache.GetOrCreate($"product_{id}", entry => {
      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
      entry.SlidingExpiration = TimeSpan.FromMinutes(2);
      return _db.Products.Find(id);
    });
  }
}
```

### 2. IDistributedCache (multi-server, Redis / SQL Server)

```csharp
// Redis
builder.Services.AddStackExchangeRedisCache(opts => {
  opts.Configuration = "localhost:6379";
});

// Read / Write
public async Task<string> GetAsync(string key, CancellationToken ct) {
  var bytes = await _cache.GetAsync(key, ct);
  return bytes != null ? Encoding.UTF8.GetString(bytes) : null;
}

public async Task SetAsync(string key, string value, CancellationToken ct) {
  var bytes = Encoding.UTF8.GetBytes(value);
  await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions {
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
  }, ct);
}
```

| Feature | IMemoryCache | IDistributedCache |
|---------|-------------|-------------------|
| Storage | In-process RAM | Redis / SQL Server |
| Multi-server | No | Yes |
| Serialization | Not needed | Required (byte[]) |
| Use when | Single server | Load-balanced / multi-instance |

---

## Design Patterns

### Singleton

One instance per application lifetime.

```csharp
// Thread-safe double-check singleton
public sealed class Logger {
  private static Logger _instance;
  private static readonly object _lock = new();
  private Logger() { }

  public static Logger Instance {
    get {
      if (_instance == null) {
        lock (_lock) {
          if (_instance == null) _instance = new Logger();
        }
      }
      return _instance;
    }
  }
}

// In .NET Core DI (preferred)
services.AddSingleton<IMyService, MyService>();
```

### Factory Pattern

Delegates object creation to a factory, hiding instantiation logic from the caller.

```csharp
public interface IAnimal { string Speak(); }
public class Dog : IAnimal { public string Speak() => "Woof"; }
public class Cat : IAnimal { public string Speak() => "Meow"; }

public static class AnimalFactory {
  public static IAnimal Create(string type) => type switch {
    "dog" => new Dog(),
    "cat" => new Cat(),
    _ => throw new ArgumentException("Unknown type")
  };
}

// Usage
IAnimal a = AnimalFactory.Create("dog");
```

### Abstract Factory Pattern

Factory of factories — creates families of related objects without specifying concrete classes.

```csharp
public interface IButton { void Render(); }
public interface ICheckbox { void Render(); }

public interface IUIFactory {
  IButton CreateButton();
  ICheckbox CreateCheckbox();
}

public class WindowsFactory : IUIFactory {
  public IButton CreateButton() => new WindowsButton();
  public ICheckbox CreateCheckbox() => new WindowsCheckbox();
}
public class MacFactory : IUIFactory {
  public IButton CreateButton() => new MacButton();
  public ICheckbox CreateCheckbox() => new MacCheckbox();
}

// Consumer doesn't care which factory it gets
public class App {
  private readonly IUIFactory _factory;
  public App(IUIFactory factory) => _factory = factory;
  public void BuildUI() { _factory.CreateButton().Render(); }
}
```

### CQRS — Command Query Responsibility Segregation

Separates read (**Query**) and write (**Command**) operations into distinct models.

```
Write path:  Client → Command → CommandHandler → Write DB
Read path:   Client → Query  → QueryHandler  → Read DB (optimized view)
```

```csharp
// Command (write)
public record CreateOrderCommand(int CustomerId, List<int> ProductIds);

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int> {
  public async Task<int> Handle(CreateOrderCommand cmd, CancellationToken ct) {
    var order = new Order(cmd.CustomerId, cmd.ProductIds);
    await _db.Orders.AddAsync(order, ct);
    await _db.SaveChangesAsync(ct);
    return order.Id;
  }
}

// Query (read)
public record GetOrderQuery(int OrderId);

public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto> {
  public async Task<OrderDto> Handle(GetOrderQuery q, CancellationToken ct) {
    return await _readDb.Orders
      .Where(o => o.Id == q.OrderId)
      .Select(o => new OrderDto(o.Id, o.Status))
      .FirstOrDefaultAsync(ct);
  }
}
```

**Benefits:** Independent scaling of reads and writes, optimized read models.

---

## Model Validation in .NET Core

### Data Annotations

```csharp
public class CreateUserDto {
  [Required]
  [StringLength(50, MinimumLength = 2)]
  public string Name { get; set; }

  [Required]
  [EmailAddress]
  public string Email { get; set; }

  [Range(18, 100)]
  public int Age { get; set; }
}
```

### Check in Controller

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto) {
  if (!ModelState.IsValid)
    return BadRequest(ModelState);
  return Ok();
}
```

### Auto-Validation with `[ApiController]`

When `[ApiController]` is applied, `ModelState.IsValid` is checked **automatically** — no manual check needed.

### Custom Validation Attribute

```csharp
public class FutureDateAttribute : ValidationAttribute {
  protected override ValidationResult IsValid(object value, ValidationContext ctx) {
    if (value is DateTime date && date > DateTime.Now)
      return ValidationResult.Success;
    return new ValidationResult("Date must be in the future.");
  }
}

// Usage
[FutureDate]
public DateTime EventDate { get; set; }
```

---

## API Rate Limiting in .NET Core

Available natively from **.NET 7** via `Microsoft.AspNetCore.RateLimiting`.

### Setup (Program.cs)

```csharp
builder.Services.AddRateLimiter(options => {
  options.AddFixedWindowLimiter("fixed", opt => {
    opt.Window = TimeSpan.FromMinutes(1);
    opt.PermitLimit = 100;
    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    opt.QueueLimit = 10;
  });
  options.RejectionStatusCode = 429;
});

app.UseRateLimiter();
```

### Apply to Endpoints

```csharp
[EnableRateLimiting("fixed")]
[ApiController]
public class ProductsController : ControllerBase { }

[DisableRateLimiting]
public IActionResult HealthCheck() => Ok("healthy");
```

### Rate Limiting Algorithms

| Algorithm | Description |
|-----------|-------------|
| Fixed Window | N requests per fixed time window |
| Sliding Window | Rolling window — more accurate than fixed |
| Token Bucket | Tokens replenish at fixed rate; allows bursts |
| Concurrency | Limits simultaneous in-flight requests |

---

## Migration from .NET Core 3.1 to .NET 6

### Step-by-Step

**1. Update `.csproj` target framework:**
```xml
<!-- Before --> <TargetFramework>netcoreapp3.1</TargetFramework>
<!-- After  --> <TargetFramework>net6.0</TargetFramework>
```

**2. Update all NuGet packages** to .NET 6 compatible versions.

**3. Migrate `Startup.cs` to `Program.cs` minimal hosting model:**
```csharp
// Old (3.1)
public class Startup {
  public void ConfigureServices(IServiceCollection services) { ... }
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { ... }
}

// New (.NET 6)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

**4. Replace `IHostBuilder`** with `WebApplication.CreateBuilder`.

**5. Enable nullable reference types** — on by default in .NET 6.

**6. Add `global using`** to reduce repetitive using statements:
```csharp
global using System;
global using Microsoft.AspNetCore.Mvc;
```

### Common Breaking Changes
- Default serializer changed from `Newtonsoft.Json` → `System.Text.Json`.
- `ILogger` constructor injection changes.
- Some middleware ordering requirements changed.

---

## SOA — Service Oriented Architecture

SOA structures software as a set of **interoperable services** communicating over a network, typically via SOAP/HTTP.

```
Client
  ↓
Enterprise Service Bus (ESB)
  ↓           ↓            ↓
Service A   Service B   Service C
(Orders)   (Payments)  (Inventory)
  ↓           ↓            ↓
     Shared / Separate Databases
```

| Feature | SOA | Microservices |
|---------|-----|---------------|
| Service size | Large, coarse-grained | Small, fine-grained |
| Communication | ESB / SOAP | REST / gRPC / events |
| Data sharing | Shared DB common | Each service owns its data |
| Deployment | Often together | Independently deployable |
| Technology | Often homogeneous | Polyglot |

**Key SOA Concepts:**
- **Service Contract** — WSDL defines what the service exposes.
- **Loose Coupling** — Services are independent; changes don't cascade.
- **Abstraction** — Internal implementation hidden from consumers.
- **Reusability** — Services designed for reuse by multiple consumers.
- **Discoverability** — Services registered in a service registry (UDDI).

---

## Func\<\> vs Action\<\>

Both are built-in generic delegate types for passing methods as arguments.

| | `Action<>` | `Func<>` |
|--|-----------|---------|
| Return type | `void` | Non-void (last type parameter) |
| Max parameters | 16 | 16 |
| Use when | Side effects, no return | Transformation / computation |

```csharp
// Action — no return value
Action<string> greet = name => Console.WriteLine($"Hello {name}");
Action<int, int> logSum = (a, b) => Console.WriteLine(a + b);
greet("Alice");   // Hello Alice

// Func — last type param is the return type
Func<int, int, int> multiply = (a, b) => a * b;
Func<string, bool> isLong = s => s.Length > 10;
int result = multiply(3, 4);  // 12

// LINQ usage
var nums = new[] { 1, 2, 3, 4, 5 };
Func<int, bool> isEven = n => n % 2 == 0;
var evens = nums.Where(isEven);  // [2, 4]
```

`Predicate<T>` is shorthand for `Func<T, bool>`:
```csharp
Predicate<int> isPositive = n => n > 0;
```

---

## Types of APIs

### REST (Representational State Transfer)
- Stateless; uses HTTP verbs (GET, POST, PUT, DELETE).
- Resources identified by URLs; returns JSON or XML.
- Most widely used pattern for public APIs.

### GraphQL
- Query language; clients specify **exactly** the fields they need.
- Single endpoint (`/graphql`); eliminates over/under-fetching.

```graphql
query {
  user(id: "1") {
    name
    email
    orders { id total }
  }
}
```

### gRPC (Google Remote Procedure Call)
- Uses **Protocol Buffers** (binary) — significantly faster than JSON.
- Strongly typed contracts via `.proto` files.
- Supports streaming (unary, server, client, bidirectional).
- Ideal for microservice-to-microservice communication.

```proto
service OrderService {
  rpc GetOrder (OrderRequest) returns (OrderResponse);
}
```

### SOAP (Simple Object Access Protocol)
- XML-based; strongly typed via WSDL.
- Enterprise features: WS-Security, distributed transactions.
- Verbose and slower compared to REST.

| Feature | REST | GraphQL | gRPC | SOAP |
|---------|------|---------|------|------|
| Protocol | HTTP | HTTP | HTTP/2 | HTTP/SMTP |
| Format | JSON | JSON | Binary (protobuf) | XML |
| Typing | Loose | Strong (schema) | Strong (.proto) | Strong (WSDL) |
| Performance | Good | Good | Excellent | Slow |
| Best for | Public APIs | Flexible queries | Microservices | Enterprise |

---

## Hosted Service in .NET Core

`IHostedService` runs background tasks alongside the web application.

### Simple IHostedService

```csharp
public class CleanupService : IHostedService, IDisposable {
  private Timer _timer;

  public Task StartAsync(CancellationToken ct) {
    _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    return Task.CompletedTask;
  }

  private void DoWork(object state) { /* delete old records */ }

  public Task StopAsync(CancellationToken ct) {
    _timer?.Change(Timeout.Infinite, 0);
    return Task.CompletedTask;
  }

  public void Dispose() => _timer?.Dispose();
}
```

### BackgroundService (preferred base class)

```csharp
public class EmailQueueProcessor : BackgroundService {
  protected override async Task ExecuteAsync(CancellationToken ct) {
    while (!ct.IsCancellationRequested) {
      await ProcessEmailQueueAsync();
      await Task.Delay(TimeSpan.FromSeconds(30), ct);
    }
  }
}
```

```csharp
// Register in Program.cs
builder.Services.AddHostedService<EmailQueueProcessor>();
```

**Common use cases:** periodic cleanup, message queue consumers, cache warmup, health monitoring.

---

## Covariance vs Contravariance

Describes how substitutability works for generic type parameters.

### Covariance (`out`) — preserve direction

If `Dog` extends `Animal`, then `IEnumerable<Dog>` can be used as `IEnumerable<Animal>`.

```csharp
// IEnumerable<T> is declared covariant: IEnumerable<out T>
IEnumerable<Dog> dogs = new List<Dog>();
IEnumerable<Animal> animals = dogs;  // valid

public interface IProducer<out T> {
  T Produce();
}
IProducer<Dog> dogProducer = new DogProducer();
IProducer<Animal> animalProducer = dogProducer;  // valid
```

### Contravariance (`in`) — reverse direction

If `Dog` extends `Animal`, then `Action<Animal>` can be used as `Action<Dog>`.

```csharp
// Action<T> is declared contravariant: Action<in T>
Action<Animal> feedAnimal = a => Console.WriteLine($"Feeding {a.Name}");
Action<Dog> feedDog = feedAnimal;  // valid — can feed any animal, including dogs

public interface IConsumer<in T> {
  void Consume(T item);
}
IConsumer<Animal> animalConsumer = new AnimalConsumer();
IConsumer<Dog> dogConsumer = animalConsumer;  // valid
```

### Summary Diagram

```
Covariance (out):
  Dog ──extends──▶ Animal
  IProducer<Dog> ──▶ IProducer<Animal>    (same direction)

Contravariance (in):
  Dog ──extends──▶ Animal
  IConsumer<Animal> ──▶ IConsumer<Dog>    (reversed)

Invariance (neither):
  List<Dog>  ≠  List<Animal>              (no substitution)
```

| Term | Keyword | Substitution | Example |
|------|---------|-------------|---------|
| Covariance | `out` | Derived → Base | `IEnumerable<Dog>` → `IEnumerable<Animal>` |
| Contravariance | `in` | Base → Derived | `Action<Animal>` → `Action<Dog>` |
| Invariance | — | None | `List<Dog>` ≠ `List<Animal>` |
