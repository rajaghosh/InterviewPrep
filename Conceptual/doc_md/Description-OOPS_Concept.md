# OOP Concepts — C# Detailed

---

## OOPS — Object-Oriented Programming System

### Features
1. Emphasis on **data** rather than procedure.
2. **Data security** — data is hidden from outside world.
3. Follows **bottom-up approach** — modules designed individually, then integrated.
   - Advantage: Testing and Reusability are easier.

### Core Properties
1. **Classes and Objects**
2. **Data Abstraction and Encapsulation** — Wrapping data and functions into a single unit. External access only via methods ("data hiding").
3. **Inheritance** — One class acquires properties of another.
4. **Polymorphism** — Same name, different logic (Method Overloading, Operator Overloading).
5. **Dynamic Binding** — Code associated with a procedure call not known until runtime.

---

## Data Types

| Category | Examples |
|----------|---------|
| **Built-in** | `int`, `char`, `void`, `float`, `double` |
| **User-Defined** | `struct`, `class`, `enum`, `union` |
| **Derived** | Array, function, pointer, reference |

---

## PascalCase vs camelCase

| Convention | Format | Usage |
|-----------|--------|-------|
| **PascalCase** | Each word starts uppercase | Classes, interfaces, namespaces (C#, Java) |
| **camelCase** | First word lowercase | Variables, methods, functions (JS, Python) |

---

## C# Virtual Method

A virtual method can be **redefined in derived classes**. Has an implementation in both base and derived class.

```csharp
public class Animal {
  public virtual void Speak() => Console.WriteLine("Animal sound");
}

public class Dog : Animal {
  public override void Speak() => Console.WriteLine("Woof");
}
```

- Base class declares with `virtual`; derived class overrides with `override`.
- Override is **optional** if derived class has the same definition.
- At runtime, the vtable determines which override to call.

---

## Classes vs Structures

| Feature | Class | Struct |
|---------|-------|--------|
| Type | Reference type | Value type |
| Inheritance | Supported | Not supported |
| Default constructor | Yes | No (can have parameterized) |
| `new` keyword | Required | Optional |
| Members | Can be abstract, virtual, protected | Cannot be abstract, virtual, protected |

---

## C# Access Specifiers

| Specifier | Accessible From |
|-----------|----------------|
| `public` | Anywhere |
| `private` | Own class only |
| `protected` | Own class + directly derived class |
| `internal` | Anywhere within the same namespace/assembly |
| `protected internal` | Internal in own namespace; protected in derived namespace |
| `private protected` | Protected in own namespace; private in derived namespace |

### Default Access
- Top-level declarations: `internal` by default.
- Class members: `private` by default.

---

## Sealed Classes

Prevents a class from being inherited.

```csharp
sealed class SealedClass { }
```

**Purpose:** Indicate the class is specialized — no further extension needed.
**Also:** A method can be `sealed` within a derived class to prevent further overriding.

```csharp
class Y : X {
  sealed protected override void F() { } // No further override allowed
}
```

---

## Types of Inheritance

| Type | Description | C# Support |
|------|-------------|-----------|
| **Single** | One derived class from one base class | Yes |
| **Multi-Level** | Chain of derived classes | Yes |
| **Multiple** | One class from multiple base classes | Via interfaces only |
| **Multipath (Diamond)** | Diamond structure | Not supported (use interfaces) |
| **Hierarchical** | Multiple classes from one base class | Yes |
| **Hybrid** | Combination of above types | Partially |

### Diamond Problem (Why Multiple Inheritance is Avoided)
```
    A
   / \
  B   C
   \ /
    D
```
If B and C both override A's method, D doesn't know which to use. C# solves this by only allowing interfaces for multiple inheritance.

---

## `final` vs `finalize` vs `finally` (Java vs C#)

| Keyword | Java | C# Equivalent |
|---------|------|--------------|
| `final` (class) | Cannot be inherited | `sealed` |
| `final` (method) | Cannot be overridden | `sealed` method |
| `final` (variable) | Constant | `const` / `readonly` |
| `finally` | Clean-up block after try/catch | `finally` (same) |
| `finalize` | GC cleanup before object destroy | Destructor / `~ClassName()` |

### `finally` Block Behaviour

```csharp
try { } catch { } finally { }
// finally ALWAYS executes — even if exception in try/catch
```

**If exception in `finally`:**
- Exception propagates out.
- If try had an unhandled exception, it is **lost** when finally throws.

```csharp
// Case 2: Try block exception LOST if finally also throws
try {
  try {
    throw new Exception("from try");
  }
  finally {
    throw new Exception("from finally"); // try's exception is lost
  }
} catch (Exception ex) {
  Console.WriteLine(ex.Message); // "from finally"
}
```

---

## `const` vs `readonly`

```csharp
public const double Pi = 3.14;                           // Compile-time constant
public static readonly uint Ticks = (uint)DateTime.Now.Ticks; // Runtime constant
```

- `readonly`: set at runtime, cannot change after initialization. Cannot be declared inside a method.
- `const`: compile-time value, predefined.

---

## `ref` vs `out`

Both pass parameters by reference.

| | `ref` | `out` |
|--|-------|-------|
| Must be initialized before call | Yes | No |
| Must be assigned in method | No | Yes |

```csharp
// ref
public static string GetNextName(ref int id) {
  id += 1;
  return "Next-" + id;
}

// out
public static string GetNextName(out int id) {
  id = 1;
  return "Next-" + id;
}
```

---

## `System.String` vs `System.Text.StringBuilder`

| | `System.String` | `StringBuilder` |
|--|-----------------|----------------|
| Mutability | Immutable — new memory on change | Mutable — no new allocation |
| Performance | Slower for frequent modifications | Faster for concatenation loops |

---

## Can `this` Be Used in a Static Method?

**No.** `this` returns a reference to the current instance. Static methods have no instance.
Exception: Extension methods use `this` on the first parameter.

---

## `IS` vs `AS` Keywords

```csharp
if (abc is string) { }          // is — type check
string x = abc as string;       // as — safe cast (returns null on failure)
```

---

## Stacks and Heaps

```
Primitive types → Stack (faster, LIFO, auto-cleanup on scope exit)
Objects         → Heap  (slower, GC-managed)
```

**Stack:** Fixed size; `StackOverflowException` when exhausted.
**Heap:** GC thread monitors and cleans unreferenced managed objects.

---

## Garbage Collection

- GC is a special .NET thread that monitors and frees heap memory.
- Only cleans **managed objects** (CLR-controlled).
- **Unmanaged objects** (files, DB connections) — must be cleaned via Destructors or Dispose pattern.

### Dispose Pattern (Microsoft Recommended)

```csharp
class ReadFileClass : IDisposable {
  bool disposed = false;

  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    if (disposed) return;
    if (disposing) Console.WriteLine("Freeing Managed Resource");
    Console.WriteLine("Freeing Unmanaged Resource");
    disposed = true;
  }
}
```

---

## Managed vs Unmanaged Code

- **Managed Code** — Created and controlled by CLR. Cleaned by GC.
- **Unmanaged Code** — Not controlled by CLR (files, DB connections). GC cannot fully manage.

---

## Boxing and Unboxing

```csharp
// Boxing — value type → object
int anum = 123;
Object obj = anum;

// Unboxing — object → value type
int anum2 = (int)obj;
```

Boxing/Unboxing involves runtime type conversion overhead.

---

## Array vs ArrayList

| Feature | Array | ArrayList |
|---------|-------|-----------|
| Type | Same type only | Any type |
| Size | Fixed | Dynamic |
| Performance | Fast | Slow (boxing/unboxing) |

```csharp
int[] intArray = new int[5];

ArrayList arrayList = new ArrayList();
arrayList.Add(10);
arrayList.Add("Hello"); // different types
```

---

## Shallow Copy vs Deep Copy

```csharp
// Shallow Copy — both variables point to same object
Class1 obj2 = obj1;

// Deep Copy — new independent copy
public object Clone() {
  return (Class1)this.MemberwiseClone();
}
Class1 obj2 = (Class1)obj1.Clone();
```
