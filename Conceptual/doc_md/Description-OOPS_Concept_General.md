# OOP Concepts — General Reference

---

## OOPS — Object-Oriented Programming System

### Features
1. Emphasis on **data** rather than procedure.
2. **Data security**.
3. Follows **bottom-up approach** — modules designed individually, then integrated.

### Core Properties
1. **Classes and Objects**
2. **Data Abstraction and Encapsulation**
3. **Inheritance**
4. **Polymorphism**
5. **Dynamic Binding** (Late Binding)

---

## Data Types

| Category | Examples |
|----------|---------|
| **Built-in** | `int`, `char`, `void`, `float`, `double` |
| **User-Defined** | Struct, Union, Class, enum |
| **Derived** | Array, function, pointer, reference |

---

## Reference Variable

An alias for the original variable — shares the same memory space.

```cpp
float total = 100;
float &sum = total;  // C++ reference
```

### Type Cast in C#
```csharp
int a = (int)1.01;
```

---

## C# Virtual Method

A virtual method can be redefined in derived classes.

```csharp
// Base class
public virtual void Method() { ... }

// Derived class
public override void Method() { ... }
```

- Defined in base class with `virtual`; overridden with `override`.
- Override is optional if derived class has the same definition.
- Runtime uses vtable to determine which implementation to call.

---

## Classes vs Structures

| Feature | Class | Struct |
|---------|-------|--------|
| Type | Reference | Value |
| Inheritance | Yes | No |
| Default constructor | Yes | No (parameterized allowed) |
| `new` keyword | Required | Optional |
| Members | Can be abstract, virtual, protected | Cannot |

---

## C# Access Specifiers

| Specifier | Access Scope |
|-----------|-------------|
| `public` | Anywhere |
| `private` | Own class only |
| `protected` | Own class + derived class |
| `internal` | Same namespace/assembly |
| `protected internal` | Internal in own namespace; protected in derived namespace |
| `private protected` | Protected in own namespace; private in derived namespace |

### Default Specifiers
- Top-level: `internal`
- Class members: `private`

### Static Modifier

```csharp
static class Author {
  public static string A_name = "Ankita";

  public static void Details() {
    Console.WriteLine("Author details");
  }
}

// Usage — no instance needed
Author.Details();
```

---

## Sealed Classes

Prevents inheritance.

```csharp
sealed class SealedClass { }
```

A **method** can also be sealed to prevent further overriding in derived classes:

```csharp
class Y : X {
  sealed protected override void F() { }  // No further override allowed
}
```

---

## Types of Inheritance

| Type | Description |
|------|-------------|
| **Single** | Class B → Class A |
| **Multi-Level** | Class C → Class B → Class A |
| **Multiple** | Via interfaces only (Diamond Problem prevents direct class multiple inheritance) |
| **Multipath** | Diamond structure — NOT supported in C# |
| **Hierarchical** | Multiple classes from one base class |
| **Hybrid** | Combination of above |

### Diamond Problem

```
    A
   / \
  B   C
   \ /
    D
```

If B and C both override A's method, D doesn't know which to call. C# avoids this by disallowing multiple class inheritance (use interfaces instead).

---

## `final` vs `finalize` vs `finally`

| Keyword | Java | C# Equivalent |
|---------|------|--------------|
| `final` class | No inheritance | `sealed` |
| `final` method | No override | `sealed` method |
| `final` variable | Constant | `const` / `readonly` |
| `finally` | Clean-up after try/catch | Same in C# |
| `finalize` | GC clean-up | Destructor `~ClassName()` |

### `finally` Block

```csharp
try { }
catch (Exception ex) { }
finally { /* always executes */ }
```

If an exception is thrown in `finally`:
- The exception propagates out.
- Any unhandled exception from `try` may be **lost**.

---

## `const` vs `readonly`

```csharp
public const double Pi = 3.14;                                // Compile-time
public static readonly uint Ticks = (uint)DateTime.Now.Ticks; // Runtime
```

---

## Managed vs Unmanaged Code

- **Managed Code** — Controlled by CLR; GC handles cleanup.
- **Unmanaged Code** — Not controlled by CLR (files, DB connections); must be manually cleaned.

---

## C# Finalize, Dispose, Destructors, and GC

### Destructor (= Finalize internally)
```csharp
class MyClass {
  public MyClass() { }    // Constructor
  ~MyClass() { }          // Destructor — no parameters or access specifier
}
```
- Implicitly called by GC.
- Cannot be controlled by programmer.

### Garbage Collector
- Tracks all objects; ensures each destroyed once.
- Does not destroy objects still being referenced.
- Destroys when necessary; can explicitly call `GC.Collect()`.

### Dispose Pattern (Microsoft Recommended)

```csharp
class ReadFileClass : IDisposable {
  bool disposed = false;

  public void Dispose() {
    Dispose(true);              // Free managed + unmanaged
    GC.SuppressFinalize(this);  // No need for GC to call destructor
  }

  protected virtual void Dispose(bool disposing) {
    if (disposed) return;
    if (disposing) Console.WriteLine("Freeing Managed Resource");
    Console.WriteLine("Freeing Unmanaged Resource");
    disposed = true;
  }
}
```

**Finalize vs Dispose:**

| | Finalize | Dispose |
|--|----------|---------|
| Control | GC-controlled | Programmer-controlled |
| Cleans | Managed only | Managed + Unmanaged |
| Interface | None | `IDisposable` |

---

## `ref` vs `out`

```csharp
// ref — must be initialized before call
public static string GetNextName(ref int id) {
  id += 1;
  return "Next-" + id;
}

// out — assigned in method, no prior initialization needed
public static string GetNextName(out int id) {
  id = 1;
  return "Next-" + id;
}
```

---

## Serialization vs Deserialization

```
Serialize:   Object → JSON string / XML / stream
Deserialize: JSON string / XML / stream → Object
```

---

## Array vs ArrayList

| Feature | Array | ArrayList |
|---------|-------|-----------|
| Type | Same type only | Any type |
| Size | Fixed | Dynamic |
| Speed | Fast | Slow (boxing/unboxing) |

---

## `System.String` vs `System.Text.StringBuilder`

| | `String` | `StringBuilder` |
|--|---------|----------------|
| Mutability | Immutable — new allocation on change | Mutable — no new allocation |
| Performance | Slower for many modifications | Faster for concatenation |

---

## Boxing and Unboxing

```csharp
// Boxing — value type → object
int anum = 123;
Object obj = anum;

// Unboxing — object → value type
int anum2 = (int)obj;
```

---

## `this` in Static Methods

`this` cannot be used in static methods — static members have no instance. Exception: Extension Methods use `this` on the first parameter.

---

## Extension Methods

```csharp
static class Extensions {
  public static void M4(this MyClass obj) {
    Console.WriteLine("Extended Method");
  }
}

// Usage
MyClass instance = new MyClass();
instance.M4(); // called like an instance method
```

Adds methods to existing types without modifying or inheriting from them.
