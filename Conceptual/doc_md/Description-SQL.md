# SQL Server

---

## SQL Server Limits

| Property | Limit |
|----------|-------|
| Max columns per table | 1,024 |
| Max columns per INSERT | 4,096 |
| Max columns per SELECT | 4,096 |
| Max columns per UPDATE | 4,096 |
| Max non-clustered indexes per table | 999 |

---

## Table Variable vs Temporary Table

Both store temporary data within a session or scope, but they differ in behaviour and performance.

### Table Variables

- **Scope:** Limited to the batch, stored procedure, or function where defined.
- **Transaction:** Part of the transaction — rolled back if the transaction is rolled back.
- **Statistics:** SQL Server does NOT maintain statistics — can lead to less optimal query plans.
- **Memory:** Typically stored in memory; may spill to TempDB if too large.
- **Indexing:** Primary keys only — no explicit non-clustered indexes.
- **Schema:** Defined inline with `DECLARE` — flexible but may cause query plan recompilations.
- **Best for:** Smaller datasets.

```sql
DECLARE @TableVar TABLE (Id INT, Name NVARCHAR(50));
```

### Temporary Tables

- **Scope:** Can be used across scopes and sessions in the same database. Persist until explicitly dropped or session ends.
- **Transaction:** Data can be committed or rolled back.
- **Statistics:** Maintained — aids query optimization.
- **Memory:** Can be in memory or on disk depending on size.
- **Indexing:** Supports primary keys AND non-clustered indexes.
- **Schema:** Defined with `CREATE TABLE`.
- **Best for:** Larger datasets, complex queries, when index control is needed.

```sql
CREATE TABLE #TempTable (Id INT, Name NVARCHAR(50));
```

### Summary

| Feature | Table Variable | Temporary Table |
|---------|---------------|----------------|
| Scope | Batch/Procedure/Function | Session-wide |
| Statistics | No | Yes |
| Indexing | Primary key only | Full indexing support |
| Performance | Better for small data | Better for large data |
| Transaction | Rolled back with transaction | Independent control |

> **Rule of thumb:** Use table variables for small datasets within narrow scope; use temporary tables for larger datasets where index control matters.

---

## Why Functions Cannot Call Stored Procedures

User-defined functions are restricted to **SELECT** statements only — they cannot execute DML statements (`INSERT`, `UPDATE`, `DELETE`).

Stored procedures **can** execute DML statements.

---

## Stack vs Heap Memory (Primary Memory)

### Stack Memory
- Stores local variables, function parameters, return addresses.
- Managed automatically (LIFO — Last In, First Out).
- Generally smaller, limited in size.
- Faster allocation and deallocation.
- Primitive types and references stored here.

### Heap Memory
- Used for dynamically allocated objects (`new` keyword).
- Managed by the programmer or garbage collector (in managed languages).
- Flexible — allocate/deallocate in any order.
- Generally larger.
- Complex objects and data structures stored here.

> In managed languages like C# and Java, the GC abstracts memory management — the programmer rarely manages heap/stack directly.

---

## EF Core and Unit of Work for Transactions

> In EF Core, the **Unit of Work pattern** is used for transaction management. It groups operations that must succeed or fail together, using a single `DbContext` instance across repositories.
