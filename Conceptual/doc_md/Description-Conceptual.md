# .NET Conceptual — C# Core Concepts

---

## Boxing and Unboxing

Occurs at **runtime**.

- **Boxing** — Converting a Value Type (`int`, `char`) → Reference Type (`object`). Implicit.
- **Unboxing** — Converting Reference Type → Value Type. Explicit.

```csharp
int num = 23;
Object obj = num;       // Boxing
int i = (int)obj;       // Unboxing
```

> Value types are stored in **Stack**. Reference types are stored in **Heap**.

---

## Generics and Boxing/Unboxing

Generics avoid Boxing/Unboxing because the compiler replaces `T` with the concrete type at compile time — no runtime wrapping.

```csharp
// Without Generics: boxing/unboxing overhead
// With Generics: no boxing — compiler resolves T at compile time
```

---

## Extension Methods

Add new methods to existing types **without modifying** the original type or creating a derived type.

```csharp
static class NewMethodClass {
  public static void M4(this Geek g) {
    Console.WriteLine("Method Name: M4");
  }
}

// Usage — called like an instance method
Geek g = new Geek();
g.M4();
```

**Advantages:**
1. Add methods without inheritance.
2. No access to source code required.
3. Works with sealed classes.

**Extension Methods vs Static Methods:**
- Extension methods: add utility to existing types you don't own.
- Static methods: utility functions that don't need instance state.

---

## CLR, CTS, and CLS

### CLR — Common Language Runtime
Runtime environment for managed code. Responsibilities:
1. Provides a common runtime environment.
2. Memory management (automatic allocation/deallocation via GC).
3. Converts compiled code (MSIL) → Native code via JIT compiler.

### CTS — Common Type System
Describes data types used by managed code.

| Category | Storage | Examples |
|----------|---------|---------|
| **Value Types** | Stack | `int`, `bool`, enums, structs |
| **Reference Types** | Heap | Classes, arrays, delegates, interfaces |

### CLS — Common Language Specification
Subset of CTS. Rules every .NET language must follow for cross-language interoperability.

---

## Reflection

Allows inspecting and interacting with metadata of assemblies, types, and members **at runtime**.

**Capabilities:**
- Inspect metadata (types, methods, properties, fields).
- Create instances dynamically.
- Invoke methods dynamically.
- Get/set field and property values.

```csharp
Type type = typeof(Example);
object instance = Activator.CreateInstance(type);
MethodInfo method = type.GetMethod("SayHello");
method.Invoke(instance, null);
```

**Trade-offs:**
- Slower than direct method calls.
- No compile-time type safety.
- Can access private members.

---

## Late Binding vs Early Binding

| | Early Binding | Late Binding |
|--|---------------|--------------|
| Resolution | Compile-time | Runtime |
| Type Safety | Yes | No |
| Performance | Faster | Slower |
| IntelliSense | Full support | No support |
| Implementation | Concrete types | `dynamic` / Reflection |

---

## `var` vs `dynamic`

| Feature | `var` | `dynamic` |
|---------|-------|-----------|
| Binding | Early (compile-time) | Late (runtime) |
| Type Safety | Yes — enforced by compiler | No — errors at runtime |
| IntelliSense | Full | None |
| As parameter | No | Yes |

```csharp
var number = 10;        // Type inferred as int at compile time
dynamic number = 10;    // Type determined at runtime
number = "Hello";       // OK for dynamic — runtime error if invalid operation
```

> `dynamic` internally uses reflection for method invocation and type resolution.

---

## Method Overriding

Method overriding uses a **virtual method table (vtable)** — NOT reflection and NOT late binding in the traditional sense.

```csharp
public class BaseClass {
  public virtual void Display() => Console.WriteLine("Base");
}

public class DerivedClass : BaseClass {
  public override void Display() => Console.WriteLine("Derived");
}
```

- Runtime resolves correct method via vtable lookup.
- Method signatures are known at compile time.

---

## `const` vs `readonly`

| | `const` | `readonly` |
|--|---------|------------|
| When set | Compile time | Runtime |
| Can change | No | No (after initialization) |
| Inside method | Yes | No |

```csharp
public const double Pi = 3.14;
public static readonly uint Ticks = (uint)DateTime.Now.Ticks;
```

---

## Value Types vs Reference Types

| Feature | Value Types | Reference Types |
|---------|------------|-----------------|
| Storage | Stack | Heap |
| Default value | 0 / specific | `null` |
| Function passing | Copies value | Passes address |
| Examples | `int`, `bool`, `struct`, `enum` | `class`, `string`, `array`, `delegate` |

---

## Serialization vs Deserialization

- **Serialization** — Convert object → format (JSON/XML/stream) for storage/transmission.
- **Deserialization** — Convert format (JSON/XML/stream) → object.

```csharp
// Serialize: Object → JSON string
// Deserialize: JSON string → Object
```

---

## `is` vs `as` Keywords

```csharp
if (abc is string) { }          // is — checks type
string x = abc as string;       // as — converts type (returns null on failure)
```

---

## Stack vs Heap

| Feature | Stack | Heap |
|---------|-------|------|
| Data stored | Primitives, references | Objects |
| Access | LIFO — faster | Random — slower |
| Memory limit | Smaller | Larger |
| Error | `StackOverflowException` | Managed by GC |
| Cleanup | Auto on scope exit | GC-managed |

---

## Garbage Collector

GC is a background process that cleans **unreferenced managed objects** from the Heap.

**GC Generations:**
| Generation | Contains |
|-----------|---------|
| **GC0** | Short-lived objects — visited most often |
| **GC1** | Intermediate-lived objects |
| **GC2** | Long-lived objects — visited least often |

**Managed vs Unmanaged:**
- **Managed** (CLR-controlled) → cleaned by GC.
- **Unmanaged** (files, DB connections) → NOT cleaned by GC → use Destructors/Dispose.

### Destructor / Finalize
```csharp
~SomeClass() { } // Cleaned by GC, not under programmer control
```

### Dispose Pattern
```csharp
public class SomeClass : IDisposable {
  ~SomeClass() { }

  public void Dispose() {
    // Clean unmanaged resources
    GC.SuppressFinalize(this); // Tell GC: no need to call destructor
  }
}

// Using statement auto-calls Dispose
using (SomeClass obj = new SomeClass()) {
  // work
}
```

### Force GC Collection
```csharp
GC.Collect();    // All generations
GC.Collect(0);   // Generation 0 only
```

### Strong vs Weak References
- **Strong reference** — Object not collected until out of scope.
- **Weak reference** — Permits GC to collect while still accessible until next GC run.

```csharp
WeakReference weakRef = new WeakReference(null);
weakRef.Target = obj;
if (weakRef.IsAlive) { /* obj still accessible */ }
```

---

## Bin vs Obj Folders

Compilation is a two-step process:
1. **Compiling** — Each code file → compiled unit → stored in `obj/` folder.
2. **Linking** — Compiled units → DLL/exe → stored in `bin/` folder.

---

## Collections: IEnumerable, ICollection, IList, IQueryable

### Hierarchy
```
IEnumerable
├── ICollection
│   └── IList
└── IQueryable
```

| Interface | Namespace | Best For | Notes |
|-----------|-----------|----------|-------|
| `IEnumerable` | `System.Collections` | In-memory, forward-only iteration | Uses deferred execution |
| `ICollection` | `System.Collections` | Add/Update/Delete + Count | Extends IEnumerable |
| `IList` | `System.Collections` | Index-based access | Supports insert/remove from middle |
| `IQueryable` | `System.Linq` | DB queries (LINQ to SQL) | Filters at DB level; lazy loading |

### IEnumerable vs IQueryable
```csharp
// IEnumerable — fetches ALL data, filters in memory
IEnumerable<Employee> data = dbContext.Employees.Where(p => p.Name.StartsWith("H"));

// IQueryable — filters at DB, fetches only matching rows
IQueryable<Employee> data = dbContext.Employees.Where(p => p.Name.StartsWith("H"));
```

### Deferred Execution vs Lazy Loading
- **Deferred Execution** — Query not executed until iterated (IEnumerable).
- **Lazy Loading** — Resources (objects, images) loaded only when explicitly requested.

> **Best choice for in-memory iteration:** `List<T>` — materialized, fast, no overhead.

---

## AppDomain

A lightweight process that acts as a container and boundary for .NET code. The CLR uses AppDomain to isolate multiple .NET applications in a single process.

---

## Yield Keyword

Used for stateful iteration — maintains loop position across calls.

```csharp
static IEnumerable<int> ValFunc(List<int> listVal) {
  int cnt = 0;
  foreach (var i in listVal) {
    cnt += i;
    yield return cnt; // state maintained between iterations
  }
}
```

---

## String Comparison

```csharp
// Best practice
val.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

// Object comparison
val1 == val2         // Checks reference equality
val1.Equals(val2)    // Checks content equality
// Note: for string, both == and Equals check content
```

---

## Aggregation vs Composition vs Association

Both Aggregation and Composition describe **HAS-A** relationships.

| Relationship | Lifetime | Coupling | Example |
|-------------|----------|----------|---------|
| **Composition** | Same lifetime | Tightly coupled (1:1) | Patient ↔ Problems |
| **Aggregation** | Different lifetimes | Loosely coupled (M:N) | Patient ↔ Doctor |
| **Association** | Superset of both | Defines dependency between classes | |

UML notation: Composition = filled diamond, Aggregation = empty diamond, Association = arrow.

---

## Struct vs Class vs Record

| Feature | Class | Struct | Record |
|---------|-------|--------|--------|
| Type | Reference | Value | Reference |
| Storage | Heap | Stack | Heap |
| Inheritance | Yes | No | Yes |
| Default constructor | Yes | No | Yes |
| Immutability | No | No | Yes (by default) |
| Use Case | Complex objects | Small, lightweight data | Immutable data structures |

```csharp
// Record example
public record Point(int X, int Y);
Point point1 = new Point(5, 10);
Point point2 = point1 with { X = 8 }; // creates new instance
```

---

## Task.Delay vs Thread.Sleep

| | `Task.Delay()` | `Thread.Sleep()` |
|--|----------------|-----------------|
| Type | Asynchronous | Synchronous |
| Blocking | No — thread continues | Yes — blocks thread |
| Cancellation | Yes (CancellationToken) | No |
| Use | async/await code | Synchronous delays |

```csharp
// Non-blocking
await Task.Delay(1000);

// Blocking
Thread.Sleep(1000);
```

---

## Task.WaitAll vs Task.WhenAll

| | `Task.WaitAll` | `Task.WhenAll` |
|--|----------------|----------------|
| Type | Synchronous | Asynchronous |
| Blocking | Yes — blocks calling thread | No — non-blocking |
| Returns | When all tasks complete | Task completing when all done |

---

## Veracode — Security Vulnerability Testing

Application security testing (AST) tool.

| Scan Type | Description |
|-----------|-------------|
| **SAST (Static Analysis)** | Scans source/binary code without executing |
| **DAST (Dynamic Analysis)** | Simulates real-world attacks on running app |
| **SCA (Software Composition Analysis)** | Identifies vulnerabilities in third-party dependencies |

---

## SonarQube — Code Quality Review

Open-source platform for continuous code quality and static code analysis.

---

## Scrum Ceremonies

| Ceremony | Purpose |
|----------|---------|
| **Backlog Refinement** | Discuss and prioritize backlog items |
| **Sprint Planning** | Select items for next sprint; set sprint goal |
| **Daily Scrum (Stand-up)** | 15-min sync: done/doing/blockers |
| **Scrum of Scrums** | Leads from each scrum sync for large projects |
| **Sprint Review** | Demo completed work to stakeholders |
| **Sprint Retrospective** | Reflect: what worked, what to improve |

---

## MOQ Testing Framework

### MS Test Structure (AAA Pattern)
```csharp
[TestClass]
public class MyTestClass {
  [TestMethod]
  public void TestAddition() {
    // Arrange — set up test data
    int a = 5, b = 3;
    int expected = 8;

    // Act — execute the operation
    int actual = MyMath.Add(a, b);

    // Assert — verify result
    Assert.AreEqual(expected, actual);
  }
}
```

### MOQ — Mock Framework

Used to create mock objects for unit testing in isolation.

```csharp
var employeeRepo = new Mock<IEmployee>();
employeeRepo.Setup(p => p.GetEmployeeById(It.IsAny<int>())).Returns(employee);

var controller = new EmployeeController(employeeRepo.Object);
var result = controller.GetEmployeeById(1);
Assert.IsNotNull(result);
```

**MOQ Advantages over MSTest alone:**
- Dynamic generation of mock types.
- Strongly typed lambda-based API.
- Quick to write — reduces boilerplate.
- Can mock protected methods via string-based API.

---

## IAM — Identity and Access Management

Framework of policies and technologies ensuring the right individuals have appropriate access to resources.

### Key Components

| Component | Description |
|-----------|-------------|
| **Identity Management** | Create/manage user identities and attributes |
| **Authentication** | Verify identity (passwords, MFA, biometrics) |
| **Authorization** | Determine what an identity can access (RBAC) |
| **Access Control** | Enforce policies |
| **Audit and Logging** | Track access attempts and actions |
| **SSO** | Login once, access multiple systems |
| **Provisioning/De-provisioning** | Automate account creation/removal |
| **Identity Federation** | Extend authentication across organizations |
| **Password Management** | Policies and self-service reset |
| **Compliance** | Meet regulatory requirements |

---

## `throw` vs `throw ex` vs `throw new Exception`

| Statement | Stack Trace | Use When |
|-----------|------------|----------|
| `throw` | Preserved — shows original error location | Re-throwing caught exception |
| `throw ex` | Reset — shows re-throw point | Intentionally hiding stack trace |
| `throw new Exception(ex.Message)` | Lost — brand new exception | Avoid — loses all context |

---

## Static Constructor

```csharp
class Test {
  private static int id;

  static Test() {
    if (OtherClass.Id < 10)
      id = 20;
    else
      id = 100;
  }
}
```

- Called **at most once**, before first instance creation or static member access.
- No access modifiers or parameters.
- Cannot access non-static members.
- Useful when one static member's value depends on another static member.
