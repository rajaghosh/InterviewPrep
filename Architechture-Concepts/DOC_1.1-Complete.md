# System Design & AI Architecture Interview Guide
### Topics: HTTP QUERY Verb · Polling/SSE/WebSockets · Rate Limiting · RAG · Enterprise AI Patterns · Event Sourcing

> **Who this is for:** College students and senior engineers preparing for system design and AI architecture interviews. Every section includes an "Interview Language" block with phrases that signal architectural maturity to interviewers.

---

## Table of Contents

1. [The HTTP QUERY Verb](#topic-1-the-http-query-verb)
2. [Short Polling vs SSE vs WebSockets](#topic-2-short-polling-vs-sse-vs-websockets)
3. [Rate Limiting — Sliding Window Algorithm](#topic-3-rate-limiting--sliding-window-algorithm)
4. [RAG — Complete Guide](#topic-4-rag-retrieval-augmented-generation--complete-guide)
5. [6 Enterprise AI Architecture Patterns](#topic-5-6-enterprise-ai-architecture-patterns)
6. [Event Sourcing — Deep Dive](#topic-6-event-sourcing--deep-dive)

---

## TOPIC 1: The HTTP QUERY Verb

### The Problem Space: How We Got Here

Before QUERY existed, developers faced a fundamental tension between HTTP semantics and real-world search requirements. To understand why QUERY matters, you need to understand why the alternatives fail.

---

### Why GET Fails for Complex Searches

GET is the workhorse of the web. It is **safe** (read-only, no side effects) and **idempotent** (calling it 100 times gives the same result as calling it once). These are excellent properties.

The problem: **GET encodes all parameters in the URL string.**

```
GET /products?category=electronics&brand=apple&minPrice=500&maxPrice=2000
    &color=silver&storage=256gb&condition=new&inStock=true
    &sortBy=relevance&sortOrder=desc&tags=featured,sale
    &releaseYearFrom=2022&releaseYearTo=2024&rating=4
```

This URL is already unwieldy. Now imagine:
- An array of 50 product IDs to filter by
- A nested JSON structure with AND/OR filter logic
- A geographic bounding box with polygon coordinates
- A full-text search query with boost weights

URLs have **practical and hard limits**:

| Limit Type | Value | What Breaks |
|---|---|---|
| HTTP spec (RFC) | No defined limit | (spec is silent) |
| Most web servers | 8 KB | Server returns 414 |
| Apache default | 8 KB | 414 URI Too Long |
| Nginx default | 8 KB | 414 URI Too Long |
| Internet Explorer | 2 KB | Silently truncated |
| Chrome | ~2 MB | Works but impractical |

When a URL exceeds server limits, the server returns **HTTP 414 URI Too Long** — a hard failure in production. Even below that limit, long URLs cause real problems:

- **Logging and observability:** Gigantic URLs pollute logs, making them hard to parse
- **Caching:** Proxy caches key on the full URL. Tiny parameter-order differences cause cache misses
- **Bookmarking / sharing:** Users cannot share a URL with 200 filter parameters
- **Security:** Sensitive filter criteria (e.g., user IDs, price ranges) appear in access logs and browser history

**The analogy:** Imagine a librarian (the server) who can only receive your search request written on the outside of an envelope. Eventually you run out of room on the envelope. QUERY lets you put the search criteria *inside* the envelope.

---

### Why POST Was Misused — And Why That's Architecturally Wrong

When GET became impractical, many teams reached for POST because POST accepts a request body.

```http
POST /products/search
Content-Type: application/json

{
  "filters": {
    "category": "electronics",
    "priceRange": { "min": 500, "max": 2000 },
    "brands": ["apple", "samsung", "sony"],
    "inStock": true
  },
  "sort": { "field": "relevance", "order": "desc" },
  "pagination": { "page": 1, "size": 20 }
}
```

This *works*, but it violates HTTP semantics in two important ways:

**1. POST is not idempotent**

POST is defined by HTTP spec as a resource-creation verb. Sending the same POST request twice is supposed to create two resources. Caches, proxies, and load balancers treat POST as "this might change state — do not cache, do not retry automatically."

When you use POST for a search, you are lying to the infrastructure layer. Consequences:
- **CDN/reverse proxy:** Will not cache your search results (caching a write operation would be unsafe)
- **Retry logic:** HTTP clients do not automatically retry failed POST requests (they might for GET)
- **API gateways:** May log POST operations as mutations in audit trails
- **Client libraries:** `fetch()`, `axios`, etc., will not follow redirects on POST the same way they do for GET

**2. POST is not safe**

"Safe" in HTTP means "the client is not requesting a state change." GET, HEAD, OPTIONS, and QUERY are safe. POST, PUT, PATCH, DELETE are not. This distinction matters for:

- Browser preflight caching
- Search engines (Googlebot will not POST)
- HTTP middleware that treats safe methods differently

**The architectural principle violated:** HTTP verbs are a semantic contract with every piece of infrastructure between your client and your server. Breaking that contract introduces subtle, hard-to-debug failures at scale.

---

### What QUERY Solves

QUERY is an HTTP method proposed via IETF draft (draft-ietf-httpbis-safe-method-w-body) that combines the best properties:

- **Safe:** Does not alter server state (read-only)
- **Idempotent:** Calling it N times with the same body returns the same result
- **Has a request body:** Can carry arbitrarily complex search criteria

```http
QUERY /products
Content-Type: application/json

{
  "filters": {
    "category": "electronics",
    "priceRange": { "min": 500, "max": 2000 },
    "brands": ["apple", "samsung", "sony"],
    "inStock": true
  },
  "sort": { "field": "relevance", "order": "desc" },
  "pagination": { "page": 1, "size": 20 }
}
```

Now every layer of your infrastructure can correctly infer: "This is a read operation. I can cache it. I can retry it. I can log it as a query, not a mutation."

---

### Full HTTP Verb Comparison Table

| Verb | Primary Action | Idempotent | Safe | Has Body | Cacheable |
|------|---------------|:----------:|:----:|:--------:|:---------:|
| **GET** | Retrieve resource | Yes | Yes | No | Yes |
| **POST** | Create resource | No | No | Yes | Rarely |
| **PUT** | Replace entire resource | Yes | No | Yes | No |
| **PATCH** | Partially update resource | No* | No | Yes | No |
| **DELETE** | Remove resource | Yes | No | No** | No |
| **HEAD** | GET but no body in response | Yes | Yes | No | Yes |
| **OPTIONS** | Describe communication options | Yes | Yes | No | No |
| **QUERY** | Search/retrieve with complex criteria | Yes | Yes | Yes | Yes |

*PATCH idempotency depends on implementation. A PATCH that sets a field to a value is idempotent; one that appends to a list is not.
**DELETE technically can have a body per spec, but it is rarely used.

---

### Concrete Example: Searching Products with 10 Filters

**The GET Attempt (breaks at scale)**

```
GET /products?category=electronics&brand=apple&brand=samsung&minPrice=500
    &maxPrice=2000&color=silver&storage=256gb&condition=new&inStock=true
    &sortBy=relevance&sortOrder=desc&tags=featured&tags=sale
    &releaseYearFrom=2022&releaseYearTo=2024&minRating=4
    &excludeIds=101,202,303,404,505,606,707,808,909,1010
```

Problems: URL is 300+ characters, no nested logic possible, arrays are ambiguous (`brand=apple&brand=samsung` vs `brand=apple,samsung`), breaks at 8 KB.

**The POST Workaround (works but semantically wrong)**

```http
POST /products/search
Content-Type: application/json

{
  "filters": { ... }
}
```

Problems: CDNs will not cache it, retry logic is off, audit trails show it as a write operation, REST purists will reject it in code review.

**The Clean QUERY Version**

```http
QUERY /products
Content-Type: application/json
Accept: application/json

{
  "filters": {
    "category": "electronics",
    "brands": ["apple", "samsung"],
    "priceRange": { "min": 500, "max": 2000 },
    "attributes": {
      "color": "silver",
      "storage": "256gb",
      "condition": "new"
    },
    "inStock": true,
    "minRating": 4,
    "releaseYear": { "from": 2022, "to": 2024 },
    "tags": { "operator": "OR", "values": ["featured", "sale"] },
    "excludeIds": [101, 202, 303, 404, 505]
  },
  "sort": { "field": "relevance", "order": "desc" },
  "pagination": { "page": 1, "pageSize": 20 }
}
```

This is cacheable, safe, idempotent, semantically correct, and infinitely extensible.

---

### Implementation Notes

**Node.js (Express) — supporting QUERY verb**

```javascript
// Express does not natively route QUERY; use the .all() method
app.all('/products', (req, res) => {
  if (req.method === 'QUERY') {
    const { filters, sort, pagination } = req.body;
    // Execute search logic
    return res.json({ results: [] });
  }
  res.status(405).json({ error: 'Method Not Allowed' });
});
```

**Python (FastAPI)**

```python
from fastapi import FastAPI, Request
from fastapi.routing import APIRoute

app = FastAPI()

@app.api_route("/products", methods=["QUERY"])
async def search_products(request: Request):
    body = await request.json()
    filters = body.get("filters", {})
    # Execute search
    return {"results": []}
```

---

### Best Practices

1. **Noun-based paths always:** `/products`, `/users`, `/orders` — not `/searchProducts` or `/getUser`
2. **Let the verb carry the action:** The HTTP method IS the verb. Your path IS the noun.
3. **Idempotency is infrastructure trust:** Safe + idempotent methods unlock caching, automatic retries, and CDN optimization. Do not throw that away.
4. **Use QUERY when:** your search criteria is too complex for URL parameters, involves nested structures, or contains sensitive data that should not appear in access logs.
5. **Return 405 Method Not Allowed** for unsupported methods, not 404.

---

### Interview Language

> "GET is safe and idempotent but cannot carry a body, which makes it impractical for complex search use cases — URLs hit 414 limits and cannot express nested filter logic. The common workaround of using POST for search violates HTTP semantics because POST is defined as non-idempotent, which causes CDNs and proxies to refuse to cache the response and causes retry logic to behave incorrectly. The QUERY verb solves this cleanly: it is safe, idempotent, and accepts a request body, so every layer of infrastructure — from the CDN to the load balancer — correctly treats it as a read operation."

> "In a high-traffic search API, the difference between GET/POST and QUERY is not just philosophical. An idempotent, cacheable QUERY can be served from a CDN edge node; a POST cannot. At 10,000 requests per second, that's the difference between cache hits costing microseconds and origin server hits costing milliseconds."

---

## TOPIC 2: Short Polling vs SSE vs WebSockets

### The Core Problem: Pushing Data to the Browser

HTTP was designed as a request-response protocol: the client asks, the server answers, the connection closes. But modern applications need the server to push updates to the client — think a live dashboard, a chat message, a delivery tracker, or an AI streaming response.

Four main strategies exist, with dramatically different tradeoffs.

---

### Strategy 1: Short Polling

**What it is:** The client sets a timer and asks the server "anything new?" at a fixed interval. The server responds immediately (with data or an empty response), and the connection closes.

```
Client                     Server
  |------ GET /updates -------->|
  |<----- { data: [] } ---------|   (empty — nothing new)
  |
  | (waits 2 seconds)
  |
  |------ GET /updates -------->|
  |<----- { data: [] } ---------|   (still empty)
  |
  | (waits 2 seconds)
  |
  |------ GET /updates -------->|
  |<----- { data: [msg1] } -----|   (finally has data)
```

**JavaScript implementation:**

```javascript
function startPolling(intervalMs = 2000) {
  setInterval(async () => {
    try {
      const response = await fetch('/api/updates?since=' + lastTimestamp);
      const data = await response.json();
      if (data.events.length > 0) {
        processEvents(data.events);
        lastTimestamp = data.events[data.events.length - 1].timestamp;
      }
    } catch (err) {
      console.error('Poll failed:', err);
    }
  }, intervalMs);
}
```

**When it works:** Simple use cases, low frequency updates, environments where persistent connections are blocked by firewalls or proxies (corporate networks).

**Why it's wasteful:**
- Every request opens a new TCP connection (unless HTTP/2 is used)
- The majority of responses are empty — pure wasted bandwidth
- Latency = up to the full polling interval. At 2-second intervals, average latency is 1 second.
- At 1,000 concurrent users polling every 2 seconds: 500 requests/second hitting your server even when nothing is happening.

---

### Strategy 2: Long Polling

**What it is:** The client sends a request, but the server holds the connection open until data is available (or a timeout is reached). When the server responds, the client immediately sends another request.

```
Client                     Server
  |------ GET /updates -------->|
  |                             | (holds connection, waiting for data)
  |                             | (30 seconds pass...)
  |<----- { data: [msg1] } -----|   (data arrives, server responds)
  |
  | (immediately re-connects)
  |
  |------ GET /updates -------->|
  |                             | (holds again...)
```

**Advantages over short polling:** Eliminates most empty responses. Latency drops to near-zero (response is sent as soon as data exists).

**Problems:**
- Server must hold thousands of open connections — increases memory usage significantly
- Timeout handling is complex (what happens when the server-side timeout fires?)
- Not truly real-time for multiple simultaneous events (server must queue them)
- Does not scale well horizontally (sticky sessions or shared state needed)

---

### Strategy 3: Server-Sent Events (SSE)

**What it is:** A persistent HTTP connection where the server continuously streams newline-delimited text events to the client. One-directional: server to client only.

```
Client                     Server
  |--- GET /events (EventSource) ->|
  |<-- HTTP 200 (text/event-stream)|
  |<-- data: {"type":"price","value":142.50}\n\n
  |<-- data: {"type":"price","value":143.00}\n\n
  |<-- data: {"type":"alert","msg":"Volume spike"}\n\n
  | (connection stays open indefinitely)
```

**The SSE wire format** is beautifully simple:

```
id: 1001
event: priceUpdate
data: {"symbol":"AAPL","price":142.50}

id: 1002
event: priceUpdate
data: {"symbol":"AAPL","price":143.00}

: this is a comment, often used as a heartbeat

```

Each event is separated by a blank line (`\n\n`). The `id` field enables automatic reconnection — if the connection drops, the browser sends `Last-Event-ID` in the reconnect request so the server can replay missed events.

**JavaScript Client (EventSource API):**

```javascript
const eventSource = new EventSource('/api/events');

// Generic message handler
eventSource.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Received:', data);
};

// Named event handler
eventSource.addEventListener('priceUpdate', (event) => {
  const { symbol, price } = JSON.parse(event.data);
  updatePriceDisplay(symbol, price);
});

// Connection opened
eventSource.onopen = () => {
  console.log('SSE connection established');
};

// Error / reconnection
eventSource.onerror = (event) => {
  if (event.target.readyState === EventSource.CLOSED) {
    console.log('SSE closed, will auto-reconnect in 3s');
    // Browser automatically reconnects after 3 seconds by default
  }
};

// Clean up
// eventSource.close();
```

**Node.js Server (Express):**

```javascript
app.get('/api/events', (req, res) => {
  // Set SSE headers
  res.setHeader('Content-Type', 'text/event-stream');
  res.setHeader('Cache-Control', 'no-cache');
  res.setHeader('Connection', 'keep-alive');
  res.flushHeaders(); // Send headers immediately

  let eventId = 0;

  // Send a heartbeat every 30 seconds to prevent proxy timeouts
  const heartbeat = setInterval(() => {
    res.write(': heartbeat\n\n');
  }, 30000);

  // Subscribe to your event source (e.g., Redis pub/sub, a queue)
  const subscription = eventBus.subscribe('priceUpdates', (data) => {
    eventId++;
    res.write(`id: ${eventId}\n`);
    res.write(`event: priceUpdate\n`);
    res.write(`data: ${JSON.stringify(data)}\n\n`);
  });

  // Clean up when client disconnects
  req.on('close', () => {
    clearInterval(heartbeat);
    subscription.unsubscribe();
    res.end();
  });
});
```

**Key SSE advantages:**
- Uses standard HTTP — works through existing load balancers, CDNs, and proxies
- Native browser API with automatic reconnection (with `Last-Event-ID`)
- Multiplexes over HTTP/2 (many SSE streams share one connection)
- Named events allow client-side routing without a separate dispatch layer
- Perfect for AI streaming responses (ChatGPT uses SSE to stream tokens)

---

### Strategy 4: WebSockets

**What it is:** A full-duplex, bidirectional, persistent TCP connection that begins with an HTTP upgrade handshake. Both client and server can send messages at any time.

```
Client                     Server
  |--- HTTP GET /ws ----------->|   (Upgrade: websocket)
  |<-- HTTP 101 Switching -------|   (connection upgraded)
  |<======= WS FRAME ==========>|   (bidirectional from here)
  |========> WS FRAME =========|
  |<======= WS FRAME ==========>|
  | (persists until explicitly closed)
```

**JavaScript Client:**

```javascript
const ws = new WebSocket('wss://api.example.com/ws');

ws.onopen = () => {
  console.log('WebSocket connected');
  // Can send immediately
  ws.send(JSON.stringify({ type: 'subscribe', channel: 'AAPL' }));
};

ws.onmessage = (event) => {
  const msg = JSON.parse(event.data);
  if (msg.type === 'priceUpdate') {
    updatePriceDisplay(msg.symbol, msg.price);
  }
};

ws.onclose = (event) => {
  console.log(`Closed: code=${event.code}, reason=${event.reason}`);
  // Manual reconnection needed (no built-in reconnect like SSE)
  setTimeout(reconnect, 3000);
};

ws.onerror = (error) => {
  console.error('WebSocket error:', error);
};

// Sending from client to server
function sendChatMessage(text) {
  ws.send(JSON.stringify({ type: 'chat', text, userId: currentUser.id }));
}
```

**WebSocket advantages:**
- True bidirectional: client can push data to server at any time without new requests
- Low latency: no HTTP overhead per message after the initial handshake
- Binary support: can send raw binary data (images, audio, game state), not just text
- Ideal for: chat apps, multiplayer games, collaborative editors, live trading platforms

**WebSocket challenges:**
- Does not work transparently through all HTTP proxies (some strip the Upgrade header)
- No built-in reconnection (you must implement it yourself)
- Stateful: WebSocket connections are tied to a specific server instance; horizontal scaling requires shared state (e.g., Redis pub/sub to fan out messages across server instances)
- More complex operational footprint than SSE

---

### Full Comparison Table

| Feature | Short Polling | Long Polling | SSE | WebSockets |
|---|---|---|---|---|
| **Direction** | Client → Server | Client → Server | Server → Client | Bidirectional |
| **Protocol** | HTTP | HTTP | HTTP | WS (upgraded from HTTP) |
| **Connection** | New per request | Held open | Persistent | Persistent |
| **Auto-reconnect** | Timer-based | Client re-requests | Yes (native) | No (manual) |
| **Browser support** | Universal | Universal | All modern + IE polyfill | All modern |
| **Proxy-friendly** | Yes | Yes | Yes | Sometimes not |
| **HTTP/2 multiplexing** | Yes | Partial | Yes | No (separate connection) |
| **Binary support** | With encoding | With encoding | No (text only) | Yes |
| **Server complexity** | Very low | Medium | Low-medium | High |
| **Ideal for** | Low-freq updates, simple | Medium-freq, fire-and-forget | Dashboards, AI streaming, notifications | Chat, gaming, collaboration |
| **AI token streaming** | Poor (batched) | Poor | Excellent | Possible but overkill |

---

### Decision Guide: When to Use Each

```
Does the client need to SEND data to the server in real-time?
├── YES → WebSockets (chat, gaming, collaborative editing)
└── NO  → Server pushes only. How frequently?
          ├── Low frequency (>10s intervals) or simple → Short Polling
          ├── Medium frequency, event-driven → Long Polling or SSE
          └── High frequency or streaming text → SSE
                ├── AI token streaming → SSE
                ├── Live dashboards → SSE
                └── Notifications → SSE
```

---

### Interview Language

> "For a live dashboard showing stock prices, I would use SSE over WebSockets. SSE is one-directional — the server pushes updates — which matches our use case. It uses standard HTTP, so it works through existing proxies and load balancers without special configuration. It has a native browser reconnection mechanism with Last-Event-ID. The only scenario where I would upgrade to WebSockets is if the client also needs to push data in real-time — for example, a collaborative trading platform where users can place orders. WebSockets add significant operational complexity: you need sticky sessions or a Redis pub/sub layer to fan out messages across horizontal replicas."

> "Short polling is not always wrong. For an admin dashboard that refreshes data every 30 seconds, short polling is simpler to build, simpler to debug, and works in every environment. Over-engineering to WebSockets for a low-frequency use case adds complexity with no user-visible benefit. I choose the simplest pattern that satisfies the latency requirement."

> "SSE is the right answer for AI streaming responses — this is exactly how ChatGPT streams tokens. The server sends each token as a text event as the LLM generates it. The browser's EventSource API renders them incrementally without any custom client code."

---

## TOPIC 3: Rate Limiting — Sliding Window Algorithm

### Why Rate Limiting Matters

Rate limiting is the enforcement of a maximum number of requests a client can make to an API within a time window. Without it:

- A single misconfigured client script can saturate your servers
- Malicious actors can use credential stuffing at scale (trying 10,000 passwords/minute)
- One noisy tenant in a multi-tenant system degrades everyone's experience
- DDoS attacks become trivially effective

Rate limiting is not just a security measure — it is a **fairness and reliability mechanism**. The goal is not to block legitimate users; it is to ensure no single client consumes a disproportionate share of capacity.

---

### Algorithm 1: Fixed Window Counter

**How it works:** Divide time into fixed windows (e.g., each minute from :00 to :59). Count requests per client per window. Reject when count exceeds limit.

```
Limit: 100 requests per minute

Window 1: 00:00 - 00:59  → Client makes 80 requests → OK
Window 2: 01:00 - 01:59  → Client makes 90 requests → OK
```

**The boundary exploitation problem:**

```
Limit: 100 requests per minute

Time 00:59 → Client makes 100 requests (filling Window 1)
Time 01:00 → Window resets. Client immediately makes 100 requests.
Result: 200 requests in 2 seconds — double the intended rate
```

A client who knows your window boundaries can burst 2× the limit at every boundary. This is the **thundering herd at window boundaries** problem.

**Redis implementation:**

```python
import redis
import time

r = redis.Redis()

def is_allowed_fixed_window(client_id: str, limit: int, window_seconds: int) -> bool:
    window_key = f"ratelimit:{client_id}:{int(time.time() // window_seconds)}"
    count = r.incr(window_key)
    if count == 1:
        r.expire(window_key, window_seconds)  # Set expiry on first request
    return count <= limit
```

---

### Algorithm 2: Sliding Window Log

**How it works:** Keep a log of the exact timestamp of every request. To check if a new request is allowed, count how many log entries fall within the last N seconds.

```
Limit: 100 requests per minute

Request at 1:30:45.123 → Check: how many requests between 1:29:45.123 and 1:30:45.123?
                          Count: 87 → Allow, add timestamp to log
```

**Accurate at the cost of memory:** For high-traffic APIs, storing every request timestamp for every client is expensive. If you have 1 million clients and each makes 100 requests/minute, you are storing 100 million timestamps per minute.

**Redis implementation (using sorted sets):**

```python
def is_allowed_sliding_log(client_id: str, limit: int, window_ms: int) -> bool:
    now_ms = int(time.time() * 1000)
    window_start = now_ms - window_ms
    key = f"ratelimit:log:{client_id}"
    
    pipe = r.pipeline()
    pipe.zremrangebyscore(key, 0, window_start)   # Remove expired entries
    pipe.zcard(key)                                # Count remaining
    pipe.zadd(key, {str(now_ms): now_ms})         # Add current request
    pipe.expire(key, window_ms // 1000 + 1)       # TTL cleanup
    results = pipe.execute()
    
    current_count = results[1]
    return current_count < limit
```

---

### Algorithm 3: Sliding Window Counter (Production Standard)

**The key insight:** You do not need exact timestamps. You need a *good estimate* of how many requests have occurred in the rolling window. A weighted combination of the previous window and current window gives a surprisingly accurate estimate.

**The formula:**

```
count = prev_window_count × (1 - elapsed_fraction) + current_window_count

Where:
  elapsed_fraction = (current_time % window_size) / window_size
```

**Worked example:**

```
Limit: 100 requests per minute
Window size: 60 seconds

Previous window (00:00 - 00:59): 80 requests
Current window start: 01:00
Current time: 01:15 (25% into current window)
Current window so far: 30 requests

Elapsed fraction = 15 / 60 = 0.25

Estimated count = 80 × (1 - 0.25) + 30
               = 80 × 0.75 + 30
               = 60 + 30
               = 90 → ALLOW (90 < 100)
```

If the client then makes 15 more requests rapidly:

```
Current window: 45 requests

Estimated count = 80 × 0.75 + 45 = 60 + 45 = 105 → REJECT
```

**Why this works:** The formula assumes the previous window's requests were uniformly distributed over that minute. If 80 requests happened in minute 0 and we're 15 seconds into minute 1, we estimate that roughly 60 of those 80 are "still in the window" (the ones from the last 45 seconds of minute 0). This is an approximation, but it is accurate enough for production and uses only O(1) memory per client.

**Redis implementation:**

```python
import redis
import time
import math

r = redis.Redis()

def is_allowed_sliding_window(client_id: str, limit: int, window_seconds: int) -> bool:
    now = time.time()
    current_window = math.floor(now / window_seconds)
    prev_window = current_window - 1
    elapsed_fraction = (now % window_seconds) / window_seconds
    
    curr_key = f"ratelimit:{client_id}:{current_window}"
    prev_key = f"ratelimit:{client_id}:{prev_window}"
    
    # Atomic read of both counters
    pipe = r.pipeline()
    pipe.get(prev_key)
    pipe.get(curr_key)
    results = pipe.execute()
    
    prev_count = int(results[0] or 0)
    curr_count = int(results[1] or 0)
    
    # Weighted estimate
    estimated = prev_count * (1 - elapsed_fraction) + curr_count
    
    if estimated >= limit:
        return False
    
    # Increment current window counter atomically
    pipe = r.pipeline()
    pipe.incr(curr_key)
    pipe.expire(curr_key, window_seconds * 2)  # Keep for 2 windows
    pipe.execute()
    return True
```

**For true atomicity (critical in distributed systems), use a Lua script:**

```lua
-- Atomic sliding window counter in Lua (executed atomically by Redis)
local curr_key = KEYS[1]
local prev_key = KEYS[2]
local limit = tonumber(ARGV[1])
local elapsed_fraction = tonumber(ARGV[2])
local window_seconds = tonumber(ARGV[3])

local prev_count = tonumber(redis.call('GET', prev_key) or 0)
local curr_count = tonumber(redis.call('GET', curr_key) or 0)

local estimated = prev_count * (1 - elapsed_fraction) + curr_count

if estimated >= limit then
    return 0  -- Rejected
end

redis.call('INCR', curr_key)
redis.call('EXPIRE', curr_key, window_seconds * 2)
return 1  -- Allowed
```

---

### Algorithm 4: Token Bucket

**Analogy:** A bucket that holds tokens. The bucket fills at a fixed rate (e.g., 10 tokens/second, up to a max of 100 tokens). Each request consumes one token. If the bucket is empty, the request is rejected.

```
Bucket capacity: 100 tokens
Refill rate: 10 tokens/second

Second 0:   Bucket = 100. Client makes 50 requests. Bucket = 50.
Second 1:   Bucket = 50 + 10 = 60. Client makes 80 requests. Bucket = 0. 40 rejected.
Second 2:   Bucket = 0 + 10 = 10. Client makes 5 requests. Bucket = 5.
```

**Key property:** Allows controlled **bursting** up to the bucket capacity. A client can consume all 100 tokens in a single burst, then must wait for the bucket to refill. This is useful for clients that are idle for long periods and then have a brief spike of legitimate requests.

---

### Algorithm 5: Leaky Bucket

**Analogy:** Water pours into a bucket at any rate, but the bucket has a fixed-size hole at the bottom that drains at a constant rate. If the bucket overflows, the water (request) is discarded.

- **Effect:** Smooths traffic to a constant rate. No bursting allowed.
- **Use case:** When you need to protect a backend that can only handle exactly N requests per second (e.g., a legacy system with a hard throughput ceiling).
- **Difference from Token Bucket:** Token Bucket allows bursting within the capacity; Leaky Bucket enforces a strict output rate.

---

### Algorithm Comparison

| Algorithm | Memory | Accuracy | Burst Handling | Complexity | Best For |
|---|---|---|---|---|---|
| Fixed Window | O(1) | Low (boundary issue) | Allows 2× burst at boundaries | Very simple | Internal tools, low-stakes |
| Sliding Window Log | O(n) | Exact | No burst | Medium | Low-volume, high-precision |
| Sliding Window Counter | O(1) | High (estimated) | Controlled | Medium | Production APIs |
| Token Bucket | O(1) | High | Controlled burst | Medium | APIs with legitimate burst patterns |
| Leaky Bucket | O(n queue) | High | No burst | Medium | Smoothing traffic to fixed-rate backends |

---

### Response Headers (Best Practice)

Always return rate limit information in response headers so clients can self-throttle:

```
HTTP/1.1 200 OK
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 34
X-RateLimit-Reset: 1722470460
Retry-After: 47
```

When the limit is exceeded, return **HTTP 429 Too Many Requests**, not 403 or 503.

---

### Interview Language

> "In production, I default to the Sliding Window Counter algorithm because it gives O(1) memory per client regardless of traffic volume, and the weighted estimate is accurate enough that clients cannot meaningfully exploit the approximation. I implement it in Redis with a Lua script to ensure atomic read-modify-write — without atomicity, you get race conditions under concurrent load where multiple requests read the same counter, both see 'under limit,' and both increment, overshoot the limit."

> "The Fixed Window algorithm has a well-known vulnerability: a client can double the allowed rate by sending requests at the end of one window and the start of the next. The Sliding Window eliminates this by decaying the previous window's contribution proportionally to how far into the current window we are."

> "For my HTTP response I always include X-RateLimit-Remaining and X-RateLimit-Reset headers. Well-behaved clients use these to self-throttle. This turns rate limiting from a blunt 'block the bad guys' mechanism into a collaborative capacity-management system."

---

## TOPIC 4: RAG (Retrieval-Augmented Generation) — Complete Guide

### What is RAG and Why Does It Matter?

A large language model (LLM) like GPT-4 or Claude is trained on a snapshot of the internet up to a cutoff date. After that date, it knows nothing. More importantly, it **never knows your private data** — your internal documents, your customer database, your proprietary research.

This creates two critical problems for enterprise AI:

1. **Knowledge staleness:** The model does not know about events, products, or policies created after training
2. **No private knowledge:** The model cannot answer questions about your own documents

**Fine-tuning** can partially address knowledge gaps, but it is expensive (compute cost + time), requires continuous re-training as data changes, and risks **catastrophic forgetting** (the model forgets general capabilities as it over-fits to your data).

**RAG solves this differently:** instead of baking knowledge into the model weights, you retrieve relevant information at query time and inject it into the prompt. The model's knowledge is now a function of what you retrieve, not just what it was trained on.

**Analogy:** RAG is like giving a brilliant but forgetful consultant a search engine and a filing cabinet. Instead of memorizing everything, they look it up when you ask. The consultant (LLM) brings reasoning ability; the filing cabinet (vector store) brings relevant facts.

---

### The Core RAG Pipeline

```
INDEXING PHASE (offline, runs periodically)
============================================

Raw Documents
     │
     ▼
┌─────────────┐
│  Document   │  PDF, Word, HTML, Confluence pages, Slack exports
│   Loader    │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Chunker   │  Split documents into overlapping segments
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Embedding  │  Convert each chunk to a dense vector
│   Model     │  (e.g., text-embedding-3-small, sentence-transformers)
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Vector    │  Store vectors with metadata and original text
│   Store     │  (Pinecone, Weaviate, Chroma, pgvector)
└─────────────┘


QUERY PHASE (online, per user request)
========================================

User Question
     │
     ▼
┌─────────────┐
│   Query     │  Optionally expand/transform the query
│ Transform   │  (HyDE, step-back, multi-query expansion)
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Embed     │  Convert query to same vector space as documents
│   Query     │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Retrieval  │  ANN search: find top-K most similar chunks
│ (Vector DB) │  Optionally combine with BM25 keyword search
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Re-ranker  │  Cross-encoder reranks top-K for relevance
│ (optional)  │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Augmented  │  Inject retrieved chunks into prompt template
│   Prompt    │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│     LLM     │  Generate answer grounded in retrieved context
└──────┬──────┘
       │
       ▼
  Final Response (with source citations)
```

---

### Embeddings: What They Are and Why They Work

An **embedding** is a numeric representation of text as a point in high-dimensional space. The key property: **semantically similar text ends up close together in this space.**

```
"The cat sat on the mat"        → [0.23, -0.81, 0.44, ...]  (384 or 1536 dimensions)
"A feline rested on the rug"    → [0.21, -0.79, 0.41, ...]  (close!)
"The stock market fell today"   → [-0.72, 0.33, -0.55, ...] (far away)
```

Similarity is measured by **cosine similarity** — the cosine of the angle between two vectors. A cosine similarity of 1.0 means identical direction (same meaning); 0.0 means orthogonal (unrelated); -1.0 means opposite.

```python
import numpy as np

def cosine_similarity(vec_a, vec_b):
    return np.dot(vec_a, vec_b) / (np.linalg.norm(vec_a) * np.linalg.norm(vec_b))
```

**Why this matters for RAG:** When a user asks "How do I reset my password?", the embedding of that question will be close to the embedding of a support document titled "Account Recovery Steps" even though the exact words are different. This is **semantic search** — matching by meaning, not keywords.

---

### Vector Databases

A vector database stores embedding vectors and enables fast **Approximate Nearest Neighbor (ANN)** search — finding the K vectors most similar to a query vector out of millions or billions.

**ANN algorithms used internally:**
- **HNSW (Hierarchical Navigable Small World):** Graph-based, excellent speed/recall tradeoff — used by Weaviate, Qdrant
- **IVF (Inverted File Index):** Cluster-based, good for very large datasets — used by Pinecone (under the hood), FAISS
- **Annoy:** Tree-based, memory-efficient — used in older systems

| Vector DB | Hosting | ANN Algorithm | Key Feature |
|---|---|---|---|
| **Pinecone** | Cloud-managed | Custom (IVF-based) | Serverless, auto-scaling, simplest ops |
| **Weaviate** | Self-host / Cloud | HNSW | Built-in hybrid search, GraphQL API |
| **Chroma** | Self-host (local) | HNSW (via hnswlib) | Simplest for prototyping, no infra |
| **Qdrant** | Self-host / Cloud | HNSW | Payload filtering, Rust performance |
| **pgvector** | PostgreSQL extension | IVF / HNSW | Reuse existing Postgres infra, SQL joins |
| **Azure AI Search** | Azure managed | HNSW | Native Azure integration, hybrid search |
| **Redis Vector** | Redis Stack | HNSW / FLAT | Low latency, already in many stacks |

**Choosing a vector database:**
- **Prototype / local development:** Chroma (zero setup)
- **Existing PostgreSQL:** pgvector (no new service to operate)
- **Fully managed, enterprise scale:** Pinecone or Weaviate Cloud
- **Azure shop:** Azure AI Search

---

### Chunking Strategies

Chunking is how you split source documents before embedding. Chunk size is one of the most impactful parameters in RAG — too large and the retrieved context is diluted; too small and you lose surrounding context needed for understanding.

**Strategy 1: Fixed-Size Chunking**

```python
def fixed_chunk(text, chunk_size=512, overlap=64):
    chunks = []
    for i in range(0, len(text), chunk_size - overlap):
        chunks.append(text[i:i + chunk_size])
    return chunks
```

- Simple, fast, predictable
- Ignores document structure — may split a sentence or paragraph mid-thought
- Overlap ensures context continuity across chunk boundaries

**Strategy 2: Recursive Character Splitting**

Split on natural boundaries in priority order: `\n\n` (paragraph) → `\n` (line) → `. ` (sentence) → ` ` (word)

```python
# LangChain's RecursiveCharacterTextSplitter approach
separators = ["\n\n", "\n", ". ", " ", ""]
```

Produces more semantically coherent chunks than fixed-size. This is the most common production default.

**Strategy 3: Semantic Chunking**

Compute embeddings for each sentence, then group consecutive sentences whose embeddings are similar. Split when there is a semantic "jump."

- Most coherent chunks — each chunk covers a single topic
- Expensive: requires embedding every sentence before indexing
- Chunk sizes vary, which complicates token budget management

**Strategy 4: Document-Aware Chunking**

Use document structure: split on Markdown headers (`##`), HTML section tags, PDF page boundaries, or JSON structure.

```python
# For Markdown: split on header boundaries
import re

def markdown_chunks(text):
    return re.split(r'\n(?=#{1,3} )', text)
```

Best for structured documents (wikis, documentation, legal contracts). Preserves section context.

**Chunking Tradeoffs:**

| Strategy | Coherence | Complexity | Variable Size | Best For |
|---|---|---|---|---|
| Fixed-size | Low | Very low | No | Unstructured text, fast indexing |
| Recursive | Medium | Low | Somewhat | General purpose (default) |
| Semantic | High | High | Yes | High-quality retrieval |
| Document-aware | High | Medium | Yes | Structured docs, code, legal |

---

### Retrieval: Dense, Sparse, and Hybrid

**Dense Retrieval (Vector Search)**

Embed the query → find nearest vectors. Excellent for semantic matching. Fails on exact keyword matches (e.g., product codes, error codes, names).

**Sparse Retrieval (BM25 Keyword Search)**

Classic TF-IDF-derived algorithm. Excellent for exact keyword matches. Fails on semantic variations ("password reset" vs "account recovery").

**Hybrid Retrieval (Reciprocal Rank Fusion)**

Run both dense and sparse retrieval, then merge the ranked lists using Reciprocal Rank Fusion (RRF):

```python
def reciprocal_rank_fusion(rankings: list[list], k=60):
    """
    rankings: list of ranked document lists from different retrievers
    k: constant to prevent high impact of top-ranked documents
    """
    scores = {}
    for ranking in rankings:
        for rank, doc_id in enumerate(ranking):
            if doc_id not in scores:
                scores[doc_id] = 0
            scores[doc_id] += 1 / (k + rank + 1)
    return sorted(scores.items(), key=lambda x: x[1], reverse=True)
```

Hybrid retrieval consistently outperforms either alone on diverse query sets. Azure AI Search and Weaviate implement this natively.

---

### Re-Ranking

Initial retrieval returns the top-K most similar vectors, but **similarity is not the same as relevance**. A passage may be semantically similar to your query without actually answering it.

**Cross-encoder re-rankers** take a (query, passage) pair and output a relevance score. Unlike bi-encoders (which embed query and passage independently), cross-encoders see both together and produce much more accurate relevance scores.

```
Initial retrieval: [doc_7, doc_2, doc_15, doc_3, doc_9] (by cosine similarity)

Re-ranker scores:
  doc_15: 0.94  ← actually most relevant
  doc_7:  0.87
  doc_3:  0.72
  doc_2:  0.41
  doc_9:  0.23

After re-ranking: [doc_15, doc_7, doc_3] → sent to LLM
```

Popular re-rankers: Cohere Rerank API, `cross-encoder/ms-marco-MiniLM-L-6-v2` (open source), BAAI/bge-reranker.

**Rule of thumb:** Retrieve top-20 with fast vector search, re-rank to get the best 3-5 for the prompt.

---

### Query Transformation

The user's raw query is often not the best search query. Query transformation improves retrieval quality.

**Multi-Query Expansion**

Generate multiple paraphrases of the user's question, retrieve for each, merge results:

```python
# Using an LLM to expand the query
prompt = f"""Generate 3 different phrasings of this question:
Question: {user_question}
Return as a JSON array."""
```

**HyDE (Hypothetical Document Embeddings)**

Instead of embedding the question, ask the LLM to write a hypothetical answer, then embed that. The embedding of a hypothetical answer is often closer to actual answer documents than the embedding of the question.

```python
# Step 1: Generate hypothetical answer
hypothetical = llm.complete(
    f"Write a brief answer to: {user_question}. Do not say 'I don't know.'"
)

# Step 2: Embed the hypothetical answer
query_vector = embedder.embed(hypothetical.text)

# Step 3: Retrieve using the hypothetical embedding
results = vector_store.search(query_vector, top_k=5)
```

**Step-Back Prompting**

For narrow, specific questions, step back to the general principle before searching:

```
User: "What's the dosage of ibuprofen for a 35-year-old with kidney disease?"
Step-back: "What are ibuprofen dosage guidelines and kidney disease contraindications?"
```

---

### Context Engineering

You have a limited context window (e.g., 128K tokens). How you pack retrieved chunks into the prompt matters:

1. **Put the most relevant chunk first and last** — LLMs suffer from "lost in the middle" attention: they attend well to the start and end of context, less so to the middle.

2. **Include source metadata:** `[Source: Product Manual v2.3, Section 4.2]` — enables citations and helps the model attribute confidence.

3. **Use a prompt template:**

```
SYSTEM: You are a helpful assistant. Answer based ONLY on the provided context.
If the context does not contain enough information, say "I don't have enough information."

CONTEXT:
[Source: Employee Handbook, Section 3.1]
{chunk_1}

[Source: HR Policy 2024, Page 7]
{chunk_2}

USER QUESTION: {question}

ANSWER:
```

4. **Compression:** If chunks are too long, use an LLM to summarize them before insertion.

---

### RAG Evaluation (RAGAS Framework)

| Metric | What It Measures | How |
|---|---|---|
| **Context Recall** | Were the relevant documents retrieved? | Compare retrieved docs to ground truth |
| **Context Precision** | What fraction of retrieved docs are relevant? | Reduces noise injection |
| **Faithfulness** | Does the answer stick to the retrieved context? | LLM-judge checks for hallucination |
| **Answer Relevance** | Does the answer address the actual question? | LLM-judge scores relevance |
| **Answer Correctness** | Is the answer factually correct? | Requires ground truth labels |

```python
# Using RAGAS
from ragas import evaluate
from ragas.metrics import faithfulness, answer_relevancy, context_recall

result = evaluate(
    dataset=eval_dataset,
    metrics=[faithfulness, answer_relevancy, context_recall]
)
print(result)
# {'faithfulness': 0.87, 'answer_relevancy': 0.91, 'context_recall': 0.74}
```

---

### Advanced RAG

**GraphRAG**

Instead of chunking documents into flat segments, extract entities and relationships, build a knowledge graph, and traverse the graph during retrieval. Answers multi-hop questions like "Who is the CEO of the company that acquired Acme Corp in 2023?" that require connecting multiple documents.

**Agentic RAG**

An agent decides retrieval strategy dynamically:
- Should it use vector search, keyword search, or SQL?
- Should it retrieve from one source or multiple?
- Is the current context sufficient or should it retrieve more?
- Should it decompose the question into sub-questions first?

```python
# Simplified agentic RAG
def agentic_retrieve(question: str) -> str:
    plan = agent.plan(question)  # Decide strategy
    
    if plan.needs_database_query:
        db_result = sql_retriever.query(plan.sql_query)
    
    if plan.needs_document_search:
        doc_result = vector_store.search(plan.search_query)
    
    if plan.needs_web_search:
        web_result = web_search(plan.web_query)
    
    return agent.synthesize(question, [db_result, doc_result, web_result])
```

---

### Interview Language

> "RAG is the production answer to the knowledge cutoff problem. Fine-tuning bakes knowledge into weights, which is expensive, slow to update, and risks catastrophic forgetting. RAG injects knowledge at inference time from a living document store. The tradeoff: RAG adds latency (retrieval + context packing), and retrieval quality directly determines answer quality — garbage in, garbage out."

> "In my RAG system, I separate retrieval quality from generation quality. A low faithfulness score tells me the LLM is hallucinating beyond the context — I address that with a stricter prompt. A low context recall score tells me retrieval is missing relevant documents — I address that with better chunking, hybrid search, or query expansion. RAGAS gives me the observability to know which knob to turn."

> "I default to hybrid retrieval — dense vector search combined with BM25, merged via reciprocal rank fusion. Pure vector search misses exact matches like product codes or error messages. Pure keyword search misses semantic variations. Hybrid consistently outperforms either alone and adds minimal latency overhead."

---

## TOPIC 5: 6 Enterprise AI Architecture Patterns

### Why Patterns Matter

Most AI systems in production today are a single Python script and an API call. That works for a proof-of-concept. It does not survive a model deprecation, a vendor switch, a compliance audit, or a 10× traffic spike. Enterprise AI architecture is about designing systems that are swappable, governable, observable, and scalable from the start.

---

### Pattern 1: Agentic Mesh

**What it is:** A decentralized architecture where AI agents operate as peers across the enterprise. No single master agent orchestrates everything. Instead, agents register their capabilities in a shared tool registry, share context through a common layer, and are wrapped with governance at every interaction point.

**Analogy:** Think of a service mesh like Istio for microservices. Istio does not change what your services do — it wraps every service-to-service call with mutual TLS, observability, and circuit breaking without your application code knowing. Agentic Mesh does the same for agent-to-agent calls: governance, identity verification, and audit logging are applied as infrastructure, not sprinkled into each agent's code.

```
┌─────────────────────────────────────────────────────────┐
│                    AGENTIC MESH                          │
│                                                         │
│  ┌───────────┐  ┌───────────┐  ┌───────────────────┐   │
│  │ Customer  │  │  Finance  │  │   HR Onboarding   │   │
│  │  Support  │  │  Analyst  │  │      Agent        │   │
│  │  Agent    │  │  Agent    │  │                   │   │
│  └─────┬─────┘  └─────┬─────┘  └────────┬──────────┘   │
│        │              │                 │               │
│  ╔═════╧══════════════╧═════════════════╧══════╗        │
│  ║           MESH FABRIC                       ║        │
│  ║  • Unified Tool Registry                    ║        │
│  ║  • Shared Context Store                     ║        │
│  ║  • Identity & Auth (agent-to-agent JWT)     ║        │
│  ║  • Governance Interceptors                  ║        │
│  ║  • Distributed Tracing                      ║        │
│  ╚══════════════════════════════════════════════╝        │
│        │              │                 │               │
│  ┌─────▼──────┐ ┌─────▼──────┐ ┌───────▼──────────┐   │
│  │  CRM Tool  │ │ SAP Tool   │ │  HRIS Tool       │   │
│  └────────────┘ └────────────┘ └──────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

**Key Components:**

- **Unified Tool Registry:** Every capability (call a CRM API, query a database, send an email) is registered once and callable by any authorized agent. No duplicated integration code.
- **Shared Context Store:** Agents pass context (conversation history, user identity, task state) through a central store rather than hand-coding point-to-point context passing.
- **Governance Wrapping:** Every agent action is intercepted by a governance layer that checks: Is this agent authorized to call this tool? Does this action violate any policies? Log this action for audit.
- **Decentralized Topology:** No single orchestrator. Agents can call each other peer-to-peer based on capability discovery. This eliminates single points of failure.

**When to use:** Large enterprises with many AI agents across business units that need to share tools, maintain consistent governance, and avoid siloed integrations.

**Tradeoffs:**
- High initial investment to build the mesh fabric
- More complex than centralized orchestration
- Governance consistency is a feature that also creates a governance bottleneck if the interceptor layer is slow

---

### Pattern 2: RAG Pattern (Enterprise Context)

**What it is:** Grounding LLMs in enterprise data without retraining. Already covered deeply in Topic 4. In the enterprise context, the critical additional dimension is **source attribution**.

**The Trust Problem:**

An LLM without citations is unverifiable. An enterprise user asking "What is our return policy?" cannot act on the answer unless they can see *which document* the answer came from, *which version* of that document, and *when* it was last updated.

```
Without RAG + Citations:
  "Your return policy allows 30 days..."
  → User cannot verify. Legal cannot audit. Compliance cannot approve.

With RAG + Citations:
  "Your return policy allows 30 days... [Source: Customer Policy v3.2, last updated
   2024-11-15, approved by Legal on 2024-11-10]"
  → Verifiable. Auditable. Trustworthy.
```

**Enterprise-specific considerations:**
- **Access control on retrieval:** A customer support agent should not retrieve documents marked "Internal Confidential." The vector store must enforce document-level permissions.
- **Data freshness:** Index documents on a schedule that matches their update frequency. A policy document updated daily needs daily re-indexing.
- **Regulatory compliance:** In finance and healthcare, the citation chain must be unbroken from answer back to source document for audit purposes.

---

### Pattern 3: LLM Router

**What it is:** An intelligent routing layer that directs queries to the appropriate model based on complexity, cost, latency requirements, and task type.

**The core insight:** Not every query needs GPT-4/Claude Opus. Classifying an email as spam or extracting a date from text is a simple task — a small, cheap, fast model handles it equally well. Routing every query to the most expensive model is like hiring a neurosurgeon to put on a bandage.

```
                         ┌─────────────────────┐
   User Query ──────────>│    LLM ROUTER       │
                         │  (Classification)   │
                         └──────────┬──────────┘
                                    │
               ┌────────────────────┼────────────────────┐
               │                    │                    │
               ▼                    ▼                    ▼
    ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
    │  Small / Local   │ │   Mid-Tier API   │ │  Frontier Model  │
    │  (Llama 3 8B,    │ │  (GPT-4o-mini,  │ │  (GPT-4o,        │
    │   Phi-3)         │ │   Claude Haiku)  │ │   Claude Opus)   │
    │                  │ │                  │ │                  │
    │  Tasks:          │ │  Tasks:          │ │  Tasks:          │
    │  • Classification│ │  • Summarization │ │  • Complex code  │
    │  • Data extract  │ │  • Standard QA   │ │  • Multi-step    │
    │  • Formatting    │ │  • Draft emails  │ │    reasoning     │
    │  • Simple lookup │ │  • Translation   │ │  • Legal review  │
    │                  │ │                  │ │  • Strategy      │
    │  Cost: $0.0001/K │ │  Cost: $0.01/K  │ │  Cost: $0.15/K  │
    └──────────────────┘ └──────────────────┘ └──────────────────┘
               │                    │                    │
               └────────────────────┼────────────────────┘
                                    │
                         ┌──────────▼──────────┐
                         │    Fallback Path     │
                         │  (retry on failure)  │
                         └─────────────────────┘
```

**Router Decision Tree:**

```
Query arrives
    │
    ├─ Contains: code generation, multi-document synthesis,
    │  legal/medical reasoning, >3-step planning
    │  → Frontier Model
    │
    ├─ Contains: summarization, Q&A, email draft, translation,
    │  standard reasoning
    │  → Mid-tier Model
    │
    └─ Contains: classification, extraction, formatting, lookup
       → Small/Local Model
```

**Cost Example:** At 1 million requests/day with an average query routed to mid-tier after the router:
- All frontier: 1M × $0.15/1K tokens × 500 tokens avg = **$75,000/day**
- Routed (70% small, 20% mid, 10% frontier): $700 + $1,000 + $7,500 = **$9,200/day**
- Savings: ~88%

**Tradeoffs:**
- The router itself is an LLM call (use a very small, cheap model or rule-based classifier)
- Misclassification sends complex queries to cheap models → poor quality. Need eval loop.
- Router adds latency (~50-100ms for classification)

---

### Pattern 4: Hub-and-Spoke

**What it is:** A central AI platform (the hub) provides shared infrastructure — models, guardrails, observability, cost management — while business units (spokes) build domain-specific applications on top of platform standards.

**Analogy:** The enterprise API platform strategy. A central API gateway team provides authentication, rate limiting, and monitoring. Business teams build APIs without reimplementing those cross-cutting concerns. Hub-and-Spoke for AI does the same: business teams focus on domain logic, not on model deployment, prompt safety, or cost tracking.

```
                    ┌──────────────────────────────────────┐
                    │          CENTER OF EXCELLENCE        │
                    │   Policy · Audit · Upskilling        │
                    └───────────────┬──────────────────────┘
                                    │ Sets policy
                                    ▼
┌───────────────────────────────────────────────────────────────┐
│                         HUB (Central AI Platform)             │
│                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  Model       │  │  Guardrails  │  │  Observability   │   │
│  │  Registry    │  │  Engine      │  │  (Traces, Evals) │   │
│  │  (GPT-4o,    │  │  (safety,    │  │                  │   │
│  │   Claude,    │  │   PII filter,│  │                  │   │
│  │   Llama)     │  │   content    │  │                  │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  Prompt      │  │  FinOps      │  │  Shared APIs     │   │
│  │  Library     │  │  (cost       │  │  & SDKs          │   │
│  │              │  │   tracking)  │  │                  │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└────────────┬──────────────┬─────────────────┬────────────────┘
             │              │                 │
      ┌──────▼─────┐ ┌──────▼─────┐  ┌───────▼────────┐
      │  Finance   │ │   HR BU    │  │  Customer Svc  │
      │    BU      │ │  Spoke     │  │     Spoke      │
      │  Spoke     │ │            │  │                │
      │            │ │  Domain    │  │  Domain Logic  │
      │  Domain    │ │  Logic +   │  │  + Uses hub    │
      │  Logic +   │ │  Uses hub  │  │  services      │
      │  Uses hub  │ │  services  │  │                │
      └────────────┘ └────────────┘  └────────────────┘
```

**Center of Excellence (CoE) responsibilities:**
- Define which models are approved for which data classifications
- Set prompt safety standards and review templates
- Run evals framework and set minimum quality bars
- Monitor overall AI spend and allocate costs to business units
- Upskill teams through training and internal tooling

**When to use:** Large organizations where multiple business units want to build AI features but centralized governance is required for compliance, security, or cost control.

**Tradeoffs:**
- Hub becomes a bottleneck if it blocks spoke teams from moving fast
- Success requires the hub to be a *platform enabler*, not a gatekeeper
- Initial cost to build the hub is high; ROI comes from scale across spokes

---

### Pattern 5: Event-Driven Agents

**What it is:** Agents communicate exclusively through events on a shared event bus. No agent calls another agent directly. Agents publish events ("I completed this task") and subscribe to events ("When this thing happens, I should act").

**Analogy:** Traditional microservices event-driven architecture — a completed payment event triggers a fulfillment service, which triggers a notification service, which triggers an analytics update. No service calls another directly; they all speak through the event bus. Event-Driven Agents applies the same pattern to AI agent workflows.

```
┌─────────────────────────────────────────────────────────┐
│                    EVENT BUS (Kafka / SNS+SQS)           │
│                                                         │
│  Events flow: customer.query.received →                 │
│               query.classified →                       │
│               response.draft.created →                 │
│               response.reviewed →                      │
│               customer.response.sent                   │
└──────────────────────────────────────────────────────────┘
         ▲              ▲              ▲              ▲
         │              │              │              │
┌────────┴────┐  ┌──────┴──────┐ ┌────┴──────┐ ┌────┴──────┐
│  Intake     │  │ Classifier  │ │  Drafter  │ │ Reviewer  │
│  Agent      │  │   Agent     │ │   Agent   │ │   Agent   │
│             │  │             │ │           │ │           │
│ Publishes:  │  │ Subscribes: │ │ Subscribes│ │ Subscribes│
│ customer.   │  │ customer.   │ │ query.    │ │ response. │
│ query.      │  │ query.      │ │ classified│ │ draft.    │
│ received    │  │ received    │ │           │ │ created   │
└─────────────┘  └─────────────┘ └───────────┘ └───────────┘
```

**Key properties:**

- **Decoupling:** Adding a new agent (e.g., a translation agent) only requires subscribing it to the right events — no existing agent's code changes
- **Replay:** Every event is persisted in the event store. You can replay a specific customer interaction from the beginning for debugging or reprocessing
- **Audit trail:** The full sequence of agent actions is captured as an immutable log of events
- **Resilience:** If the Drafter Agent goes down, events queue in the bus and are processed when it recovers — no lost data

**Event schema best practice:**

```json
{
  "eventId": "evt_01HXYZ789",
  "eventType": "query.classified",
  "timestamp": "2026-07-31T09:14:33Z",
  "version": "1.0",
  "source": "classifier-agent-v2",
  "correlationId": "req_01HXYZ000",
  "payload": {
    "queryText": "What's my order status?",
    "category": "order_tracking",
    "confidence": 0.97
  }
}
```

**Tradeoffs:**
- Eventual consistency: agents do not get a synchronous response; they must wait for a downstream event
- Debugging is harder: a failure in step 4 requires tracing through 4 event types
- Event schema versioning is critical — breaking schema changes break all subscribers

---

### Pattern 6: Hexagonal Architecture for AI

**What it is:** Also known as the "Ports and Adapters" pattern, applied to AI systems. The core application logic (what the system *does*) is separated from the infrastructure it *uses* (which LLM, which vector store, which tool APIs) through clearly defined ports (interfaces) and adapters (implementations).

**The 2026 context:** In AI, models deprecate. Vendors change pricing. A vector database startup gets acquired. If your business logic is tightly coupled to "GPT-4o" and "Pinecone", swapping either requires a rewrite. Hexagonal architecture makes swaps a one-adapter change.

```
                    ┌─────────────────────────────┐
                    │                             │
  ┌──────────────┐  │     CORE APPLICATION        │  ┌──────────────┐
  │  UI Adapter  │◄─►  LOGIC                     ◄─► │ LLM Adapter  │
  │  (REST API,  │  │  ("Analyze this contract    │  │ (GPT-4o,     │
  │   Slack,     │  │   and extract key terms")   │  │  Claude,     │
  │   CLI)       │  │                             │  │  Llama)      │
  └──────────────┘  │  ┌─────────────────────┐   │  └──────────────┘
                    │  │   Domain Interfaces  │   │
  ┌──────────────┐  │  │  (Ports)            │   │  ┌──────────────┐
  │  Tool Adapter│◄─►  │  ILanguageModel     ◄─►  │ Vector Adapter│
  │  (Search,    │  │  │  IVectorStore       │   │  │ (Pinecone,   │
  │   Email,     │  │  │  IToolExecutor      │   │  │  pgvector,   │
  │   Calendar)  │  │  │  IUIPort            │   │  │  Weaviate)   │
  └──────────────┘  │  └─────────────────────┘   │  └──────────────┘
                    │                             │
                    └─────────────────────────────┘
```

**What swapping looks like in practice:**

```python
# The port (interface) — core logic only knows this
class ILanguageModel:
    def complete(self, prompt: str, max_tokens: int) -> str:
        raise NotImplementedError

# Adapter 1: OpenAI
class OpenAIAdapter(ILanguageModel):
    def __init__(self, model="gpt-4o"):
        self.client = openai.OpenAI()
        self.model = model
    
    def complete(self, prompt: str, max_tokens: int) -> str:
        response = self.client.chat.completions.create(
            model=self.model,
            messages=[{"role": "user", "content": prompt}],
            max_tokens=max_tokens
        )
        return response.choices[0].message.content

# Adapter 2: Anthropic — swap without touching core logic
class AnthropicAdapter(ILanguageModel):
    def __init__(self, model="claude-opus-4-5"):
        self.client = anthropic.Anthropic()
        self.model = model
    
    def complete(self, prompt: str, max_tokens: int) -> str:
        response = self.client.messages.create(
            model=self.model,
            max_tokens=max_tokens,
            messages=[{"role": "user", "content": prompt}]
        )
        return response.content[0].text

# Core application — never references OpenAI or Anthropic directly
class ContractAnalyzer:
    def __init__(self, llm: ILanguageModel, vector_store: IVectorStore):
        self.llm = llm
        self.vector_store = vector_store
    
    def extract_key_terms(self, contract_text: str) -> list[str]:
        # Core logic unchanged regardless of which LLM is injected
        prompt = f"Extract key legal terms from:\n{contract_text}"
        return self.llm.complete(prompt, max_tokens=500)
```

**Why this matters in 2026:**
- GPT-3.5 was deprecated in early 2025 with 3 months' notice. Teams without port abstractions scrambled.
- Vector database pricing changes monthly — lock-in is expensive
- Regulatory environments (EU AI Act) may require switching to an on-premise model for certain data — you need a clean swap path

**Ports to define in any AI system:**
- `ILanguageModel` — LLM inference
- `IVectorStore` — similarity search
- `IEmbeddingModel` — text-to-vector conversion
- `IToolExecutor` — external tool calls
- `IUIPort` — user interface (REST, WebSocket, CLI)
- `IObservabilityPort` — logging and tracing

---

### Interview Language (Patterns Section)

> "For an enterprise with 20+ AI use cases across business units, I would recommend Hub-and-Spoke. Each business unit moves fast using hub services — no one re-implements PII filtering or model cost tracking. The Center of Excellence sets policy once and it applies everywhere. The risk is that the hub becomes a bottleneck, so the platform team must operate it like a product team: dedicated SRE, public roadmap, SLA guarantees to spoke teams."

> "I apply Hexagonal Architecture to every AI system I build. In 2026, the model landscape changes every six months. If your core business logic contains hardcoded references to a specific model or vendor SDK, you are one deprecation notice away from an unplanned rewrite. Ports and Adapters costs maybe 2 extra hours of initial design and saves weeks of emergency refactoring."

> "For an event-driven agent workflow, my first question is: what is the schema contract between agents? Agents communicate exclusively through events, so the event schema is the API contract. I version schemas from day one — any breaking change gets a new version, old subscribers continue on v1, new subscribers use v2. Without that discipline, a single schema change breaks the entire pipeline."

---

## TOPIC 6: Event Sourcing — Deep Dive

### What is Event Sourcing?

In a traditional CRUD system, the database stores **current state**. When a customer changes their address, you UPDATE the row and the old address is gone.

In an event-sourced system, the database stores **events** — facts about things that happened. The current state is not stored directly; it is **derived by replaying events** from the beginning.

```
Traditional CRUD (stores current state):
┌──────────────────────────────────────────────┐
│ orders table                                 │
├────────┬──────────┬──────────────────────────┤
│ id     │ status   │ total                    │
├────────┼──────────┼──────────────────────────┤
│ 1001   │ shipped  │ 149.99                   │
└────────┴──────────┴──────────────────────────┘
(History is gone. Why was it shipped? When? By whom?)


Event Sourcing (stores events):
┌──────────────────────────────────────────────────────────────┐
│ order_events table                                           │
├──────┬────────────────────┬──────────────────┬──────────────┤
│ seq  │ event_type         │ timestamp        │ data         │
├──────┼────────────────────┼──────────────────┼──────────────┤
│ 1    │ OrderPlaced        │ 2026-07-30 09:00 │ {items: ...} │
│ 2    │ PaymentProcessed   │ 2026-07-30 09:01 │ {amount: ..} │
│ 3    │ WarehousePicked    │ 2026-07-30 14:00 │ {picker: ..} │
│ 4    │ OrderShipped       │ 2026-07-31 08:00 │ {carrier: .} │
└──────┴────────────────────┴──────────────────┴──────────────┘
(Full audit trail. Replay events to get current state at any point in time.)
```

---

### Reconstructing State by Replaying Events

The current state of an aggregate (e.g., Order #1001) is computed by starting from an empty state and applying each event in sequence:

```python
class Order:
    def __init__(self):
        self.id = None
        self.status = None
        self.items = []
        self.total = 0.0
        self.payment_id = None

    def apply(self, event: dict) -> 'Order':
        """Apply a single event to evolve state."""
        event_type = event['event_type']
        data = event['data']
        
        if event_type == 'OrderPlaced':
            self.id = data['order_id']
            self.items = data['items']
            self.total = data['total']
            self.status = 'placed'
        
        elif event_type == 'PaymentProcessed':
            self.payment_id = data['payment_id']
            self.status = 'paid'
        
        elif event_type == 'WarehousePicked':
            self.status = 'picked'
        
        elif event_type == 'OrderShipped':
            self.tracking_number = data['tracking_number']
            self.status = 'shipped'
        
        return self

def load_order(order_id: str, event_store) -> Order:
    events = event_store.get_events(aggregate_id=order_id)
    order = Order()
    for event in events:
        order.apply(event)
    return order
```

**The power of this model:** You can reconstruct the state of Order #1001 *at any point in time* by replaying events up to that timestamp. "What did this order look like at 10:00 AM on July 30?" is a query, not a mystery.

---

### Snapshots: Avoiding Full Replay from the Beginning

If an aggregate has thousands of events (a bank account with 10 years of transactions), replaying all of them on every read is expensive.

**Snapshots** are periodic captures of computed state. Instead of replaying from event #1, you load the most recent snapshot and replay only the events since then.

```
Event log:
[1-500] → Snapshot at event 500 (persisted state at that point)
[501-1000] → Snapshot at event 1000
[1001-1047] → No snapshot yet

To load current state:
1. Load snapshot at event 1000 (fast: single row lookup)
2. Replay events 1001-1047 (cheap: only 47 events)
3. Apply → current state
```

```python
def load_order_with_snapshot(order_id: str, event_store, snapshot_store) -> Order:
    # Try to load the latest snapshot
    snapshot = snapshot_store.get_latest_snapshot(order_id)
    
    if snapshot:
        order = snapshot.state          # Deserialize snapshot
        from_sequence = snapshot.seq    # Resume from here
    else:
        order = Order()
        from_sequence = 0
    
    # Load only events since snapshot
    events = event_store.get_events(
        aggregate_id=order_id,
        after_sequence=from_sequence
    )
    for event in events:
        order.apply(event)
    
    return order
```

---

### CQRS + Event Sourcing: Why They Pair Naturally

**CQRS (Command Query Responsibility Segregation)** separates write operations (commands) from read operations (queries) into different models. Event Sourcing and CQRS fit together like a hand in a glove:

- **Command side** receives commands (PlaceOrder, ProcessPayment), validates them, emits events to the event store
- **Event store** is the single source of truth
- **Projections** (read models) consume events asynchronously and build optimized query models

```
                    WRITE SIDE                READ SIDE
                    ──────────                ──────────
User Action
     │
     ▼
Command Handler
(validate, execute)
     │
     ▼
Event Store ──────── event stream ──────► Projection 1: Order Summary View
(append-only)                             (optimized for: "list all orders")
                               │
                               └────────► Projection 2: Analytics View
                                          (optimized for: "orders by region")
                               │
                               └────────► Projection 3: Fulfillment View
                                          (optimized for: "orders ready to ship")
```

Each projection is a separate read model, optimized for its specific query pattern. You are not constrained to one database schema for all queries. The same event stream can power a relational database view, an Elasticsearch index, and a Redis cache simultaneously.

---

### Event Store vs Regular Database

| Property | Event Store | Regular DB |
|---|---|---|
| **Write pattern** | Append-only | Read-write (CRUD) |
| **History** | Full history preserved | Only current state |
| **Time travel** | Yes (replay to any point) | No |
| **Audit trail** | Native | Requires audit triggers |
| **Read pattern** | Sequential scan for replay | Indexed reads |
| **Storage growth** | Grows forever | Can be pruned/archived |
| **Schema evolution** | Events are versioned | Schema migrations |
| **Examples** | EventStoreDB, Apache Kafka, custom | PostgreSQL, MySQL, MongoDB |

**EventStoreDB** is purpose-built: optimized for append, ordered reads within a stream, and subscriptions (push new events to consumers in real-time).

**Apache Kafka** is often used as an event store in practice: durable, ordered within a partition, supports replay with consumer offsets.

---

### Projections: Building Read Models

A **projection** is a function that transforms an event stream into a specific read model. Projections can be:

**Synchronous (inline):** Computed on the write path. Fast reads, but couples read and write performance.

**Asynchronous (eventual consistency):** A background process consumes events and updates the read model. Higher read performance, but the read model may lag behind.

```python
class OrderSummaryProjection:
    """Builds a flat 'order summaries' table for fast listing."""
    
    def __init__(self, db):
        self.db = db
    
    def handle(self, event: dict):
        event_type = event['event_type']
        data = event['data']
        
        if event_type == 'OrderPlaced':
            self.db.execute("""
                INSERT INTO order_summaries (id, status, total, placed_at)
                VALUES (?, 'placed', ?, ?)
            """, [data['order_id'], data['total'], event['timestamp']])
        
        elif event_type == 'OrderShipped':
            self.db.execute("""
                UPDATE order_summaries
                SET status = 'shipped', shipped_at = ?
                WHERE id = ?
            """, [event['timestamp'], event['aggregate_id']])
```

**Rebuilding projections:** Because events are immutable and complete, you can always **drop and rebuild** a projection. This is enormously powerful:

- Add a new query requirement → write a new projection that processes the existing event history
- Fix a bug in a projection → rebuild it from scratch with the corrected logic
- No data migration needed: the events are the data

---

### Challenges

**1. Eventual Consistency**

The read model may lag behind the event store by milliseconds to seconds. A user places an order and immediately navigates to "My Orders" — the order might not appear yet.

**Mitigation strategies:**
- Return the projected state optimistically from the command response
- Use read-your-writes consistency: the user always reads from the write model for their own actions
- Accept that eventual consistency is a feature: the user's order will appear shortly

**2. Event Schema Evolution (Versioning)**

An OrderPlaced event from 2023 may not have a `coupon_code` field that was added in 2024. Projections must handle missing fields gracefully.

**Patterns for schema evolution:**
- **Upcasting:** When loading old events, transform them to the latest schema version before applying
- **Weak schema:** Use optional fields and default values; projections tolerate missing fields
- **Versioned event types:** `OrderPlaced_v1`, `OrderPlaced_v2` — projections handle each version explicitly

```python
def upcast_event(event: dict) -> dict:
    """Upgrade old events to current schema."""
    if event['event_type'] == 'OrderPlaced' and 'coupon_code' not in event['data']:
        event['data']['coupon_code'] = None  # Default for old events
    return event
```

---

### Real-World Examples

**Banking Ledger**

A bank account balance is never "stored" — it is the sum of all debit and credit events. Every transaction is an immutable event. Regulators can audit every cent. This is event sourcing by nature, not by design.

```
Deposit:   +$500  → event: MoneyDeposited    { amount: 500.00 }
Withdrawal: -$200  → event: MoneyWithdrawn   { amount: 200.00 }
Transfer:  -$100  → event: MoneyTransferred  { amount: 100.00, to: "ACC-456" }
Balance = replay and sum = $200
```

**E-Commerce Order History**

An order is a sequence of state transitions, each meaningful: Placed → Paid → Fulfilling → Shipped → Delivered → Returned. Every transition is an event with a timestamp and actor. Customer service can reconstruct exactly what happened, when, and why.

**Git**

Git is arguably the most widely-used event store in existence. Every commit is an immutable event. A file's current content is computed by replaying commits from the initial commit. You can check out any past state. Branches are named pointers into the event stream.

```
git log  →  event stream (newest first)
git checkout abc123  →  replay events up to commit abc123
git blame  →  attribute each line to the event that last modified it
```

---

### Event Sourcing Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│  USER / API                                                 │
│  Sends Commands:  PlaceOrder, ProcessPayment, ShipOrder     │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────┐
│  COMMAND HANDLERS                                            │
│  Validate command → Load aggregate (from event store)       │
│  → Apply business rules → Emit events                       │
└──────────────────────────┬──────────────────────────────────┘
                           │  append events
                           ▼
┌──────────────────────────────────────────────────────────────┐
│  EVENT STORE (append-only, immutable)                       │
│  OrderPlaced | PaymentProcessed | OrderShipped | ...        │
└──────────┬───────────────────────┬──────────────────────────┘
           │ stream                │ stream
           ▼                       ▼
┌────────────────────┐  ┌───────────────────────────────────┐
│  SNAPSHOT STORE    │  │  PROJECTIONS (read models)        │
│  (periodic state   │  │  Order Summary | Analytics |      │
│   snapshots)       │  │  Fulfillment View | Search Index  │
└────────────────────┘  └───────────────────────────────────┘
```

---

### Interview Language

> "Event sourcing stores the sequence of events that happened, not the current state. Current state is derived by replaying events. This gives you a perfect audit trail, the ability to reconstruct state at any point in time, and the ability to build new read models by replaying the existing event history — without any data migration."

> "The most common pushback on event sourcing is eventual consistency. My response: the question is not 'is this system eventually consistent' but 'what is the acceptable lag and how do we handle the UX during it?' A 100ms lag on a read model is invisible to users. The projection can also include the command's result synchronously in the response so the user sees their own action immediately."

> "I pair event sourcing with CQRS because the write model and the read model have fundamentally different requirements. The write model cares about correctness and consistency. The read model cares about query performance. Forcing both through the same schema is the relational anti-pattern. Event Sourcing gives you one write model (the event stream) and unlimited read models (projections) optimized for specific query patterns."

> "Git is my go-to analogy. Every developer uses event sourcing daily without thinking about it. Git stores commits (events), not file snapshots. The current state of a file is computed by replaying commits. You can check out any past state. That's exactly what event sourcing does for business data."

---

## Quick Reference: Interview Cheat Sheet

### One-Sentence Definitions

| Topic | One-Sentence Definition |
|---|---|
| HTTP QUERY | A safe, idempotent HTTP verb that accepts a request body, solving GET's URL-length limit for complex searches without misusing POST |
| Short Polling | Client repeatedly asks the server on a timer; simple but wastes bandwidth on empty responses |
| SSE | One-directional persistent HTTP stream from server to client with native browser auto-reconnect |
| WebSockets | Full-duplex bidirectional TCP connection for true real-time two-way communication |
| Sliding Window Counter | Rate limiting algorithm using weighted average of current + previous window count; O(1) memory |
| RAG | Injecting retrieved documents into LLM prompts at query time, grounding responses in private/fresh data |
| Agentic Mesh | Decentralized multi-agent architecture where governance wraps every action via a shared fabric |
| LLM Router | Routing queries to models matched by complexity/cost; cheap models for simple tasks, frontier for complex |
| Hub-and-Spoke | Central AI platform (governance, models, observability) with business units building on platform standards |
| Event-Driven Agents | Agents communicate exclusively via events on a bus; decoupled, replayable, auditable |
| Hexagonal Architecture | Core AI logic separated from infrastructure via ports/adapters, making models and vendors swappable |
| Event Sourcing | Storing immutable events instead of current state; current state derived by replaying the event sequence |
| CQRS | Separate write models (commands) and read models (queries) optimized independently |

---

*End of DOC_1.1-Complete.md*
