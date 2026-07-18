# Entity Framework Core

---

## Overview

EF Core is an **ORM (Object-Relational Mapper)** for communicating with databases via .NET Domain Classes.

### Key Roles of DbContext
1. Managing DB connection.
2. Configuring entities and relationships.
3. CRUD operations.
4. Transaction management.

```csharp
public class MyContext : DbContext {
  public DbSet<Employee> Employees { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder builder) {
    if (!builder.IsConfigured)
      builder.UseSqlServer("<DBConnection>");
  }
}
```

> `DbSet<T>` — Represents a collection for a given entity; the gateway for DB operations.

---

## Two Approaches

### 1. Code-First
Create domain models → generate DB schema via migrations.

```powershell
Add-Migration "MigrationFileName" -context DbContextName
Update-Database
```

### 2. DB-First
Create DB schema first → scaffold into classes.

```powershell
Scaffold-DbContext "<DBConnection>" Microsoft.EntityFrameworkCore.SqlServer -OutputDir EFModels
```

---

## Setting Up EF Core

### Required NuGet Packages
```
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.Design
```

### Migration File Methods
- `Up()` — Commands executed when migration is applied.
- `Down()` — Commands executed when migration is removed.

---

## Fluent API

Overrides default EF Core conventions for more configuration options.

---

## Lazy Loading in EF Core

### Option 1: With Proxy Package

```csharp
// Install: Microsoft.EntityFrameworkCore.Proxies
public void ConfigureServices(IServiceCollection services) {
  services.AddDbContext<EFCoreContext>(b => b
    .UseLazyLoadingProxies()
    .UseSqlServer(ConnectionString));
}
```

### Option 2: Without Proxy Package — Using `ILazyLoader`

```csharp
public class Actor {
  private List<Movie> _movies;
  private ILazyLoader LazyLoader { get; set; }

  private Actor(ILazyLoader lazyLoader) {
    LazyLoader = lazyLoader;
  }

  public List<Movie> Movies {
    get => LazyLoader.Load(this, ref _movies);
    set => _movies = value;
  }
}
```

### When is Lazy Loading Useful?
- One-to-many relationships where associated entities are not needed immediately.
- Reduces startup time, memory usage, and DB query load.

---

## MVC-Related Questions

### ViewData vs ViewBag vs TempData

| | ViewData | ViewBag | TempData |
|--|---------|---------|---------|
| Type | Key-Value Dictionary | Dynamic object | Key-Value Dictionary |
| Speed | Faster | Slower | N/A |
| Syntax | `ViewData["key"]` | `ViewBag.Key` | `TempData["key"]` |

```csharp
ViewData["Message"] = "Hello!";
ViewBag.Name = "John";
```

### `[NonAction]`
Prevents a public method from being accessible as an action.

### `RoutePrefix` vs `Route`
```csharp
[RoutePrefix("api/students")]
public class StudentsController : ApiController {
  [Route("{id}")]         // maps to api/students/{id}
  public Student Get(int id) { ... }

  [Route("{id}/courses")] // maps to api/students/{id}/courses
  public IEnumerable<string> GetStudentCourses(int id) { ... }
}
```

### Output Cache
Caches the content returned by a controller action.

---

## Unit of Work Pattern

Groups multiple operations into a **single transaction** — all succeed or all fail.

### Benefits
- **Separation of Concerns** — business logic separated from data access.
- **Testability** — easy to mock repositories.
- **Consistency** — all operations share the same `DbContext`.

### Implementation

#### Step 1: Define the Interface
```csharp
public interface IUnitOfWork : IDisposable {
  IRepository<T> Repository<T>() where T : class;
  void Save();
}
```

#### Step 2: Implement
```csharp
public class UnitOfWork : IUnitOfWork {
  private readonly DbContext _context;
  private readonly Dictionary<Type, object> _repositories = new();

  public UnitOfWork(DbContext context) => _context = context;

  public IRepository<T> Repository<T>() where T : class {
    if (!_repositories.ContainsKey(typeof(T)))
      _repositories[typeof(T)] = new Repository<T>(_context);
    return (IRepository<T>)_repositories[typeof(T)];
  }

  public void Save() => _context.SaveChanges();
  public void Dispose() => _context.Dispose();
}
```

#### Step 3: Register in DI
```csharp
services.AddDbContext<YourDbContext>(options =>
  options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

services.AddScoped<IUnitOfWork, UnitOfWork>();
```

#### Step 4: Use in Service
```csharp
public class YourService {
  private readonly IUnitOfWork _unitOfWork;

  public YourService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

  public void SomeOperation() {
    var repository = _unitOfWork.Repository<YourEntity>();
    var entities = repository.GetAll();
    // perform operations
    _unitOfWork.Save();
  }
}
```

---

## Repository Pattern

Abstracts the data access layer, providing a collection-like interface for domain objects.

### Generic Repository Interface

```csharp
public interface IRepository<T> where T : class {
  IEnumerable<T> GetAll();
  T GetById(int id);
  void Add(T entity);
  void Update(T entity);
  void Delete(T entity);
}
```

### Generic Repository Implementation

```csharp
public class Repository<T> : IRepository<T> where T : class {
  private readonly DbContext _context;
  private readonly DbSet<T> _dbSet;

  public Repository(DbContext context) {
    _context = context;
    _dbSet = context.Set<T>();
  }

  public IEnumerable<T> GetAll() => _dbSet.ToList();
  public T GetById(int id) => _dbSet.Find(id);
  public void Add(T entity) => _dbSet.Add(entity);

  public void Update(T entity) {
    _dbSet.Attach(entity);
    _context.Entry(entity).State = EntityState.Modified;
  }

  public void Delete(T entity) {
    if (_context.Entry(entity).State == EntityState.Detached)
      _dbSet.Attach(entity);
    _dbSet.Remove(entity);
  }
}
```

---

> **Note:** In EF Core, the **Unit of Work pattern** is used for transaction management across multiple repositories.
