# UI & Web Architecture — Complete Reference Guide

> Consolidates: Web Rendering Strategies · Micro-frontends · Performance Optimization · Frontend Architecture Patterns · PWA · Headless · EDA · CQRS · BFF · TypeScript · SOLID · Design Patterns · RADIO Framework

---

## Table of Contents

1. [Web Rendering Strategies](#1-web-rendering-strategies)
2. [Micro-frontends & Module Federation](#2-micro-frontends--module-federation)
3. [React Performance Optimization](#3-react-performance-optimization)
4. [Pagination & Rate Limiting](#4-pagination--rate-limiting)
5. [Communication Protocols](#5-communication-protocols)
6. [Frontend Architecture Patterns](#6-frontend-architecture-patterns)
7. [Thick vs Thin Clients](#7-thick-vs-thin-clients)
8. [BFF (Backend for Frontend) Pattern](#8-bff-backend-for-frontend-pattern)
9. [Progressive Web Apps (PWA)](#9-progressive-web-apps-pwa)
10. [Headless Architecture](#10-headless-architecture)
11. [Event-Driven Architecture (EDA)](#11-event-driven-architecture-eda)
12. [CQRS & Event Sourcing](#12-cqrs--event-sourcing)
13. [GoF Design Patterns for Frontend](#13-gof-design-patterns-for-frontend)
14. [SOLID Principles for Web](#14-solid-principles-for-web)
15. [TypeScript Advanced Concepts](#15-typescript-advanced-concepts)
16. [RADIO Interview Framework](#16-radio-interview-framework)
17. [Core Web Vitals & Accessibility](#17-core-web-vitals--accessibility)

---

## 1. Web Rendering Strategies

### Overview
Rendering strategy defines where and when HTML is generated — the decision has major implications for SEO, Time to First Byte (TTFB), interactivity, and server cost.

### Rendering Strategy Taxonomy

```mermaid
flowchart TD
    Q(["When/Where is HTML generated?"]) --> TYPES

    TYPES --> CSR["CSR — Client-Side Rendering\nBrowser downloads empty HTML + JS bundle\nJS runs in browser, fetches data, builds DOM\nFrameworks: React SPA, Vue SPA, Angular SPA\n+++ Rich interactivity, no server reload\n--- Poor SEO (bot may not wait for JS)\n--- Slow initial load (large bundle)\nBest for: Dashboards, admin panels, B2B apps"]

    TYPES --> SSR["SSR — Server-Side Rendering\nServer generates full HTML per request\nBrowser receives ready-to-display HTML\nFrameworks: Next.js, Nuxt.js, Remix\n+++ Excellent SEO, fast TTFB\n--- Server CPU per request\n--- Flash on navigation (full page fetch)\nBest for: Marketing sites, e-commerce product pages"]

    TYPES --> SSG["SSG — Static Site Generation\nHTML generated at BUILD TIME, not request time\nServed from CDN — no server needed at runtime\nFrameworks: Next.js, Gatsby, Astro\n+++ Fastest load (pre-built, CDN-cached)\n--- Stale data (rebuild needed for updates)\nBest for: Blogs, documentation, landing pages"]

    TYPES --> ISR["ISR — Incremental Static Regeneration\nSSG pages revalidated after a TTL expires\nFirst stale request triggers background rebuild\nNext request gets fresh page\nFrameworks: Next.js (revalidate: 60)\n+++ Near-static speed + fresh content\n--- Short window of stale data\nBest for: News sites, product listings, pricing pages"]

    TYPES --> PPR["PPR — Partial Pre-Rendering (Next.js 15)\nStatic shell rendered at build time (CDN-cached)\nDynamic holes filled by Suspense streaming\nCombines SSG speed with SSR dynamism\nFrameworks: Next.js 15+ (experimental)\n+++ Best of both worlds\n--- Very new, limited framework support\nBest for: Product pages with dynamic recommendations"]

    style CSR fill:#f59e0b,color:#fff
    style SSR fill:#0078D4,color:#fff
    style SSG fill:#22c55e,color:#fff
    style ISR fill:#8b5cf6,color:#fff
    style PPR fill:#22c55e,color:#fff
```

### Rendering Performance Comparison

| Strategy | TTFB | FCP | SEO | Hosting cost | Stale risk |
|---|---|---|---|---|---|
| CSR | Fast | Slow (bundle download + JS run) | Poor | Low (static files) | None |
| SSR | Medium | Fast | Excellent | High (server per req) | None |
| SSG | Fast (CDN) | Fast (pre-built) | Excellent | Very low (CDN) | High (rebuild required) |
| ISR | Fast (CDN) | Fast | Excellent | Low | Low (TTL-bounded) |
| PPR | Fast (static shell) | Fast (streaming) | Excellent | Low | Very low |

### Interview Talking Points

| Question | Answer |
|---|---|
| Why does CSR hurt SEO? | Search engine crawlers may not execute JavaScript or may time out before JS builds the DOM. The crawler sees an empty `<div id="root">` instead of real content. SSR or SSG sends complete HTML immediately. |
| When would you choose SSG over SSR? | When content doesn't change per user or per minute. A marketing page, blog post, or documentation site is identical for all users — build it once, cache it at the edge forever. SSR for the same content wastes server CPU on identical work. |
| What problem does ISR solve? | SSG gets stale when data changes. ISR adds a TTL so that after N seconds, the next request triggers a background regeneration — the stale page still serves immediately (no user waits for rebuild), and fresh content is ready on the next subsequent request. |

---

## 2. Micro-frontends & Module Federation

### Overview
Micro-frontends apply microservices principles to the frontend — splitting a monolithic frontend into independently deployable UI applications owned by different teams.

### Micro-frontend Architecture

```mermaid
flowchart TD
    SHELL["Shell / Container App\n(App host — bootstraps the MFE runtime)\nOwns global nav, auth, routing\nDeploys independently"]

    SHELL --> CATALOG["Catalog MFE\n(Team A)\nProduct browsing + search\nReact 18 + TypeScript\nDeployed on its own schedule"]

    SHELL --> CART["Cart MFE\n(Team B)\nShopping cart + checkout\nReact 18 + TypeScript\nOwns /cart and /checkout routes"]

    SHELL --> AUTH_MFE["Auth MFE\n(Team C)\nLogin, Register, Profile\nAngular 17\nExposes LoginWidget component"]

    SHELL --> SHARED["Shared Lib\n(Design System + Utils)\nButton, Typography, Icons\nPublished to NPM or CDN\nVersioned explicitly"]

    style SHELL fill:#0f172a,color:#fff
    style CATALOG fill:#0078D4,color:#fff
    style CART fill:#22c55e,color:#fff
    style AUTH_MFE fill:#8b5cf6,color:#fff
    style SHARED fill:#f59e0b,color:#fff
```

### Module Federation (Webpack 5)

```mermaid
flowchart LR
    subgraph HOST ["Host App (Shell)"]
        IMPORT["import('cartApp/Cart')\nDynamic remote import at runtime\nNo rebuild of host needed\nwhen remote updates"]
    end

    subgraph REMOTE ["Remote App (Cart MFE)"]
        EXPOSE["exposes: { './Cart': CartComponent }\nPublishes a component\nas a remote module\n\nshared: ['react', 'react-dom']\nSingleton shared deps\navoid duplicate React instances"]
    end

    HOST -->|"Fetches remoteEntry.js at runtime"| REMOTE

    style HOST fill:#0078D4,color:#fff
    style REMOTE fill:#22c55e,color:#fff
```

### MFE Communication Patterns

| Method | How | Best For |
|---|---|---|
| **Custom Events** | `document.dispatchEvent(new CustomEvent('cart:updated', {detail}))` | Decoupled cross-MFE events |
| **Shared State** | Redux store in shared lib + MFEs subscribe | Complex shared state (prefer to minimize) |
| **URL / Query Params** | Each MFE reads URL; navigation triggers updates | Deep-linkable state, shallow integration |
| **Props / Callbacks** | Shell passes down to MFE components | Simple parent→child data passing |
| **Event Bus** | Pub/sub singleton (`mitt`, `RxJS Subject`) | Type-safe event routing |

### MFE Trade-offs

| Pro | Con |
|---|---|
| Teams deploy independently — no release train | Multiple React instances if shared deps not configured correctly |
| Each team owns their tech stack | Consistent UX harder across teams (use design system) |
| Fault isolation — one MFE crash doesn't kill shell | Higher complexity: CI/CD, versioning, contract testing |
| Scale teams horizontally | Increased initial bundle load (multiple entry points) |

---

## 3. React Performance Optimization

### Re-render Prevention

```mermaid
flowchart TD
    RERENDER(["Unnecessary Re-render Problem"]) --> Q{"What to use?"}

    Q -->|"Memoize component (skip re-render\nwhen props unchanged)"| MEMO["React.memo()\nWraps a function component\nShallow-compares props\nSkip re-render if props same"]

    Q -->|"Memoize a function reference\n(stable ref between renders)"| UCB["useCallback(fn, [deps])\nReturns same fn reference\nuntil deps change\nPrevents child re-renders\nfrom inline fn prop"]

    Q -->|"Memoize a computed value\n(expensive calculation)"| UM["useMemo(() => expensive(), [deps])\nRe-computes only when deps change\nDon't use for simple values"]

    style MEMO fill:#0078D4,color:#fff
    style UCB fill:#22c55e,color:#fff
    style UM fill:#8b5cf6,color:#fff
    style RERENDER fill:#ef4444,color:#fff
```

### Code Splitting & Lazy Loading

```mermaid
flowchart LR
    INITIAL["Initial Page Load"] --> SPLIT{"Without Code Split"}
    SPLIT -->|"Download"| GIANT["Single 5MB bundle\n(all routes, all components)\nUser waits for ALL code\nbefore seeing anything"]

    INITIAL --> LAZY{"With Lazy Loading"}
    LAZY -->|"Download"| SMALL["Small entry bundle\n(just what's needed now)"]
    SMALL -->|"User navigates to /admin"| CHUNK["Admin chunk downloaded\nonly when needed"]

    style GIANT fill:#ef4444,color:#fff
    style SMALL fill:#22c55e,color:#fff
    style CHUNK fill:#22c55e,color:#fff
```

### Tree Shaking
Tree shaking eliminates dead code from JavaScript bundles at build time.

```mermaid
flowchart LR
    SOURCE["Source Code:\nimport { formatDate } from 'utils'\n\nutils.js exports 50 functions\nOnly formatDate is used"] --> BUNDLER["Bundler (Webpack / Rollup / esbuild)\nStatic analysis of import graph\nIdentifies used exports"]
    BUNDLER --> OUTPUT["Bundle: only formatDate included\nOther 49 functions removed (dead code)\nRequires ES Modules (import/export)\nNOT CommonJS (require)"]

    style SOURCE fill:#f59e0b,color:#fff
    style OUTPUT fill:#22c55e,color:#fff
```

### API Caching

| Tool | Approach | Best For |
|---|---|---|
| **React Query (TanStack Query)** | `useQuery` — automatic stale-while-revalidate, retry, background refetch | REST APIs with cache invalidation |
| **SWR** | `useSWR` — stale-while-revalidate pattern, lightweight | Simple data fetching with auto-revalidation |
| **Apollo Client** | GraphQL-specific; normalized in-memory cache | GraphQL APIs; complex data dependencies |
| **RTK Query** | Redux Toolkit's built-in data fetching + caching | Apps already using Redux |

---

## 4. Pagination & Rate Limiting

### Pagination Strategies

```mermaid
flowchart TD
    PAG(["Pagination Strategy"]) --> LO["Limit-Offset\nGET /items?limit=20&offset=40\nSimple, widely supported\nCON: slow for large offsets (DB scans all rows)\nCON: duplicates if rows inserted while paging"]

    PAG --> CB["Cursor-Based\nGET /items?cursor=eyJpZCI6MTIzfQ==\nOpaque token (base64 of last-seen ID/timestamp)\nPRO: O(log N) — uses index efficiently\nPRO: stable — inserts/deletes don't shift pages\nBest for: infinite scroll, social feeds"]

    PAG --> PB["Page-Based\nGET /items?page=3&pageSize=20\nSame as offset (page * size = offset)\nFamiliar UX: 'Page 1 of 50'\nSame cons as offset"]

    style LO fill:#f59e0b,color:#fff
    style CB fill:#22c55e,color:#fff
    style PB fill:#f59e0b,color:#fff
```

### Debounce vs Throttle

```mermaid
flowchart LR
    subgraph DEBOUNCE ["Debounce — 'Wait for silence'"]
        D1["User typing: a...an...and...andr...\nEach keystroke resets a timer\nCallback fires ONLY when user\nstops typing for N ms\nUse case: search autocomplete\nType 'android' → one API call"]
    end

    subgraph THROTTLE ["Throttle — 'Allow max N times per second'"]
        T1["Scroll event fires 100x/second\nThrottle: allow at most once per 100ms\nCallback fires at regular intervals\nUse case: scroll listeners, resize,\nmap pan — steady stream but limited"]
    end

    EVENTS(["High-frequency events\n(typing, scrolling, resizing, mouse move)"]) --> DEBOUNCE
    EVENTS --> THROTTLE

    style D1 fill:#0078D4,color:#fff
    style T1 fill:#22c55e,color:#fff
    style EVENTS fill:#0f172a,color:#fff
```

### Rate Limiting (Server-Side)

| Algorithm | How it works | Best for |
|---|---|---|
| **Fixed Window** | Counter resets every N seconds; reject if limit exceeded | Simple APIs; cheap to implement |
| **Sliding Window** | Count requests in a rolling time window | More accurate; prevents boundary bursts |
| **Token Bucket** | Tokens refill at rate R; each request consumes one | Burst-friendly; AWS API Gateway uses this |
| **Leaky Bucket** | Requests processed at fixed rate; excess queued/dropped | Smooth, predictable output rate |

---

## 5. Communication Protocols

### Protocol Decision Tree

```mermaid
flowchart TD
    NEED(["Frontend communication need"]) --> Q1{"Request type?"}

    Q1 -->|"Simple request-response"| REST_C["REST / HTTP\nGET POST PUT DELETE\nStateless, simple, cacheable\nJSON + HTTP/1.1 or HTTP/2"]

    Q1 -->|"Query with flexible fields"| GQL_C["GraphQL\nSingle endpoint /graphql\nClient defines shape of response\nNo over-fetching"]

    Q1 -->|"Real-time server push"| Q2{"Bi-directional?"}

    Q2 -->|"Server → Client only\n(notifications, feeds)"| SSE_C["Server-Sent Events\ntext/event-stream\nAuto-reconnect\nSimpler than WebSockets\nHTTP/2 multiplexed"]

    Q2 -->|"Full bi-directional\n(chat, gaming)"| WS_C["WebSockets\nPersistent TCP upgrade\nws:// or wss://\nFull-duplex"]

    Q1 -->|"Service-to-service\n(BFF calling backend)"| GRPC_C["gRPC\nHTTP/2 + Protobuf\nStrongly typed contracts\nHigh throughput, low latency\nNot browser-native (needs grpc-web)"]

    style REST_C fill:#0078D4,color:#fff
    style GQL_C fill:#8b5cf6,color:#fff
    style SSE_C fill:#22c55e,color:#fff
    style WS_C fill:#22c55e,color:#fff
    style GRPC_C fill:#f59e0b,color:#fff
```

### Long Polling vs WebSockets vs SSE

| | Long Polling | SSE | WebSockets |
|---|---|---|---|
| **Connection** | New HTTP request per message | One persistent HTTP connection | One persistent TCP connection |
| **Direction** | Client pulls (simulated push) | Server → Client only | Bidirectional |
| **Overhead** | High (new request per update) | Low (one connection, stream) | Low (persistent) |
| **Auto-reconnect** | Client must implement | Built into EventSource API | Client must implement |
| **Complexity** | Low | Low | High |
| **Use case** | Legacy support, simple fallback | Live ticker, notifications, feeds | Chat, games, collaborative editing |

---

## 6. Frontend Architecture Patterns

### Pattern Taxonomy

```mermaid
flowchart TD
    PATTERNS["Frontend Architecture Patterns"] --> LAYERED["Layered Patterns\n(who knows about whom?)"]
    PATTERNS --> COMPONENT["Component-Based\n(UI structure)"]
    PATTERNS --> SYSTEM["System Patterns\n(code organization)"]

    LAYERED --> MVC_L["MVC — Model View Controller\nController mediates View ↔ Model"]
    LAYERED --> MVP_L["MVP — Model View Presenter\nPassive View; Presenter drives UI"]
    LAYERED --> MVVM_L["MVVM — Model View ViewModel\nViewModel exposes observable state"]
    LAYERED --> MVVMC_L["MVVM-C — + Coordinator\nAdds navigation coordination layer"]
    LAYERED --> VIPER_L["VIPER\nView-Interactor-Presenter-Entity-Router\nStrict mobile-first separation"]

    SYSTEM --> CLEAN["Clean Architecture\nConcentric dependency rings"]
    SYSTEM --> HEX["Hexagonal (Ports & Adapters)\nCore ← Ports ← Adapters (UI/DB/API)"]
    SYSTEM --> SCREAM["Screaming Architecture\nFolder structure by domain, not by type"]
    SYSTEM --> VERT["Vertical Slices\nFeature-first slices cut through all layers"]

    style LAYERED fill:#0078D4,color:#fff
    style COMPONENT fill:#8b5cf6,color:#fff
    style SYSTEM fill:#22c55e,color:#fff
```

### MVC vs MVP vs MVVM

```mermaid
flowchart TD
    subgraph MVC ["MVC (Original)"]
        M1["Model\n(data + business logic)"]
        V1["View\n(may update Model directly)"]
        C1["Controller\n(handles events, updates Model)"]
        V1 <-->|"View knows Model"| M1
        C1 --> M1
        C1 --> V1
    end

    subgraph MVP ["MVP (Testable)"]
        M2["Model\n(data + business logic)"]
        V2["View (Passive)\n(implements interface, no logic)"]
        P2["Presenter\n(all UI logic + updates View via interface)"]
        P2 --> M2
        P2 <-->|"Only via interface"| V2
    end

    subgraph MVVM ["MVVM (Reactive)"]
        M3["Model\n(data + business logic)"]
        V3["View\n(data-binds to ViewModel)"]
        VM3["ViewModel\n(exposes observable state\nno reference to View)"]
        V3 <-->|"Two-way binding\nobservable/reactive"| VM3
        VM3 --> M3
    end

    style M1 fill:#0f172a,color:#fff
    style V1 fill:#ef4444,color:#fff
    style C1 fill:#f59e0b,color:#fff
    style M2 fill:#0f172a,color:#fff
    style V2 fill:#0078D4,color:#fff
    style P2 fill:#22c55e,color:#fff
    style M3 fill:#0f172a,color:#fff
    style V3 fill:#0078D4,color:#fff
    style VM3 fill:#22c55e,color:#fff
```

### Hexagonal Architecture (Ports & Adapters)

```mermaid
flowchart LR
    UI_A["UI Adapter\n(React, Angular, Vue)"]
    DB_A["DB Adapter\n(Prisma, TypeORM)"]
    API_A["API Adapter\n(REST, GraphQL, gRPC)"]
    MSG_A["Message Adapter\n(Kafka, SQS)"]

    UI_A --> PORT_IN["Inbound Port\n(UseCase interfaces)"]
    PORT_IN --> CORE["Core Domain\n(Business Logic\nPure functions\nNo external deps)"]
    CORE --> PORT_OUT["Outbound Port\n(Repository interfaces)"]
    PORT_OUT --> DB_A
    PORT_OUT --> API_A
    PORT_OUT --> MSG_A

    style CORE fill:#22c55e,color:#fff
    style PORT_IN fill:#0078D4,color:#fff
    style PORT_OUT fill:#8b5cf6,color:#fff
    style UI_A fill:#f59e0b,color:#fff
```

### Screaming Architecture vs Vertical Slices

```mermaid
flowchart LR
    subgraph TECH ["Technical Structure (anti-pattern)"]
        direction TB
        TA1["src/\n├── components/\n│   ├── ProductCard.tsx\n│   ├── CartItem.tsx\n│   └── UserProfile.tsx\n├── hooks/\n│   ├── useProduct.ts\n│   └── useCart.ts\n└── services/\n    ├── productService.ts\n    └── cartService.ts"]
    end

    subgraph SCREAM2 ["Screaming / Feature-First (recommended)"]
        direction TB
        SA1["src/\n├── features/\n│   ├── product/\n│   │   ├── ProductCard.tsx\n│   │   ├── useProduct.ts\n│   │   └── productService.ts\n│   └── cart/\n│       ├── CartItem.tsx\n│       ├── useCart.ts\n│       └── cartService.ts"]
    end

    subgraph VERT2 ["Vertical Slices (per use case)"]
        direction TB
        VA1["src/\n├── AddToCart/\n│   ├── AddToCartButton.tsx (UI)\n│   ├── addToCart.handler.ts (logic)\n│   └── cart.repository.ts (data)\n└── ViewProduct/\n    ├── ProductPage.tsx\n    ├── viewProduct.query.ts\n    └── product.api.ts"]
    end

    style TECH fill:#ef4444,color:#fff
    style SCREAM2 fill:#22c55e,color:#fff
    style VERT2 fill:#0078D4,color:#fff
```

### Architecture Pattern Interview Q&A

| Question | Answer |
|---|---|
| Why is Screaming Architecture preferred over technical folder structure? | When you open the codebase, the folder structure should "scream" the domain. `src/features/checkout` tells you more than `src/components`. Grouping by feature co-locates all related code (UI, logic, tests, types) — reducing cognitive load and cross-folder jumps. |
| What is the key invariant of Clean Architecture? | The dependency rule: source code dependencies must point inward only. The domain/entity layer has zero knowledge of the UI framework, database, or network. Outer layers (UI, DB adapters) depend on inner layers, never the reverse. |
| How does Hexagonal Architecture differ from Clean? | Both share the "ports and adapters" intuition, but Hexagonal makes the port-as-interface concept explicit: inbound ports (APIs into the app) and outbound ports (interfaces the app calls out through). Clean Architecture adds the concentric-ring visual metaphor and names layers more specifically. |

---

## 7. Thick vs Thin Clients

```mermaid
flowchart LR
    subgraph THIN ["Thin Client"]
        T_SERVER["Server holds all logic + state\nClient is just a display terminal\nExamples: Server-rendered pages,\nterminal apps, remote desktop\nPros: Simple client, easy to update\nCons: Server-dependent, offline impossible"]
    end

    subgraph THICK ["Thick / Fat Client"]
        TH_CLIENT["Client holds significant logic + state\nRich local capabilities\nExamples: SPAs, desktop apps, games\nPros: Offline capable, rich UX\nCons: Larger download, device resource use"]
    end

    subgraph SMART ["Smart Client (Modern Middle Ground)"]
        SM["Hybrid: logic split between client and server\nClient handles UI state, validation, optimistic UI\nServer handles auth, business rules, persistence\nExamples: Next.js with Server Components\nPros: Best of both worlds\nCons: Complex deployment"]
    end

    style THIN fill:#ef4444,color:#fff
    style THICK fill:#0078D4,color:#fff
    style SMART fill:#22c55e,color:#fff
```

---

## 8. BFF (Backend for Frontend) Pattern

### Overview
A Backend for Frontend is a dedicated server-side layer — thin API gateway — tailored to the needs of a specific frontend client (web, mobile, TV). Instead of one general-purpose API serving all clients differently, each client gets its own BFF that aggregates, shapes, and transforms data specifically for it.

### BFF Architecture

```mermaid
flowchart TD
    WEB_CLIENT["Web Browser\n(React SPA)"] --> WEB_BFF["Web BFF\n(Node.js / Next.js API Routes)\nMerges user + product + inventory\ninto one response payload\nTailored fields for large screen"]

    MOBILE_CLIENT["Mobile App\n(iOS / Android)"] --> MOB_BFF["Mobile BFF\n(Node.js / .NET Minimal API)\nLightweight payloads\nOptimized for slow networks\nPush notification registration"]

    TV_CLIENT["Smart TV"] --> TV_BFF["TV BFF\n(Streamlined content)\nLimited interaction model\nLarge asset serving"]

    WEB_BFF --> USER_SVC["User Service"]
    WEB_BFF --> PRODUCT_SVC["Product Service"]
    WEB_BFF --> INVENTORY_SVC["Inventory Service"]
    MOB_BFF --> USER_SVC
    MOB_BFF --> PRODUCT_SVC

    style WEB_BFF fill:#0078D4,color:#fff
    style MOB_BFF fill:#22c55e,color:#fff
    style TV_BFF fill:#8b5cf6,color:#fff
    style WEB_CLIENT fill:#0f172a,color:#fff
    style MOBILE_CLIENT fill:#0f172a,color:#fff
```

### BFF Interview Q&A

| Question | Answer |
|---|---|
| Why not just have the frontend call microservices directly? | Frontend would need to orchestrate multiple API calls, aggregate responses, handle partial failures, and format data — putting complex logic in the client. The BFF moves this to the server side where it's closer to services, can run parallel requests efficiently, and can be tested independently. |
| What's the difference between BFF and API Gateway? | An API Gateway is a generic cross-cutting infrastructure layer (auth, rate limiting, routing). A BFF is application-specific — it contains business logic tailored to a specific client. You may have a BFF behind an API Gateway. |
| When does BFF become an anti-pattern? | When BFF teams become bottlenecks for frontend teams (they must wait for BFF changes). Solution: frontend teams own their BFF. Also when BFF accumulates too much business logic that should live in domain services. |

---

## 9. Progressive Web Apps (PWA)

### PWA Capabilities

```mermaid
flowchart TD
    PWA(["Progressive Web App"]) --> SW["Service Worker\nJavaScript worker running\nin background thread\nNo access to DOM\nProxy all network requests\nEnable offline capability\nCache-first strategies"]

    PWA --> WAM["Web App Manifest\nmanifest.json\nApp name, icons, theme color\nstart_url, display: standalone\nEnables 'Add to Home Screen'"]

    PWA --> PUSH["Push Notifications\nvia Web Push API\nServer sends push\nService Worker shows notification\nWorks even when app not open"]

    PWA --> OFFLINE["Offline Support\nService Worker caches assets\nCache-first: load from cache,\nupdate in background\nStale-while-revalidate strategy"]

    style SW fill:#0078D4,color:#fff
    style WAM fill:#22c55e,color:#fff
    style PUSH fill:#8b5cf6,color:#fff
    style OFFLINE fill:#f59e0b,color:#fff
```

### Service Worker Caching Strategies

```mermaid
flowchart LR
    REQ(["Request"]) --> STRAT{"Caching\nstrategy?"}

    STRAT -->|"Static assets\n(CSS, JS, fonts)"| CF["Cache First\nCheck cache → serve\nif found, else fetch + cache\nFastest, may be stale"]

    STRAT -->|"API calls where\nfreshness matters"| NF["Network First\nFetch → serve\nif fail, fall back to cache\nFreshest, but slow on bad network"]

    STRAT -->|"Feeds, dashboards"| SWR_SW["Stale-While-Revalidate\nServe cache immediately\nFetch update in background\nCache updated for next visit"]

    STRAT -->|"Login page,\ncritical assets"| NO["Network Only\nAlways fetch, no cache\nReal-time data only"]

    style CF fill:#22c55e,color:#fff
    style NF fill:#0078D4,color:#fff
    style SWR_SW fill:#22c55e,color:#fff
    style NO fill:#ef4444,color:#fff
```

---

## 10. Headless Architecture

### Headless vs Traditional CMS

```mermaid
flowchart LR
    subgraph TRAD ["Traditional CMS (Monolithic)"]
        T_CONTENT["Content + templates\ntightly coupled\nWordPress, Drupal\nOutput: HTML pages only\nChanging frontend = CMS work"]
    end

    subgraph HEADLESS ["Headless CMS + API"]
        H_CMS["Headless CMS\n(Contentful, Sanity, Strapi)\nContent + API only\nNo frontend opinion"]
        H_WEB["Web Frontend\n(Next.js, React)"]
        H_MOB["Mobile App\n(iOS / Android)"]
        H_KIOSK["Kiosk / TV\n(any frontend)"]

        H_CMS -->|"REST / GraphQL API"| H_WEB
        H_CMS -->|"REST / GraphQL API"| H_MOB
        H_CMS -->|"REST / GraphQL API"| H_KIOSK
    end

    style TRAD fill:#ef4444,color:#fff
    style H_CMS fill:#0f172a,color:#fff
    style H_WEB fill:#22c55e,color:#fff
    style H_MOB fill:#0078D4,color:#fff
    style H_KIOSK fill:#8b5cf6,color:#fff
```

### Headless Commerce Architecture

```mermaid
flowchart TD
    STOREFRONT["Storefront\n(Next.js + React — any framework)"] --> BFF_LAYER["BFF / API Layer\n(aggregates headless services)"]
    BFF_LAYER --> HEADLESS_COMM["Commerce Engine\n(Medusa, Commercetools, Shopify Hydrogen)"]
    BFF_LAYER --> HEADLESS_CMS["Content CMS\n(Contentful, Sanity)"]
    BFF_LAYER --> SEARCH_SVC["Search Service\n(Algolia, ElasticSearch)"]
    BFF_LAYER --> PAYMENT["Payment\n(Stripe, Adyen)"]

    style STOREFRONT fill:#22c55e,color:#fff
    style BFF_LAYER fill:#0078D4,color:#fff
    style HEADLESS_COMM fill:#8b5cf6,color:#fff
```

---

## 11. Event-Driven Architecture (EDA)

### EDA Overview

```mermaid
flowchart TD
    PRODUCER["Event Producer\n(Service that emits events)\nOrder Service: 'order.placed'"] -->|"Publishes to"| BROKER["Event Broker\n(Kafka / RabbitMQ / Azure Service Bus / SQS+SNS)\nDecouples producers from consumers\nDurable: stores events\nReplay: reprocess historical events"]

    BROKER -->|"Subscribes"| CONS1["Consumer 1\nInventory Service\nReserves stock when order placed"]
    BROKER -->|"Subscribes"| CONS2["Consumer 2\nNotification Service\nSends confirmation email"]
    BROKER -->|"Subscribes"| CONS3["Consumer 3\nAnalytics Service\nRecords order metrics"]

    style PRODUCER fill:#0f172a,color:#fff
    style BROKER fill:#0078D4,color:#fff
    style CONS1 fill:#22c55e,color:#fff
    style CONS2 fill:#22c55e,color:#fff
    style CONS3 fill:#22c55e,color:#fff
```

### EDA vs Request-Response

| | Request-Response (REST) | Event-Driven |
|---|---|---|
| **Coupling** | Tight: caller knows callee's address | Loose: producer publishes; consumers self-register |
| **Availability** | Both services must be up simultaneously | Consumer can be down; events wait in broker |
| **Scalability** | Caller waits; fan-out = N sequential calls | Broker fans out to N consumers in parallel |
| **Complexity** | Low | High: eventual consistency, ordering guarantees, idempotency |
| **Best for** | Simple CRUD, queries, synchronous workflows | Complex workflows, high fan-out, audit trails |

### Event Brokers Comparison

| Broker | Best For | Key Feature |
|---|---|---|
| **Apache Kafka** | High-throughput event streaming, event sourcing, data pipelines | Durable log; consumer groups; replay; very high throughput |
| **RabbitMQ** | Task queues, microservice messaging, fan-out | Flexible routing (exchanges/bindings); low latency |
| **Azure Service Bus** | Enterprise Azure messaging; sessions; dead-letter queue | Managed; sessions for ordered processing; guaranteed delivery |
| **AWS SQS + SNS** | AWS-native; fan-out with SNS + SQS fanout pattern | Serverless-friendly; SQS for queue, SNS for pub/sub |
| **Redis Pub/Sub** | In-memory; very low latency | Fire-and-forget; not durable; at-most-once |

---

## 12. CQRS & Event Sourcing

### CQRS (Command Query Responsibility Segregation)

```mermaid
flowchart LR
    CLIENT(["Client Request"]) --> Q_TYPE{"Command or\nQuery?"}

    Q_TYPE -->|"Write / Change state"| COMMAND["Command Side\nCreate/Update/Delete\nWrite DB (normalized)\nReturns: void or success/fail\nOptimized for writes"]

    Q_TYPE -->|"Read data"| QUERY["Query Side\nRead-only\nRead DB / Read Model\n(denormalized for reads)\nReturns: data\nOptimized for reads"]

    COMMAND -->|"Publishes domain events"| EVENT_BUS["Event Bus\n(Kafka / Service Bus)"]
    EVENT_BUS -->|"Updates"| READ_MODEL["Read Model (Projection)\nMaterialized view, pre-aggregated\nOptimized for specific queries"]
    READ_MODEL --> QUERY

    style COMMAND fill:#ef4444,color:#fff
    style QUERY fill:#22c55e,color:#fff
    style EVENT_BUS fill:#0078D4,color:#fff
    style READ_MODEL fill:#8b5cf6,color:#fff
```

### Event Sourcing

```mermaid
flowchart LR
    EVENTS["Event Store (Append-Only)\n─────────────────────\nOrderCreated {id:1, ...}\nItemAdded {id:1, item:'shirt'}\nCouponApplied {id:1, code:'SAVE10'}\nOrderPaid {id:1, amount:90}\n─────────────────────\nNever updates/deletes\nSource of truth = the event log"]

    EVENTS -->|"Replay events to build"| CURRENT["Current State\n(order #1 is paid, contains shirt)\nRebuilt from events on demand"]

    EVENTS -->|"Project events to"| VIEWS["Read Views / Projections\nOrder summary view\nCustomer order history\nAnalytics dashboard"]

    style EVENTS fill:#0f172a,color:#fff
    style CURRENT fill:#22c55e,color:#fff
    style VIEWS fill:#0078D4,color:#fff
```

### CQRS + Event Sourcing — Interview Q&A

| Question | Answer |
|---|---|
| What problem does CQRS solve? | Traditional CRUD mixes reads and writes on the same model. Reads and writes have different scaling needs (reads are often 10x more frequent), different data shapes (reads need aggregated views, writes need normalized tables), and different optimization strategies. CQRS separates them cleanly. |
| What is a projection in Event Sourcing? | An event-driven materialized view. As new events are appended to the event store, a projector processes them and updates a read-optimized data store (e.g., a denormalized table or Elasticsearch index). Different projections can represent the same events in different ways for different use cases. |
| What are the trade-offs of Event Sourcing? | Benefits: complete audit log, time travel/replay, decoupled read models. Costs: query complexity (can't just SELECT), eventual consistency (projections lag behind events), and schema evolution (old events must still be processable after business logic changes). |

---

## 13. GoF Design Patterns for Frontend

### Most Relevant Patterns for Web/Mobile

```mermaid
flowchart TD
    GOF_FE["GoF Patterns in Frontend Context"] --> OBS["Observer\n(Event Listeners, Pub/Sub, Store subscriptions)\nSubject notifies observers of state changes\nReact useState + useEffect\nRxJS, EventEmitter, document.addEventListener"]

    GOF_FE --> STRAT_P["Strategy\n(Swappable algorithm)\nPayment strategy (Stripe/PayPal/Apple Pay)\nSorting strategy (by price/date/rating)\nRendering strategy (SSR/CSR/SSG)"]

    GOF_FE --> FAC["Factory\nCreate objects without knowing exact class\nReact.createElement — Factory for DOM elements\nViewControllerFactory.make(route:)\nLoggerFactory.create(level:)"]

    GOF_FE --> DEC["Decorator\nAdd behavior without modifying original class\nHigher-Order Components (HOC) in React\nwithAuth(Component) wraps + adds auth check\nwithLogging(Component)"]

    GOF_FE --> FAC2["Facade\nSimple interface to complex subsystem\nAPIClient hiding fetch + retry + token refresh\nNavigator hiding platform routing details"]

    GOF_FE --> CMD["Command\nEncapsulate action as object\nUndo/Redo in rich editors\nNetwork request queue\nForm submission history"]

    style OBS fill:#0078D4,color:#fff
    style STRAT_P fill:#22c55e,color:#fff
    style FAC fill:#8b5cf6,color:#fff
    style DEC fill:#f59e0b,color:#fff
    style FAC2 fill:#22c55e,color:#fff
    style CMD fill:#0f172a,color:#fff
```

---

## 14. SOLID Principles for Web

### SOLID Applied to Web Development

| Principle | Web Example | Anti-pattern |
|---|---|---|
| **S — Single Responsibility** | `ProductCard` displays a product; `ProductService` fetches products; never mixed | One God Component that fetches, formats, validates, displays, and logs |
| **O — Open/Closed** | Add new payment provider by creating `PayPalProvider implements PaymentProvider` — don't modify existing code | Adding `if (type === 'paypal')` to a monster switch statement |
| **L — Liskov Substitution** | `AdminUser` and `GuestUser` both implement `User` interface; code using `User` works with either | `AdminUser` extends `User` but throws on `logout()` because admins can't log out — breaks callers |
| **I — Interface Segregation** | `LoggingService` only requires `log(message)`, not the full `MonitoringService` interface | Implementing a 20-method interface where only 2 are used |
| **D — Dependency Inversion** | `ReportGenerator` depends on `DataSource` interface, not `MySQLDataSource` class | `ReportGenerator` directly imports `MySQLDataSource` — can't test or swap |

---

## 15. TypeScript Advanced Concepts

### Type System Features

```mermaid
flowchart TD
    TS_TYPES["TypeScript Advanced Types"] --> GENERIC["Generics\nType-safe containers + functions\nfunction getById<T>(id: string): Promise<T>\nUsed for: API response wrappers, collections, utilities"]

    TS_TYPES --> UNION["Union Types\ntype Status = 'loading' | 'success' | 'error'\nMakes impossible states unrepresentable"]

    TS_TYPES --> INTERSECT["Intersection Types\ntype AdminUser = User & Admin\nCombines multiple types"]

    TS_TYPES --> MAPPED["Mapped Types\ntype Partial<T> = { [K in keyof T]?: T[K] }\nGenerate new types from existing types"]

    TS_TYPES --> CONDITIONAL["Conditional Types\ntype IsArray<T> = T extends any[] ? true : false\nType-level if/else"]

    TS_TYPES --> GUARD["Type Guards\nfunction isUser(x: unknown): x is User\nNarrow union types safely"]

    TS_TYPES --> TYPEOF_T["Typeof / Keyof\ntype Keys = keyof User → 'id' | 'name' | 'email'\ntype Config = typeof defaultConfig"]

    style GENERIC fill:#0078D4,color:#fff
    style UNION fill:#22c55e,color:#fff
    style MAPPED fill:#8b5cf6,color:#fff
    style CONDITIONAL fill:#f59e0b,color:#fff
    style GUARD fill:#22c55e,color:#fff
```

### Type vs Interface

| | `type` | `interface` |
|---|---|---|
| **Object shapes** | `type User = { id: number }` | `interface User { id: number }` |
| **Extends** | Uses `&` intersection | Uses `extends` keyword |
| **Declaration merging** | No — can't redeclare | Yes — multiple declarations merge |
| **Union / Intersection** | Yes — `type Result = A | B` | No — interfaces can't do unions |
| **Compute types** | Yes — conditional, mapped, template literal | No |
| **When to use** | Complex computed types, unions, tuples | Object shapes, APIs, classes |

---

## 16. RADIO Interview Framework

### Structured System Design Response

```mermaid
flowchart TD
    R["R — Requirements\n3-5 mins\nFunctional: What the system does (user stories)\nNon-Functional: Scale, latency, availability,\noffline, accessibility, SEO, security"] --> A

    A["A — Architecture Overview\n5 mins\nHigh-level diagram\nList major components\nChoose rendering strategy and justify\nClient ↔ API ↔ Services ↔ DB"] --> D

    D["D — Data Model & APIs\n5-10 mins\nKey entities: User, Post, Order, Cart\nAPI contracts: endpoints, request/response shapes\nData flow diagrams\nState management approach"] --> I

    I["I — Interface & Optimizations\n5-10 mins\nUI component structure\nPerformance: lazy loading, pagination, caching\nNetwork: debounce, retry, offline support\nBundle: code splitting, tree shaking"] --> O

    O["O — Observability & Edge Cases\n5 mins\nLogging, error reporting (Sentry, Datadog)\nAnalytics, A/B testing\nEdge cases: empty state, error state, loading state\nSecurity: XSS, CSRF, auth flows"]

    style R fill:#0078D4,color:#fff
    style A fill:#8b5cf6,color:#fff
    style D fill:#22c55e,color:#fff
    style I fill:#f59e0b,color:#fff
    style O fill:#ef4444,color:#fff
```

### Design a Feature Walk-through Example — "News Feed"

| RADIO Step | Decisions |
|---|---|
| **Requirements** | Users see personalized posts; infinite scroll; offline support; < 2s initial load; millions of DAU |
| **Architecture** | Next.js (ISR for initial page, client-side infinite scroll). BFF aggregates user service + post service + ad service. CDN for assets. |
| **Data Model** | `Post { id, authorId, content, media[], timestamp, likeCount }`. `GET /feed?cursor=<timestamp>&limit=20`. Cursor pagination. |
| **Interface & Opt** | `IntersectionObserver` for infinite scroll trigger. React Query for caching posts (stale-while-revalidate). Images lazy-loaded. Service Worker caches last 50 posts for offline. |
| **Observability** | Sentry for JS errors. Core Web Vitals (LCP, FID, CLS) tracked. Rollback plan: feature flag kills infinite scroll. |

---

## 17. Core Web Vitals & Accessibility

### Core Web Vitals

```mermaid
flowchart TD
    CWV["Core Web Vitals\n(Google ranking signals)"] --> LCP["LCP — Largest Contentful Paint\nTime to render largest visible element\nTarget: < 2.5s\nFix: optimize images, SSR, CDN, preload LCP image"]

    CWV --> FID["FID — First Input Delay (→ INP in 2024)\nTime from first interaction to browser response\nTarget: < 100ms\nFix: break long tasks, defer non-critical JS"]

    CWV --> CLS["CLS — Cumulative Layout Shift\nSum of unexpected layout shifts\nTarget: < 0.1\nFix: reserve space for images/ads\n(width + height attributes)\nAvoid injecting content above fold"]

    style LCP fill:#22c55e,color:#fff
    style FID fill:#0078D4,color:#fff
    style CLS fill:#8b5cf6,color:#fff
    style CWV fill:#0f172a,color:#fff
```

### Accessibility (a11y) Essentials

| Area | Rule | Implementation |
|---|---|---|
| **Semantic HTML** | Use `<button>`, `<nav>`, `<main>`, `<article>` — not `<div>` everywhere | Correct element roles convey meaning to screen readers |
| **ARIA** | Use ARIA only when semantic HTML is insufficient | `aria-label`, `aria-describedby`, `aria-live`, `role` attributes |
| **Color contrast** | WCAG 2.1 AA: 4.5:1 ratio for normal text | Use contrast checker tool |
| **Keyboard navigation** | All interactive elements reachable via Tab; visible focus ring | Never `outline: none` without alternative |
| **Alt text** | All `<img>` with `alt` attribute | Decorative images: `alt=""` (screen reader skips) |
| **Form labels** | Every input has associated `<label>` or `aria-label` | Click label → focuses input |
| **Focus management** | After modal opens, move focus into it; on close, return focus | Trap focus within modal (`aria-modal`) |

### Security Headers & XSS Prevention

| Threat | Prevention |
|---|---|
| **XSS (Cross-Site Scripting)** | Never use `dangerouslySetInnerHTML` with unsanitized input. Use DOMPurify for sanitization. React auto-escapes JSX. |
| **CSRF** | `SameSite=Strict` cookies. CSRF token in forms. |
| **Content Injection** | Content Security Policy (CSP) header: restrict which scripts/styles can load |
| **Clickjacking** | `X-Frame-Options: DENY` or CSP `frame-ancestors 'none'` |
| **CORS** | Server whitelists allowed origins. Never `Access-Control-Allow-Origin: *` for auth endpoints. |
