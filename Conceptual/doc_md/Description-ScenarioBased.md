# Scenario-Based Interview Questions

---

## Handling High Traffic — 100,000 Requests

**Question:** The API is supposed to handle 100,000 requests. How would you handle the situation?

**Answer:**

Before optimizing, run performance tests to understand the baseline:

| Test Type | When to Use |
|-----------|-------------|
| **Load Test** | Constant traffic scenarios |
| **Stress Test** | Gradual increase in traffic (ramp-up) |
| **Spike Test** | Sudden traffic spikes |
| **Soak Test** | Detecting memory leaks — run for extended period to record stable response times |

**Steps:**
1. Run performance tests to identify the bottleneck.
2. Profile memory usage, CPU, and DB query times.
3. Apply caching (response cache, distributed cache) to reduce DB load.
4. Add horizontal scaling (multiple instances behind a load balancer).
5. Use async/await patterns to prevent thread blocking.
6. Optimize DB queries (indexing, query optimization).
7. Consider rate limiting to protect the API from overload.
