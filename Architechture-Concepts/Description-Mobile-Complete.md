# Mobile System Design — Complete Reference Guide

> Consolidates: Mobile System Design · UI Frameworks · API & Networking · Storage · Testing · Privacy · Advanced Topics · Design Patterns · Interview Strategy

---

## Table of Contents

1. [UI Frameworks](#1-ui-frameworks)
2. [Lifecycle Management](#2-lifecycle-management)
3. [Threading & Concurrency](#3-threading--concurrency)
4. [Navigation](#4-navigation)
5. [Data Binding](#5-data-binding)
6. [Data Storage](#6-data-storage)
7. [API Communication Protocols](#7-api-communication-protocols)
8. [Real-Time Updates](#8-real-time-updates)
9. [Pagination Strategies](#9-pagination-strategies)
10. [Caching Strategies](#10-caching-strategies)
11. [Authentication](#11-authentication)
12. [Retry Policies & Resilience](#12-retry-policies--resilience)
13. [Performance & Optimization](#13-performance--optimization)
14. [Observability & Testing](#14-observability--testing)
15. [Privacy & Security](#15-privacy--security)
16. [App-Wide Architecture Patterns](#16-app-wide-architecture-patterns)
17. [GoF Design Patterns](#17-gof-design-patterns)
18. [SOLID Principles for Mobile](#18-solid-principles-for-mobile)
19. [Advanced Topics](#19-advanced-topics)
20. [Interview Strategy](#20-interview-strategy)

---

## 1. UI Frameworks

### Overview
Mobile UI frameworks define how the user interface is constructed and updated. The industry has shifted from **imperative** (tell the system *how* to change the UI step by step) to **declarative** frameworks (describe *what* the UI should look like for a given state, and the framework handles the rest).

### Declarative vs Imperative

```mermaid
flowchart LR
    subgraph Declarative ["Declarative (Modern — State Drives UI)"]
        DS["State changes → Framework re-renders UI automatically\nDev describes the desired outcome"]
        iOS_D["iOS: SwiftUI\n(2019+, all Apple platforms)"]
        AND_D["Android: Jetpack Compose\n(2021+, replaces XML layouts)"]
    end

    subgraph Imperative ["Imperative (Traditional — Manual UI Updates)"]
        IS["Dev explicitly calls UI update methods\nbutton.setText(), view.setVisibility()"]
        iOS_I["iOS: UIKit\n(2008–present, battle-tested)"]
        AND_I["Android: View System (XML + Code)\n(2008–present)"]
    end

    STATE(["App State"]) -->|"Automatically re-renders"| Declarative
    STATE -->|"Dev manually triggers"| Imperative

    style DS fill:#22c55e,color:#fff
    style iOS_D fill:#0078D4,color:#fff
    style AND_D fill:#22c55e,color:#fff
    style IS fill:#f59e0b,color:#fff
    style iOS_I fill:#8b5cf6,color:#fff
    style AND_I fill:#8b5cf6,color:#fff
    style STATE fill:#0f172a,color:#fff
```

### Framework Comparison

| Aspect | SwiftUI (iOS) | Jetpack Compose (Android) | UIKit (iOS) | View System (Android) |
|---|---|---|---|---|
| **Paradigm** | Declarative | Declarative | Imperative | Imperative |
| **Language** | Swift | Kotlin | Swift / Obj-C | Kotlin / Java / XML |
| **State management** | `@State`, `@ObservedObject`, `@EnvironmentObject` | `remember`, `mutableStateOf`, `ViewModel` | Manual / KVO / Combine | LiveData / Flow / ViewModel |
| **Performance** | Good — diff algorithm | Good — smart recomposition | Excellent — battle-tested | Excellent — very mature |
| **Learning curve** | Medium | Medium | High (Obj-C roots) | High (XML + Java) |
| **Best for** | Greenfield iOS apps | Greenfield Android apps | Complex legacy iOS | Complex legacy Android |

### Interview Talking Points

| Question | Answer |
|---|---|
| What is the key benefit of declarative UI? | The UI is a pure function of state — when state changes, the framework computes the minimal set of UI updates needed. Developers stop thinking about *how* to update (add this view, remove that button) and start thinking about *what* the UI should look like in each state. |
| When would you still use UIKit over SwiftUI? | For apps targeting iOS < 13, for complex custom animations where UIKit APIs are richer, for very performance-sensitive custom views, or when integrating with existing UIKit codebases. SwiftUI and UIKit can interoperate via `UIViewRepresentable`. |
| What is Jetpack Compose recomposition? | When state observed by a composable function changes, Compose automatically re-executes (recomposes) only the affected composable subtrees. It skips functions whose inputs haven't changed (smart recomposition). |

---

## 2. Lifecycle Management

### Overview
Lifecycle management refers to how apps and their UI components respond to state changes — foreground, background, termination — triggered by the OS or user actions.

### iOS Lifecycle

```mermaid
stateDiagram-v2
    [*] --> NotRunning
    NotRunning --> Inactive : Launch
    Inactive --> Active : Became active
    Active --> Inactive : Interrupt (call, notification)
    Inactive --> Background : Home button / swipe
    Background --> Suspended : OS suspends (low memory)
    Suspended --> Background : Wake up
    Background --> NotRunning : Terminated
    Suspended --> NotRunning : Terminated
```

| iOS Component | Responsibility |
|---|---|
| `AppDelegate` | App-level lifecycle (launch, terminate, push token registration) |
| `SceneDelegate` | Per-window/scene lifecycle (multiple windows on iPad) |
| `UIViewController` | Individual screen lifecycle (`viewDidLoad`, `viewWillAppear`, `viewDidDisappear`) |
| `SwiftUI View` | Lifecycle managed by state; use `onAppear` / `onDisappear` modifiers |

### Android Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created : onCreate()
    Created --> Started : onStart()
    Started --> Resumed : onResume()
    Resumed --> Paused : onPause()
    Paused --> Resumed : User returns
    Paused --> Stopped : onStop()
    Stopped --> Started : onRestart()
    Stopped --> Destroyed : onDestroy()
    Destroyed --> [*]
```

| Android Component | Responsibility |
|---|---|
| `Application` | App-level init; singleton for app-wide state |
| `Activity` | Single full-screen UI; has `onCreate/onStart/onResume/onPause/onStop/onDestroy` |
| `Fragment` | Reusable UI component nested inside Activity; has its own lifecycle |
| `ViewModel` | Survives configuration changes (rotation); stores UI-related data |
| `Jetpack Compose` | Composables managed via `LaunchedEffect`, `DisposableEffect` |

---

## 3. Threading & Concurrency

### Overview
All UI updates must happen on the main thread. Any heavy work (network, disk I/O, computation) must be offloaded to background threads. Violating this causes ANR (App Not Responding) on Android and UI freeze on iOS.

### Threading Model Comparison

```mermaid
flowchart TD
    subgraph iOS ["iOS Concurrency Evolution"]
        GCD["GCD (Grand Central Dispatch)\nLow-level C API\nDispatchQueue.global().async{}"]
        OQ["Operation Queue\nNSOperationQueue\nDependency management"]
        SW["Swift async/await + Tasks + Actors\nStructured concurrency\nSafe shared mutable state"]
        GCD --> OQ --> SW
    end

    subgraph Android ["Android Concurrency Evolution"]
        LP["Looper + Handler + MessageQueue\nLow-level thread communication"]
        TP["ThreadPoolExecutor\nManaged thread pool"]
        CR["Kotlin Coroutines\nviewModelScope.launch{}\nStructured, lightweight"]
        LP --> TP --> CR
    end

    UI(["Main Thread / UI Thread\n(ALL UI updates here)"]) -.->|"Dispatch back to main"| iOS
    UI -.->|"Dispatch back to main"| Android

    style UI fill:#0f172a,color:#fff
    style SW fill:#22c55e,color:#fff
    style CR fill:#22c55e,color:#fff
```

### ANR vs Crash

| | ANR (App Not Responding) | Crash |
|---|---|---|
| **Cause** | Main thread blocked > 5s (Android) | Unhandled exception or fatal error |
| **User sees** | "App not responding" dialog | App closes abruptly |
| **Fix** | Move work off main thread | Fix the exception / add error handling |
| **Detection** | Android Studio + StrictMode | Crashlytics, Sentry |

---

## 4. Navigation

### Overview
Navigation manages how users move between screens. Modern approaches use a navigation stack (push/pop) and support deep links — URLs that open specific screens within an app.

### Navigation Architecture

```mermaid
flowchart LR
    subgraph iOS_NAV ["iOS Navigation"]
        UINav["UINavigationController\n(UIKit — stack-based)"]
        SWNav["SwiftUI NavigationStack\n(declarative, value-based)"]
        COORD["Coordinator Pattern\nDecouples navigation logic\nfrom ViewControllers\nSingle navigation object\nmanages flow"]
    end

    subgraph AND_NAV ["Android Navigation"]
        NAVCOMP["Navigation Component\n(Jetpack)\nNavGraph + NavController\nBack stack management\nDeep link support"]
    end

    DEEPLINK["Deep Links\napp://product/123\nhttps://contoso.com/product/123\n\nOpen specific screen\ndirectly from URL"]

    iOS_NAV --> DEEPLINK
    AND_NAV --> DEEPLINK

    style UINav fill:#0078D4,color:#fff
    style SWNav fill:#22c55e,color:#fff
    style COORD fill:#8b5cf6,color:#fff
    style NAVCOMP fill:#22c55e,color:#fff
    style DEEPLINK fill:#f59e0b,color:#fff
```

### Coordinator Pattern (iOS)

```mermaid
flowchart TD
    APP["AppCoordinator\n(Root — manages app flow)"] --> AUTH["AuthCoordinator\n(Login, Register, ForgotPwd)"]
    APP --> MAIN["MainCoordinator\n(Tab bar + child flows)"]
    MAIN --> HOME["HomeCoordinator"]
    MAIN --> PROFILE["ProfileCoordinator"]

    style APP fill:#0f172a,color:#fff
    style AUTH fill:#8b5cf6,color:#fff
    style MAIN fill:#0078D4,color:#fff
    style HOME fill:#22c55e,color:#fff
    style PROFILE fill:#22c55e,color:#fff
```

---

## 5. Data Binding

### Overview
Data binding synchronizes UI state with underlying data models — when the model changes, the UI updates automatically, and vice versa.

### Binding Approaches

```mermaid
flowchart LR
    subgraph iOS_BIND ["iOS Data Binding"]
        OBJ["ObservableObject + @Published\n(SwiftUI — reactive)"]
        COMB["Combine Framework\nPublisher/Subscriber\nasync event streams"]
        KVO["KVO (Key-Value Observing)\nObjective-C legacy\nObserves property changes"]
        CH["Completion Handlers\nSimple async callback\nfor one-time results"]
    end

    subgraph AND_BIND ["Android Data Binding"]
        LD["LiveData\nLifecycle-aware observable\nOnly updates active observers"]
        SF["StateFlow / Flow\nKotlin Coroutines\nStateful reactive streams"]
    end

    UI(["UI Layer"]) <-->|"Two-way binding"| iOS_BIND
    UI <-->|"Two-way binding"| AND_BIND
    DATA(["Data / ViewModel"]) --> iOS_BIND
    DATA --> AND_BIND

    style OBJ fill:#22c55e,color:#fff
    style SF fill:#22c55e,color:#fff
    style LD fill:#0078D4,color:#fff
    style COMB fill:#8b5cf6,color:#fff
    style UI fill:#0f172a,color:#fff
```

---

## 6. Data Storage

### Storage Type Decision Tree

```mermaid
flowchart TD
    Q(["What data to store?"]) --> Q1{"Sensitive?\n(tokens, passwords)"}
    Q1 -->|"Yes"| SEC["Secure Storage\niOS: Keychain\nAndroid: EncryptedSharedPreferences\n+ Android KeyStore"]
    Q1 -->|"No"| Q2{"Simple key-value?\n(settings, prefs)"}
    Q2 -->|"Yes"| KV["Key-Value Storage\niOS: UserDefaults\nAndroid: Preferences DataStore"]
    Q2 -->|"No"| Q3{"Structured / relational?\n(users, orders, products)"}
    Q3 -->|"Yes"| DB["Database\niOS: Core Data / SQLite / Realm\nAndroid: Room / ObjectBox"]
    Q3 -->|"No"| Q4{"Large binary?\n(images, video, docs)"}
    Q4 -->|"Yes"| FILE["File Storage\niOS: Documents / Caches dirs\nAndroid: Internal / External storage"]
    Q4 -->|"No"| PROTO["Binary Storage\nAndroid: Proto DataStore\n(typed, Protocol Buffers)"]

    style SEC fill:#ef4444,color:#fff
    style KV fill:#22c55e,color:#fff
    style DB fill:#0078D4,color:#fff
    style FILE fill:#8b5cf6,color:#fff
    style PROTO fill:#f59e0b,color:#fff
```

### Storage Options Comparison

| Type | iOS | Android | Use Case | Limit |
|---|---|---|---|---|
| **Key-Value** | `UserDefaults` | `SharedPreferences` / `Preferences DataStore` | Settings, toggles, small preferences | Small (< 1MB) |
| **Database** | `Core Data`, `SQLite`, `Realm` | `Room`, `SQLite`, `ObjectBox` | Relational/structured data with queries | GB (device storage) |
| **Secure** | `Keychain` | `EncryptedSharedPreferences` + `KeyStore` | Auth tokens, passwords, crypto keys | Small |
| **File** | Documents / Caches dirs | Internal / External storage | Images, video, large documents | Device storage |
| **Binary** | — | `Proto DataStore` | Type-safe structured binary data | Device storage |

---

## 7. API Communication Protocols

### Overview
Mobile apps communicate with backends via several protocols, each with different trade-offs in performance, complexity, and use case fit.

### Protocol Comparison

```mermaid
flowchart LR
    CLIENT(["Mobile Client"]) --> REST
    CLIENT --> WS
    CLIENT --> GQL
    CLIENT --> GRPC

    subgraph REST ["REST"]
        R1["HTTP verbs: GET POST PUT DELETE\nStateless, resource-based\nJSON over HTTP/1.1 or HTTP/2\nSimple, widely understood\nCan over-fetch / under-fetch"]
    end

    subgraph WS ["WebSockets"]
        W1["Persistent TCP connection\nFull-duplex bi-directional\nws:// or wss://\nLow latency — ideal for chat,\ngaming, collaborative apps"]
    end

    subgraph GQL ["GraphQL"]
        G1["Query language for APIs\nClient specifies exact fields needed\nSingle endpoint\nPrevents over-fetching\nComplex caching"]
    end

    subgraph GRPC ["gRPC"]
        GR1["HTTP/2 + Protocol Buffers\nStrong typing, code gen\nHigh performance, low latency\nIdeal for microservices\nBinary format (not human-readable)"]
    end

    style R1 fill:#0078D4,color:#fff
    style W1 fill:#22c55e,color:#fff
    style G1 fill:#8b5cf6,color:#fff
    style GR1 fill:#f59e0b,color:#fff
    style CLIENT fill:#0f172a,color:#fff
```

### When to Use Each

| Protocol | Best For | Avoid When |
|---|---|---|
| **REST** | Standard CRUD, public APIs, simple request-response | Real-time needs, high-frequency updates |
| **WebSockets** | Live chat, multiplayer games, collaborative editing, live feeds | Infrequent updates (overkill), server simplicity required |
| **GraphQL** | Complex UIs needing flexible data shapes, BFF pattern | Simple data models, teams unfamiliar with GraphQL tooling |
| **gRPC** | Internal microservice-to-service communication, IoT | Public APIs (binary format hard to debug), browser support (limited) |

---

## 8. Real-Time Updates

### Update Strategy Comparison

```mermaid
flowchart TD
    Q(["Need real-time updates?"]) --> FREQ{"How frequent?"}

    FREQ -->|"Occasional\n(every 30s+)"| POLL["HTTP Polling\nSimple: client pings server every N seconds\nInefficient: many wasted requests\nGood for: simple dashboards"]

    FREQ -->|"Variable —\nwhen data is ready"| LP["HTTP Long Polling\nServer holds connection open\nSends response when data arrives\nBetter than polling, more complex"]

    FREQ -->|"Server pushing\nupdates to client only"| SSE["Server-Sent Events (SSE)\nOne-way: server → client\nSingle HTTP connection\nAuto-reconnect built in\nGood for: news feeds, notifications"]

    FREQ -->|"True bi-directional\nreal-time"| WSO["WebSockets\nFull-duplex persistent connection\nLowest latency\nGood for: chat, gaming, collaboration"]

    FREQ -->|"App not running"| PUSH["Push Notifications\nAPNs (iOS) / FCM (Android)\nBattery-efficient\nGood for: alerts, re-engagement"]

    style POLL fill:#f59e0b,color:#fff
    style LP fill:#f59e0b,color:#fff
    style SSE fill:#0078D4,color:#fff
    style WSO fill:#22c55e,color:#fff
    style PUSH fill:#8b5cf6,color:#fff
```

| Strategy | Latency | Battery | Complexity | Direction |
|---|---|---|---|---|
| HTTP Polling | High | Draining | Low | Client → Server |
| Long Polling | Medium | Medium | Medium | Client → Server (held open) |
| SSE | Low | Good | Low-Medium | Server → Client only |
| WebSockets | Lowest | Medium | High | Bidirectional |
| Push Notifications | Low (delivery varies) | Excellent | Medium | Server → Device OS |

---

## 9. Pagination Strategies

### Overview
Pagination splits large datasets into smaller chunks to avoid loading everything at once — essential for performance and bandwidth.

### Strategy Comparison

```mermaid
flowchart TD
    DATA(["Large Dataset"]) --> Q{"Pagination\nStrategy?"}

    Q --> LO["Limit-Offset\nGET /items?limit=20&offset=40\nSimple, universal\nCON: Slow on large offsets\nCON: Skips/duplicates if data changes"]

    Q --> PB["Page-Based\nGET /items?page=3&size=20\nFamiliar to users\nSame cons as offset"]

    Q --> KS["Keyset / Index-Based\nGET /items?after_id=1234\nFast: uses DB index\nStable: add/delete doesn't shift pages\nCON: No random page jump"]

    Q --> CB["Cursor-Based\nGET /items?cursor=eyJpZCI6MTIzNH0=\nOpaque cursor (base64 encoded pointer)\nMost stable and efficient\nIdeal for infinite scroll\nCON: No random page jump"]

    style LO fill:#f59e0b,color:#fff
    style PB fill:#f59e0b,color:#fff
    style KS fill:#22c55e,color:#fff
    style CB fill:#22c55e,color:#fff
```

### Offset vs Cursor — The Key Difference

```mermaid
sequenceDiagram
    participant C as Client
    participant S as Server/DB

    Note over C,S: OFFSET PROBLEM: New item inserted shifts all pages

    C->>S: GET /posts?offset=20 (page 2)
    Note over S: New post inserted at top
    S-->>C: Posts 21-40 (post 20 duplicated, post 41 missed)

    Note over C,S: CURSOR SOLUTION: Anchor to a stable point

    C->>S: GET /posts?cursor=<timestamp or ID of last seen post>
    S-->>C: Posts AFTER that cursor (stable, no duplicates)
```

### Interview Talking Points — Pagination

| Question | Answer |
|---|---|
| Why is offset pagination problematic for social feeds? | Social feeds have frequent inserts at the top. Offset-based pagination shifts all records — a new post at position 0 pushes everything down, causing page 2 to duplicate the last item from page 1 or skip items. Cursor pagination anchors to a stable point in the dataset. |
| How does cursor pagination work? | The server returns a cursor (opaque token — usually a base64-encoded timestamp or ID of the last item). The client sends this cursor on the next request. The server queries `WHERE created_at < cursor_timestamp LIMIT 20` — always returns the next stable page regardless of inserts/deletes. |
| When is offset pagination acceptable? | For datasets that rarely change (product catalogs, archived documents), or when users need to jump to arbitrary pages (page 50 of 200). |

---

## 10. Caching Strategies

### Cache Layers in Mobile

```mermaid
flowchart LR
    SERVER(["Backend Server"]) --> CDN["CDN / Edge Cache\nGeographically distributed\nCaches static assets\nHTTP Cache-Control headers"]
    CDN --> NET["Network Cache\nHTTP ETag / Last-Modified\n304 Not Modified responses\nOS-level HTTP caching"]
    NET --> MEM["Memory Cache\niOS: NSCache\nAndroid: LRUCache\nFastest — in RAM\nLost on app terminate"]
    MEM --> DISK["Disk Cache\nPersistent across launches\nSlower than memory\nLimited by storage"]
    DISK --> DB_CACHE["DB / Repository Cache\nRoom / Core Data\nStructured, queryable\nLong-lived"]

    APP(["App Request"]) -->|"Check caches in order:\nmem → disk → network"| MEM

    style CDN fill:#8b5cf6,color:#fff
    style MEM fill:#22c55e,color:#fff
    style DISK fill:#0078D4,color:#fff
    style SERVER fill:#0f172a,color:#fff
    style APP fill:#0f172a,color:#fff
```

### Cache Invalidation Strategies

| Strategy | How it works | Use case |
|---|---|---|
| **Time-based (TTL)** | Cache expires after N seconds/minutes | Weather data, news feeds |
| **ETag** | Server returns ETag header; client sends `If-None-Match`; server returns 304 if unchanged | Static assets, API responses |
| **Last-Modified** | Server returns `Last-Modified`; client sends `If-Modified-Since`; 304 if unchanged | Documents, images |
| **Manual invalidation** | App explicitly clears cache on user action or event | After user logs out, after write operation |
| **Write-through** | Update cache and backend simultaneously on write | Inventory, account balance |

---

## 11. Authentication

### Auth Flow Overview

```mermaid
flowchart TD
    LOGIN(["User Login"]) --> Q{"Auth method?"}

    Q -->|"Email + Password"| BASIC["HTTPS POST credentials\nServer returns JWT access + refresh token\nNever send password over HTTP"]

    Q -->|"Social Login"| OAUTH["OAuth 2.0 / OpenID Connect\nSign in with Apple / Google\nClient gets ID token\nExchange for app token"]

    Q -->|"Biometric"| BIO["Face ID / Touch ID / Fingerprint\nOS-level biometric challenge\nKeychain / KeyStore releases\nsecret on success"]

    Q -->|"MFA"| MFA["Multi-Factor Auth\nFactor 1: password\nFactor 2: TOTP (Google Auth)\nor SMS OTP"]

    BASIC --> TOKEN["Access Token (short-lived: 15min)\n+ Refresh Token (long-lived: 30 days)\nStored in Keychain / EncryptedPrefs"]
    OAUTH --> TOKEN
    BIO --> TOKEN

    TOKEN --> EXPIRE{"Token expired?"}
    EXPIRE -->|"Yes"| REFRESH["POST /auth/refresh\nSend refresh token\nGet new access token"]
    EXPIRE -->|"No"| API["Call API with\nBearer {access_token}"]
    REFRESH --> API

    style LOGIN fill:#0f172a,color:#fff
    style API fill:#22c55e,color:#fff
    style TOKEN fill:#1e40af,color:#fff
    style REFRESH fill:#f59e0b,color:#fff
    style BIO fill:#8b5cf6,color:#fff
```

### Token Storage Best Practices

| Token | Storage | Why |
|---|---|---|
| Access Token (short-lived) | In-memory (variable) | Least exposure; lost on app termination |
| Refresh Token (long-lived) | iOS Keychain / Android EncryptedSharedPreferences | Encrypted by OS; protected from other apps |
| **Never store in** | UserDefaults / SharedPreferences (unencrypted) | Accessible in device backups, unprotected |

---

## 12. Retry Policies & Resilience

### Retry Decision Flow

```mermaid
flowchart TD
    REQ(["API Request"]) --> FAIL{"Request\nfailed?"}
    FAIL -->|"No"| DONE(["Success"])
    FAIL -->|"Yes"| CLASSIFY{"Error type?"}

    CLASSIFY -->|"5xx (server error)\n429 (rate limited)\nNetwork timeout"| RETRY["Retry eligible"]
    CLASSIFY -->|"4xx (client error)\n400, 401, 403, 404"| NORETRY["Do NOT retry\n(fix the request)"]

    RETRY --> STRAT{"Retry strategy?"}
    STRAT -->|"Simple"| LINEAR["Linear Backoff\nWait: 1s, 2s, 3s, 4s..."]
    STRAT -->|"Recommended"| EXP["Exponential Backoff + Jitter\nWait: 1s, 2s, 4s, 8s + random jitter\nPrevents thundering herd"]

    STRAT -->|"Service consistently failing"| CB["Circuit Breaker\nOpen: stop sending requests\nWait cooling-off period\nHalf-open: test one request\nClose if success"]

    NORETRY --> ERR(["Show user-friendly error"])
    CB -->|"Open state"| FALLBACK(["Return cached data\nor graceful degradation"])

    style DONE fill:#22c55e,color:#fff
    style ERR fill:#ef4444,color:#fff
    style FALLBACK fill:#f59e0b,color:#fff
    style EXP fill:#22c55e,color:#fff
    style CB fill:#8b5cf6,color:#fff
```

### OAuth Token Refresh Retry

```mermaid
sequenceDiagram
    participant APP as Mobile App
    participant API as Backend API
    participant AUTH as Auth Server

    APP->>API: GET /user (with expired access token)
    API-->>APP: 401 Unauthorized

    APP->>AUTH: POST /auth/refresh (refresh token)
    AUTH-->>APP: New access token

    APP->>API: GET /user (with NEW access token)
    API-->>APP: 200 OK + user data
```

---

## 13. Performance & Optimization

### Memory Management

```mermaid
flowchart LR
    subgraph iOS_MEM ["iOS Memory Issues"]
        RC["Retain Cycles\nStrong reference cycles\nin Swift closures\n[weak self] fixes it"]
        PROF["Xcode Instruments\nLeaks instrument\nAllocation graph"]
    end

    subgraph AND_MEM ["Android Memory Issues"]
        CTX["Context Leaks\nHolding Activity reference\nin long-lived objects\n(static fields, singletons)"]
        APROF["Android Studio Profiler\nHeap dumps\nAllocation tracking"]
    end

    style RC fill:#ef4444,color:#fff
    style CTX fill:#ef4444,color:#fff
    style PROF fill:#22c55e,color:#fff
    style APROF fill:#22c55e,color:#fff
```

### CPU & Battery Optimization

| Area | iOS | Android |
|---|---|---|
| **Background tasks** | `BackgroundTasks` framework (`BGTaskScheduler`) | `WorkManager` (guaranteed deferred work) |
| **Power-saving mode** | **App Nap** — reduces background app power | **Doze Mode** — deep sleep when stationary; defers most app activity |
| **Location** | Use `kCLLocationAccuracyReduced` when precision not needed | Use `PRIORITY_BALANCED_POWER_ACCURACY` over `HIGH_ACCURACY` |
| **Target FPS** | 60fps (or 120fps ProMotion) | 60fps; use `RecyclerView` for smooth lists |

### App Startup Optimization

```mermaid
flowchart TD
    LAUNCH(["App Launch"]) --> TYPES{"Launch type"}
    TYPES --> COLD["Cold Start\nProcess not running\nSlowest: load all code"]
    TYPES --> WARM["Warm Start\nProcess alive, Activity destroyed\nFaster: skip app init"]
    TYPES --> HOT["Hot Start\nActivity in backstack\nFastest: just re-display"]

    COLD --> OPT["Optimization strategies:\n1. Defer non-essential init\n2. Lazy load heavy libs\n3. Use app startup library (Android)\n4. Avoid heavy work in didFinishLaunching\n5. Profile with Xcode/Android Studio\n6. Splash screen instead of blank screen"]

    style COLD fill:#ef4444,color:#fff
    style WARM fill:#f59e0b,color:#fff
    style HOT fill:#22c55e,color:#fff
    style LAUNCH fill:#0f172a,color:#fff
    style OPT fill:#0078D4,color:#fff
```

### Rendering & Animation
- **Target 60fps** — each frame budget = 16ms (1000ms ÷ 60)
- **Never do heavy work on the main thread** — I/O, networking, large computation must be async
- **Avoid overdraw** — multiple overlapping views painting the same pixels unnecessarily
- **iOS tools**: Xcode View Debugger, Instruments (Core Animation)
- **Android tools**: Android Studio Layout Inspector, GPU Overdraw visualization

---

## 14. Observability & Testing

### Testing Pyramid for Mobile

```mermaid
flowchart TD
    E2E["E2E Tests\n(Most expensive, slowest)\nDetox, Appium\nFull user journey\nCross-platform"]
    UI_T["UI Tests\n(Simulate user interactions)\niOS: XCUITest, EarlGrey 2.0\nAndroid: Espresso, UI Automator"]
    INT["Integration Tests\n(Modules working together)\nTest data flow between layers"]
    UNIT["Unit Tests\n(Fastest, cheapest)\niOS: XCTest\nAndroid: JUnit + Mockito\nTest individual functions"]
    UNIT --> INT --> UI_T --> E2E

    style UNIT fill:#22c55e,color:#fff
    style INT fill:#0078D4,color:#fff
    style UI_T fill:#f59e0b,color:#fff
    style E2E fill:#ef4444,color:#fff
```

### Beta Distribution & Rollout Strategy

```mermaid
flowchart LR
    BUILD["Build"] --> BETA["Beta Distribution\niOS: TestFlight\nAndroid: Google Play\nInternal/Closed/Open test tracks"]
    BETA --> PHASED["Phased Rollout\nRelease to 1% → 5% → 25% → 50% → 100%\nMonitor crash rate + ANR rate\nRollback instantly if metrics spike"]
    PHASED --> GA(["General Availability"])

    style BUILD fill:#0f172a,color:#fff
    style GA fill:#22c55e,color:#fff
    style PHASED fill:#f59e0b,color:#fff
```

### Crash Reporting & Monitoring

| Tool | Use Case |
|---|---|
| **Firebase Crashlytics** | Crash reporting + stack traces + user impact |
| **Sentry** | Crashes + performance monitoring + error tracking |
| **Datadog / New Relic** | APM + mobile performance metrics |
| **Xcode Instruments** | On-device profiling (memory, CPU, network) |
| **Android Studio Profiler** | CPU, memory, network, energy profiling |

---

## 15. Privacy & Security

### Security Layers

```mermaid
flowchart TD
    subgraph DataInTransit ["Data In Transit"]
        TLS["HTTPS / TLS 1.3\nEncrypts all network traffic\nPrevents man-in-the-middle\nCertificate pinning for extra security"]
    end

    subgraph DataAtRest ["Data At Rest"]
        ENC["Device Encryption\n(enabled by default on modern iOS/Android)\nKeychain / EncryptedSharedPreferences\nfor sensitive app data"]
    end

    subgraph CodeSec ["Code Security"]
        OBF["Obfuscation\nAndroid: R8 / ProGuard\niOS: Swift native (less needed)\nMakes reverse engineering harder"]
        INT["Code Integrity\nCode signing (both platforms)\nApp Store / Play Store verification"]
    end

    subgraph Compliance ["Privacy Compliance"]
        GDPR["GDPR (EU)\nUser consent, data access rights\nRight to be forgotten\nData portability"]
        CCPA["CCPA (California)\nOpt-out of data sale\nDisclosure of data collected"]
        STORE["App Store / Play Store Policies\nPrivacy manifests (iOS 17+)\nData safety section (Android)"]
    end

    style TLS fill:#22c55e,color:#fff
    style ENC fill:#0078D4,color:#fff
    style OBF fill:#8b5cf6,color:#fff
    style GDPR fill:#ef4444,color:#fff
    style CCPA fill:#ef4444,color:#fff
```

### Privacy Best Practices

| Practice | Why it matters |
|---|---|
| **Minimize data collection** | Less data = less breach risk + easier GDPR compliance |
| **Minimum permissions** | Request only permissions needed for core function. Camera app shouldn't need contacts. |
| **Clear data retention policy** | Define how long you keep data; delete on user request |
| **Certificate pinning** | Prevents MITM even if a CA is compromised — pin the server's cert in the app |
| **Jailbreak/root detection** | Compromised devices bypass OS-level security; detect and warn/restrict |

---

## 16. App-Wide Architecture Patterns

### Pattern Evolution

```mermaid
flowchart LR
    MVC["MVC\n(1979 — original)\nModel-View-Controller\nFat controller problem"] -->
    MVP["MVP\n(1990s)\nModel-View-Presenter\nPassive view, testable"] -->
    MVVM["MVVM\n(2005)\nModel-View-ViewModel\nTwo-way binding,\nUI / business logic split"] -->
    MVI["MVI\n(2016)\nModel-View-Intent\nUnidirectional data flow\nImmutable state"] -->
    CLEAN["Clean Architecture\n(Robert Martin)\nConcentric layers\nDependency rule:\nouter depends on inner"]

    style MVC fill:#ef4444,color:#fff
    style MVP fill:#f59e0b,color:#fff
    style MVVM fill:#0078D4,color:#fff
    style MVI fill:#8b5cf6,color:#fff
    style CLEAN fill:#22c55e,color:#fff
```

### VIPER Architecture (Mobile)

```mermaid
flowchart LR
    V["View\nDisplays UI\nPassive — no logic\nDelegates user events\nto Presenter"] <--> P["Presenter\nPrepares data for View\nReceives View events\nCalls Interactor\nHandles presentation logic"]
    P <--> I["Interactor\nBusiness logic\nFetches / processes data\nCalls entities\nUse case layer"]
    I <--> E["Entity\nData models only\nNo business logic\nPure data structures"]
    P <--> R["Router\nNavigation logic\nCreates next screen\nInjects dependencies"]

    style V fill:#0078D4,color:#fff
    style P fill:#8b5cf6,color:#fff
    style I fill:#22c55e,color:#fff
    style E fill:#1e40af,color:#fff
    style R fill:#f59e0b,color:#fff
```

### Clean Architecture Layers

```mermaid
flowchart TD
    subgraph Outer ["Outer Layer — Infrastructure"]
        UI["UI / Views\n(React, SwiftUI, Compose)"]
        DB["Database\n(Room, Core Data, SQLite)"]
        NET["Network\n(Retrofit, URLSession)"]
    end

    subgraph Middle ["Interface Adapters"]
        CTRL["Controllers / Presenters / ViewModels"]
        REPO["Repository Implementations"]
        MAP["Data Mappers (DTO → Domain)"]
    end

    subgraph Inner ["Application Business Logic"]
        UC["Use Cases\n(app-specific business rules)"]
    end

    subgraph Core ["Core — Domain"]
        ENT["Entities\n(enterprise business rules)\nRarely change"]
    end

    Outer --> Middle --> Inner --> Core
    Note["Dependency Rule:\nouter depends on inner\nINNER NEVER knows about outer"]

    style UI fill:#ef4444,color:#fff
    style ENT fill:#22c55e,color:#fff
    style UC fill:#0078D4,color:#fff
    style Note fill:#f59e0b,color:#fff
```

### Pattern Comparison

| Pattern | Best For | Key Benefit | Key Con |
|---|---|---|---|
| **MVC** | Small apps, rapid prototyping | Simple, well-known | Fat controller; View+Model coupling |
| **MVP** | Medium apps, high testability | Pure passive View | Fat Presenter risk |
| **MVVM** | Apps with complex UI state, reactive data | Two-way binding; ViewModel survives rotation | Overkill for simple screens |
| **MVI** | Predictable state machines, Redux-style | Immutable state; easy to debug/test | More boilerplate; learning curve |
| **VIPER** | Large iOS apps with multiple teams | Maximum separation of concerns | Heavy boilerplate |
| **Clean Architecture** | Enterprise apps, long-lived codebases | Framework-agnostic core; highly testable | High initial complexity |

---

## 17. GoF Design Patterns

### Pattern Taxonomy

```mermaid
flowchart TD
    GOF["Gang of Four\nDesign Patterns (23)"] --> CREAT["Creational\n(Object Creation)"]
    GOF --> STRUCT["Structural\n(Composition)"]
    GOF --> BEHAV["Behavioral\n(Communication)"]

    CREAT --> S["Singleton\nFactory Method\nAbstract Factory\nBuilder\nPrototype"]
    STRUCT --> ST["Facade\nAdapter\nDecorator\nProxy\nComposite\nBridge\nFlyweight"]
    BEHAV --> B["Observer\nStrategy\nCommand\nChain of Responsibility\nMediator\nMemento\nInterpreter\nVisitor\nTemplate Method\nIterator\nState"]

    style GOF fill:#0f172a,color:#fff
    style CREAT fill:#0078D4,color:#fff
    style STRUCT fill:#8b5cf6,color:#fff
    style BEHAV fill:#22c55e,color:#fff
```

### Most Common Mobile Patterns

| Pattern | Category | Mobile Use Case | Example |
|---|---|---|---|
| **Singleton** | Creational | Network manager, Logger, Analytics | `NetworkManager.shared`, `Logger.instance` |
| **Builder** | Creational | Complex object construction with optional params | `URLRequest.Builder`, `AlertDialog.Builder` (Android) |
| **Factory** | Creational | Create objects based on type without specifying class | `ViewControllerFactory.make(type:)` |
| **Observer** | Behavioral | Data binding, event handling | `NotificationCenter`, `LiveData`, SwiftUI `@Published` |
| **Strategy** | Behavioral | Swappable algorithm at runtime | Payment strategy (card/PayPal/Apple Pay) |
| **Facade** | Structural | Simplify complex subsystem | `NetworkLayer` hiding URLSession details |
| **Adapter** | Structural | Bridge incompatible interfaces | Wrapping third-party SDK in your own protocol |
| **Decorator** | Structural | Add behavior without subclassing | `UIView` layer decorators, middleware chains |

---

## 18. SOLID Principles for Mobile

### SOLID Quick Reference

```mermaid
flowchart TD
    SOLID["SOLID Principles"] --> SRP["S — Single Responsibility\nA class should have only ONE reason to change\nProblem: Massive ViewController (iOS)\nSolution: Separate networking, parsing, UI logic"]
    SOLID --> OCP["O — Open/Closed\nOpen for extension, closed for modification\nProblem: Endless if/else for cell types\nSolution: Protocol-based configurable cells"]
    SOLID --> LSP["L — Liskov Substitution\nSubtypes must be substitutable for their base\nProblem: Subclass that breaks parent contract\nSolution: Proper protocol conformance"]
    SOLID --> ISP["I — Interface Segregation\nDon't force classes to implement unused methods\nProblem: Huge ChatProtocol\nSolution: Split into smaller focused protocols"]
    SOLID --> DIP["D — Dependency Inversion\nHigh-level modules depend on abstractions\nProblem: ViewController directly imports CoreData\nSolution: FeedProvider protocol — both depend on abstraction"]

    style SRP fill:#0078D4,color:#fff
    style OCP fill:#22c55e,color:#fff
    style LSP fill:#8b5cf6,color:#fff
    style ISP fill:#f59e0b,color:#fff
    style DIP fill:#22c55e,color:#fff
```

### Dependency Injection

```mermaid
flowchart LR
    subgraph Without_DI ["Without DI (Tight Coupling)"]
        VC1["ViewController"] -->|"Creates directly"| CD["CoreData\n(specific implementation)"]
    end

    subgraph With_DI ["With DI (Loose Coupling)"]
        VC2["ViewController"] -->|"Depends on abstraction"| FP["FeedProvider\n(protocol/interface)"]
        FP -->|"Concrete impl injected"| CD2["CoreDataFeedProvider"]
        FP -->|"Test impl injected"| MOCK["MockFeedProvider"]
    end

    style CD fill:#ef4444,color:#fff
    style FP fill:#22c55e,color:#fff
    style CD2 fill:#0078D4,color:#fff
    style MOCK fill:#8b5cf6,color:#fff
```

| DI Tool | Platform | Notes |
|---|---|---|
| Manual DI (constructor injection) | iOS / Android | Best for small apps; explicit, no magic |
| **Swinject** | iOS | Type-safe DI container for Swift |
| **Hilt** | Android | Official Jetpack DI; Dagger under the hood |
| **Dagger** | Android | Compile-time, powerful, steeper learning curve |

---

## 19. Advanced Topics

### On-Device Machine Learning

```mermaid
flowchart LR
    subgraph OnDevice ["On-Device ML"]
        IOS_ML["iOS: Core ML\nPre-trained model .mlmodel\nObjective-C / Swift API\nOptimized for Apple Silicon"]
        AND_ML["Android: TensorFlow Lite\n.tflite model\nKotlin / Java API\nGPU / NPU acceleration"]
    end

    subgraph Benefits ["Benefits vs Cloud ML"]
        PRIV["Privacy: data never leaves device"]
        SPEED["Speed: no network round-trip"]
        OFFLINE["Offline: works without internet"]
    end

    style IOS_ML fill:#0078D4,color:#fff
    style AND_ML fill:#22c55e,color:#fff
    style PRIV fill:#22c55e,color:#fff
    style SPEED fill:#22c55e,color:#fff
    style OFFLINE fill:#22c55e,color:#fff
```

### Advanced Topics Summary

| Topic | iOS | Android | Key Consideration |
|---|---|---|---|
| **On-Device ML** | Core ML | TensorFlow Lite | Model size vs accuracy trade-off |
| **AR** | ARKit | ARCore | Plane detection, anchors, lighting |
| **VR** | Vision Pro (RealityKit) | Cardboard / standalone HMDs | Comfort, motion sickness |
| **Wearables** | WatchKit (watchOS) | Wear OS | Limited display, battery, input |
| **Foldables** | iPad multi-window | WindowSizeClass (Compose) | Seamless layout transitions |
| **Server-Driven UI** | Codable-driven rendering | JSON-driven component tree | Update UI without app update |
| **Cross-Platform** | React Native / Flutter / Xamarin | Same | Trade-off: dev speed vs native feel |
| **i18n** | Localizable.strings, RTL layouts | strings.xml, RTL support | Date/currency/text direction |

---

## 20. Interview Strategy

### RADIO Framework for System Design Interviews

```mermaid
flowchart TD
    R["R — Requirements\n• Functional: What the app does\n• Non-Functional: offline, bandwidth,\n  battery, 60fps, consistency, scale"] --> A
    A["A — Architecture\n• High-level component diagram\n• MVC/MVVM/Clean as starting point\n• Client, API, Controller, Model, View"] --> D
    D["D — Data Model & API\n• Define key data types\n• Choose: REST / GraphQL / WebSocket\n• Design API contracts"] --> I
    I["I — Interface / Optimizations\n• Network: caching, batching, HTTP/2\n• Rendering: lazy load, virtualization\n• App: offline support, pagination"] --> O
    O["O — Observability & Security\n• Logging, crash reporting, metrics\n• Auth, CORS, XSS prevention\n• Privacy compliance"]

    style R fill:#0078D4,color:#fff
    style A fill:#8b5cf6,color:#fff
    style D fill:#22c55e,color:#fff
    style I fill:#f59e0b,color:#fff
    style O fill:#ef4444,color:#fff
```

### Non-Functional Requirements Checklist

| NFR | Questions to ask | Mobile implication |
|---|---|---|
| **Offline mode** | Which features must work offline? What data to cache? | SQLite + sync queue; WorkManager / URLSession background |
| **Bandwidth** | Mobile data costs; slow networks (2G/3G) | Delta updates, image compression, pagination |
| **Battery** | Background location? Push vs polling? | WorkManager batch jobs; avoid wake locks |
| **Scroll performance** | FPS = 60; Long lists? | RecyclerView / LazyColumn; avoid heavy main-thread work |
| **Data consistency** | Strong (chat) or eventual (feed)? | WebSockets for strong; polling/SSE for eventual |
| **Scale** | DAU? Peak load? | Server concerns, but affects caching strategy |
| **Authentication** | Which login methods? Token refresh? | Keychain + refresh token flow |

### Interview Talking Point — Common Red Flags

| Red Flag | Correct Approach |
|---|---|
| "We call the API on the main thread" | Always async; use coroutines / async-await / GCD background queue |
| "We store auth tokens in UserDefaults" | Use Keychain (iOS) or EncryptedSharedPreferences (Android) |
| "We reload all data on every app open" | Cache aggressively; use ETag / TTL; only fetch delta |
| "We request all permissions at launch" | Request permissions just-in-time, when the feature is first used |
| "We use offset pagination for the news feed" | Use cursor pagination for dynamic, frequently-updated datasets |
| "We do heavy computation in cellForRow / onBindViewHolder" | Pre-compute off-screen; main thread for display only |
