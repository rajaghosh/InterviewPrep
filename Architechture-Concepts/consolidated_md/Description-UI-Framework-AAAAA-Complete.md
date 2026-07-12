# Mobile System Design — Complete Guide (iOS & Android)

---

## Table of Contents

1. [Mobile System Design Overview](#1-mobile-system-design-overview)
2. [UI Frameworks](#2-ui-frameworks)
3. [Application Lifecycle Management](#3-application-lifecycle-management)
4. [Threading & Concurrency](#4-threading--concurrency)
5. [Navigation](#5-navigation)
6. [Data Binding & Reactive Programming](#6-data-binding--reactive-programming)
7. [API Design & Networking](#7-api-design--networking)
8. [Software Architecture Patterns](#8-software-architecture-patterns)
9. [GOF Design Patterns in Mobile](#9-gof-design-patterns-in-mobile)
10. [Dependency Injection](#10-dependency-injection)
11. [Data Storage](#11-data-storage)
12. [Observability & Testing](#12-observability--testing)
13. [Privacy & Security](#13-privacy--security)
14. [Advanced Topics](#14-advanced-topics)
15. [Interview Strategy](#15-interview-strategy)

---

## 1. Mobile System Design Overview

### The RADIO Framework for Mobile Design Interviews

```mermaid
flowchart TD
    R["R — Requirements\nFunctional + Non-functional\nEdge cases, constraints"]
    A["A — Architecture\nHigh-level components\nData flows, services"]
    D["D — Data Model\nEntities, relationships\nStorage strategy"]
    I["I — Interfaces\nAPI design, contracts\nScreen flows"]
    O["O — Optimization\nPerformance, caching\nScalability, offline"]

    R --> A --> D --> I --> O

    classDef step fill:#8b5cf6,color:#fff
    class R,A,D,I,O step
```

### Non-Functional Requirements for Mobile

| Category | Requirements |
|---|---|
| Performance | < 100ms UI response, 60fps smooth scroll, < 3s cold start |
| Reliability | Offline support, graceful degradation, crash rate < 0.1% |
| Security | Biometric auth, secure storage, certificate pinning |
| Scalability | Pagination, lazy loading, data compression |
| Battery | Background processing minimized, wake locks avoided |
| Accessibility | VoiceOver/TalkBack support, dynamic font sizes |

---

## 2. UI Frameworks

### iOS vs Android Framework Comparison

```mermaid
flowchart LR
    subgraph iOS["iOS UI Frameworks"]
        SWIFTUI["SwiftUI\nDeclarative (2019+)\nCross-Apple-platform\nState-driven"]
        UIKIT["UIKit\nImperative (2008)\nMature, full control\nCustom transitions"]
    end

    subgraph Android["Android UI Frameworks"]
        COMPOSE["Jetpack Compose\nDeclarative (2021+)\nKotlin-first\nMaterial 3"]
        VSYS["View System (XML)\nImperative (2008)\nMature, broad support"]
    end

    SWIFTUI -.->|"Same paradigm"| COMPOSE
    UIKIT -.->|"Same paradigm"| VSYS

    classDef ios fill:#0078D4,color:#fff
    classDef android fill:#22c55e,color:#fff
    class SWIFTUI,UIKIT ios
    class COMPOSE,VSYS android
```

| Framework | Platform | Paradigm | Pros | Cons |
|---|---|---|---|---|
| SwiftUI | iOS 13+ | Declarative | Less boilerplate, preview canvas, cross-Apple-platform | Less mature, some UIKit features still missing |
| UIKit | iOS 2+ | Imperative | Full control, proven APIs, large community | Verbose, delegate patterns, boilerplate |
| Jetpack Compose | Android API 21+ | Declarative | Kotlin-first, composable functions, live preview | Newer, some View System features require interop |
| View System (XML) | All Android | Imperative | Broad support, familiar, extensive tooling | Verbose XML, findById pattern error-prone |

### SwiftUI vs UIKit Code Comparison

```swift
// SwiftUI — declarative, state-driven
struct UserProfileView: View {
    @StateObject private var viewModel = UserProfileViewModel()

    var body: some View {
        VStack {
            if viewModel.isLoading {
                ProgressView()
            } else {
                Text(viewModel.user?.name ?? "")
                    .font(.title)
            }
        }
        .task { await viewModel.loadUser() }
    }
}

// UIKit — imperative, delegate-based
class UserProfileViewController: UIViewController {
    private let nameLabel = UILabel()
    private let viewModel = UserProfileViewModel()

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
        bindViewModel()
        viewModel.loadUser()
    }

    private func bindViewModel() {
        viewModel.onUserLoaded = { [weak self] user in
            DispatchQueue.main.async {
                self?.nameLabel.text = user.name
            }
        }
    }
}
```

### Jetpack Compose vs XML

```kotlin
// Jetpack Compose — declarative
@Composable
fun UserProfileScreen(viewModel: UserProfileViewModel = hiltViewModel()) {
    val uiState by viewModel.uiState.collectAsState()

    when (uiState) {
        is UiState.Loading -> CircularProgressIndicator()
        is UiState.Success -> Text(
            text = uiState.user.name,
            style = MaterialTheme.typography.headlineMedium
        )
    }
}

// XML View System — imperative
class UserProfileFragment : Fragment(R.layout.fragment_user_profile) {
    private val viewModel: UserProfileViewModel by viewModels()

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        viewModel.uiState.observe(viewLifecycleOwner) { state ->
            when (state) {
                is UiState.Loading -> binding.progressBar.isVisible = true
                is UiState.Success -> binding.nameText.text = state.user.name
            }
        }
    }
}
```

---

## 3. Application Lifecycle Management

### iOS App Lifecycle

```mermaid
stateDiagram-v2
    [*] --> NotRunning
    NotRunning --> InActive: Launch
    InActive --> Active: App appears
    Active --> InActive: Phone call/notification
    InActive --> Background: Home button
    Background --> Suspended: OS suspends
    Suspended --> Background: Resume
    Background --> NotRunning: OS terminates (low memory)

    note right of Background: viewDidDisappear\nsave state here
    note right of Active: viewDidAppear\nresume timers
```

### Android Activity Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created: onCreate()
    Created --> Started: onStart()
    Started --> Resumed: onResume()
    Resumed --> Paused: onPause()
    Paused --> Resumed: onResume()
    Paused --> Stopped: onStop()
    Stopped --> Resumed: onRestart() → onStart()
    Stopped --> Destroyed: onDestroy()
    Destroyed --> [*]

    note right of Paused: save transient state\nrelease camera/GPS
    note right of Stopped: save persistent state
```

### Interview Talking Points

| Question | Answer |
|---|---|
| When should you save state in iOS? | `viewWillDisappear` for UI state; `sceneWillResignActive` for app-level saves; `sceneWillEnterForeground` to refresh stale data. |
| What is `onSaveInstanceState` in Android? | Called before the Activity is destroyed (rotation, back-stack). Use it to save lightweight UI state (scroll position, input text). Restore in `onCreate` via the Bundle. Not for persistent data. |
| What happens to a Swift ViewController in the background? | iOS may terminate background apps to reclaim memory. Always save critical state before entering background; reload on resume. |
| Why use `weak self` in closures? | Prevents retain cycles — if a ViewController holds a closure, and the closure captures `self`, a strong reference cycle forms and memory leaks. `weak self` breaks the cycle. |

---

## 4. Threading & Concurrency

### Concurrency Models

```mermaid
flowchart TD
    subgraph iOS["iOS — Grand Central Dispatch (GCD) / Swift Concurrency"]
        MAIN_IOS["Main Thread\nUI updates only"]
        GCD["DispatchQueue\nBackground work\nglobal(qos:)"]
        ASYNC_IOS["async/await\nStructured concurrency\nSwift 5.5+"]
        ACTOR["Actors\nThread-safe state\nSwift 5.5+"]
    end

    subgraph Android["Android — Coroutines"]
        MAIN_AND["Main (UI) Dispatcher\nUI updates only"]
        IO["IO Dispatcher\nNetwork, DB\n(64 threads)"]
        DEFAULT["Default Dispatcher\nCPU-intensive\n(thread per CPU core)"]
        FLOW["StateFlow / SharedFlow\nReactive streams"]
    end

    classDef ios fill:#0078D4,color:#fff
    classDef android fill:#22c55e,color:#fff
    class MAIN_IOS,GCD,ASYNC_IOS,ACTOR ios
    class MAIN_AND,IO,DEFAULT,FLOW android
```

### Threading Code Examples

```swift
// iOS — Swift async/await (modern)
class UserRepository {
    func fetchUser(id: String) async throws -> User {
        let (data, _) = try await URLSession.shared.data(from: URL(string: "api/users/\(id)")!)
        return try JSONDecoder().decode(User.self, from: data)
    }
}

// iOS — Actor for thread-safe state
actor Cache {
    private var storage: [String: Any] = [:]

    func set(_ key: String, value: Any) {
        storage[key] = value
    }

    func get(_ key: String) -> Any? {
        storage[key]
    }
}

// iOS — GCD (legacy)
DispatchQueue.global(qos: .userInitiated).async {
    let data = fetchData() // heavy work off main thread
    DispatchQueue.main.async {
        self.updateUI(data) // UI updates on main thread
    }
}
```

```kotlin
// Android — Coroutines
class UserViewModel @Inject constructor(
    private val repository: UserRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<UiState>(UiState.Loading)
    val uiState: StateFlow<UiState> = _uiState.asStateFlow()

    fun loadUser(id: String) {
        viewModelScope.launch {
            _uiState.value = UiState.Loading
            runCatching {
                withContext(Dispatchers.IO) { repository.fetchUser(id) }
            }.onSuccess { user ->
                _uiState.value = UiState.Success(user)
            }.onFailure { error ->
                _uiState.value = UiState.Error(error.message)
            }
        }
    }
}
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What is `Dispatchers.Main` in Android? | The main UI thread dispatcher. Used to update UI. All state updates to LiveData/StateFlow that trigger recomposition must happen here. |
| What is a Swift Actor? | A reference type that protects its mutable state from data races — only one task accesses it at a time. Replaces locks/serial queues with language-enforced safety. |
| What is a Coroutine vs a Thread? | A coroutine is a lightweight suspendable computation — thousands can run on a small thread pool. Threads are OS-level, expensive. Coroutines don't block threads when suspended. |
| What happens if you update UI from a background thread? | Crash or undefined behavior. iOS: `UIKit` is not thread-safe. Android: throws `CalledFromWrongThreadException`. Always dispatch UI updates to main thread. |

---

## 5. Navigation

### Navigation Patterns

| Pattern | iOS | Android |
|---|---|---|
| Push/Pop Stack | `UINavigationController`, SwiftUI `NavigationStack` | `FragmentManager` back stack, Compose `NavController` |
| Tab-Based | `UITabBarController`, SwiftUI `TabView` | `BottomNavigationView`, Compose `NavigationBar` |
| Modals | `present(vc, animated:)`, SwiftUI `.sheet` | `DialogFragment`, Compose `ModalBottomSheet` |
| Deep Linking | `UIApplicationDelegate openURL`, `onOpenURL` | Android Intent filters, Compose `NavDeepLink` |

```swift
// SwiftUI Navigation
struct AppView: View {
    var body: some View {
        NavigationStack {
            ContentView()
                .navigationDestination(for: User.self) { user in
                    UserDetailView(user: user)
                }
        }
    }
}
```

```kotlin
// Jetpack Compose Navigation
@Composable
fun AppNavigation(navController: NavHostController = rememberNavController()) {
    NavHost(navController = navController, startDestination = "home") {
        composable("home") { HomeScreen(navController) }
        composable("user/{userId}") { backStack ->
            val userId = backStack.arguments?.getString("userId")
            UserDetailScreen(userId = userId, navController = navController)
        }
    }
}
```

---

## 6. Data Binding & Reactive Programming

### Reactive Architecture Diagram

```mermaid
flowchart LR
    subgraph iOS["iOS — Combine"]
        MODEL_IOS["@Published property\nViewModel"] -->|"Publisher"| VIEW_IOS["View\n.onReceive / .sink"]
    end

    subgraph Android["Android — StateFlow / LiveData"]
        MODEL_AND["MutableStateFlow\nViewModel"] -->|"Flow stream"| VIEW_AND["Composable\n.collectAsState()"]
    end

    classDef ios fill:#0078D4,color:#fff
    classDef android fill:#22c55e,color:#fff
    class MODEL_IOS,VIEW_IOS ios
    class MODEL_AND,VIEW_AND android
```

| Framework | iOS | Android |
|---|---|---|
| Reactive Library | Combine (Apple), RxSwift (third-party) | Kotlin Flow, LiveData, RxJava |
| State Container | `@Published`, `@State`, `@ObservableObject` | `StateFlow`, `MutableState` (Compose) |
| Binding Primitive | `@Binding`, `$` two-way binding | `mutableStateOf`, `remember` |

```swift
// iOS Combine — ViewModel
class UserViewModel: ObservableObject {
    @Published var users: [User] = []
    @Published var isLoading = false
    private var cancellables = Set<AnyCancellable>()

    func loadUsers() {
        isLoading = true
        URLSession.shared.dataTaskPublisher(for: url)
            .map(\.data)
            .decode(type: [User].self, decoder: JSONDecoder())
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] _ in self?.isLoading = false },
                receiveValue: { [weak self] in self?.users = $0 }
            )
            .store(in: &cancellables)
    }
}
```

---

## 7. API Design & Networking

### Networking Stack

```mermaid
flowchart TD
    APP["App Layer\nViewModel / Repository"] --> HTTP["HTTP Client"]

    subgraph iOS_Net["iOS"]
        URLSession["URLSession\n(built-in)"]
        Alamofire["Alamofire\n(third-party)"]
    end

    subgraph Android_Net["Android"]
        OkHttp["OkHttp\n(core client)"]
        Retrofit["Retrofit\n(type-safe wrapper)"]
    end

    HTTP --> AUTH["Auth Interceptor\nBearer token injection"]
    AUTH --> RETRY["Retry Policy\nExponential backoff"]
    RETRY --> NETWORK["Network Layer"]

    classDef ios fill:#0078D4,color:#fff
    classDef android fill:#22c55e,color:#fff
    class URLSession,Alamofire ios
    class OkHttp,Retrofit android
```

### Retrofit Example (Android)

```kotlin
// API interface
interface UserApi {
    @GET("users/{id}")
    suspend fun getUser(@Path("id") id: String): UserDto

    @POST("users")
    suspend fun createUser(@Body request: CreateUserRequest): UserDto
}

// OkHttp client with auth interceptor
val okHttpClient = OkHttpClient.Builder()
    .addInterceptor { chain ->
        val request = chain.request().newBuilder()
            .addHeader("Authorization", "Bearer ${tokenManager.getToken()}")
            .build()
        chain.proceed(request)
    }
    .addInterceptor(HttpLoggingInterceptor().apply { level = Level.BODY })
    .build()

val retrofit = Retrofit.Builder()
    .baseUrl(BuildConfig.API_BASE_URL)
    .client(okHttpClient)
    .addConverterFactory(GsonConverterFactory.create())
    .build()
```

### Interview Talking Points

| Question | Answer |
|---|---|
| What is certificate pinning? | Hardcodes the expected server TLS certificate/public key in the app. Prevents MITM attacks even if a certificate authority is compromised. Must be updated before cert expiry. |
| What is exponential backoff? | On network failure, retry with increasing delays: 1s, 2s, 4s, 8s + jitter. Prevents thundering herd when all clients retry simultaneously after server recovery. |
| What is a circuit breaker in mobile? | After N consecutive failures, stop sending requests for a cool-down period. Protects backend and saves battery — fail fast instead of waiting for timeouts. |

---

## 8. Software Architecture Patterns

### Pattern Evolution

```mermaid
flowchart TD
    MVC["MVC — Model View Controller\nController mediates\nMassive ViewController problem"] -->
    MVP["MVP — Model View Presenter\nPassive View\nPresenter handles logic\nTestable"] -->
    MVVM["MVVM — Model View ViewModel\nBindings update View\nNo View ref in ViewModel\nMost popular"] -->
    MVI["MVI — Model View Intent\nUnidirectional Data Flow\nSingle immutable state\nPredictable"]

    CLEAN["Clean Architecture\nDomain → Data → Presentation\nDependency Rule\nUse cases"] --> VIPER["VIPER (iOS)\nView Interactor Presenter Entity Router\nStrict separation\nLarge teams"]

    classDef pattern fill:#8b5cf6,color:#fff
    class MVC,MVP,MVVM,MVI,CLEAN,VIPER pattern
```

### MVVM Implementation

```typescript
// React MVVM equivalent
// Model (data entity)
interface User { id: string; name: string; email: string; }

// ViewModel (state + business logic)
function useUserViewModel(userId: string) {
  const [user, setUser] = React.useState<User | null>(null);
  const [isLoading, setIsLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const loadUser = React.useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await userRepository.findById(userId);
      setUser(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error');
    } finally {
      setIsLoading(false);
    }
  }, [userId]);

  React.useEffect(() => { loadUser(); }, [loadUser]);
  return { user, isLoading, error, reload: loadUser };
}

// View (pure rendering, no business logic)
function UserProfileView({ userId }: { userId: string }) {
  const { user, isLoading, error } = useUserViewModel(userId);
  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage message={error} />;
  return <div>{user?.name}</div>;
}
```

### Architecture Pattern Comparison

| Pattern | Testability | Boilerplate | Best For |
|---|---|---|---|
| MVC | Hard (Massive VC) | Low | Small apps, rapid prototypes |
| MVP | Good (Passive View) | Medium | Classic Android XML apps |
| MVVM | Excellent (ViewModel testable) | Medium | Modern iOS/Android, React |
| MVI | Excellent (pure functions) | High | Complex state, Redux-style apps |
| Clean + VIPER | Excellent | Very High | Large teams, domain-rich apps |

---

## 9. GOF Design Patterns in Mobile

### Creational Patterns

| Pattern | Intent | Mobile Example |
|---|---|---|
| Singleton | One instance app-wide | `UserDefaults.standard`, `NetworkManager.shared` |
| Factory Method | Subclass decides which object to create | `ViewControllerFactory.makeProfile()` |
| Abstract Factory | Family of related objects | `ThemeFactory` → LightTheme / DarkTheme |
| Builder | Complex object construction step by step | `URLRequest` builder, `AlertDialog.Builder` |
| Prototype | Clone an existing object | Copying a configuration object with overrides |

### Structural Patterns

| Pattern | Intent | Mobile Example |
|---|---|---|
| Adapter | Bridge incompatible interfaces | Wrapping a REST client to match a repository protocol |
| Decorator | Add behavior by wrapping | Logging decorator around a network client |
| Facade | Simplified interface to complex subsystem | `AuthManager` wrapping Keychain + token refresh + biometric |
| Composite | Treat single objects and compositions uniformly | UIView hierarchy — view tree is a Composite |
| Proxy | Surrogate for another object | Image placeholder while loading — proxy for the real image |

### Behavioral Patterns

| Pattern | Intent | Mobile Example |
|---|---|---|
| Observer | Notify dependents of state changes | NotificationCenter, LiveData, Combine, Flow |
| Strategy | Swap algorithms at runtime | Authentication strategy — OTP vs Biometric vs Password |
| Command | Encapsulate a request as an object | Undo/redo actions, queued network requests |
| Template Method | Skeleton algorithm; subclasses fill in steps | `BaseViewController` with `setupUI()` / `bindViewModel()` hooks |
| Chain of Responsibility | Pass request through handler chain | Middleware — auth → logging → rate limit → actual handler |

### Code: Observer Pattern (iOS Combine)

```swift
// Publisher-Subscriber via Combine
class StockPriceService: ObservableObject {
    @Published var currentPrice: Double = 0.0
    private var timer: AnyCancellable?

    func startStreaming(symbol: String) {
        timer = Timer.publish(every: 1.0, on: .main, in: .common)
            .autoconnect()
            .sink { [weak self] _ in
                self?.currentPrice = Double.random(in: 150...160) // simulated
            }
    }
}

struct StockView: View {
    @ObservedObject var service: StockPriceService
    var body: some View {
        Text("$\(service.currentPrice, specifier: "%.2f")")
    }
}
```

---

## 10. Dependency Injection

### DI Architecture

```mermaid
flowchart TD
    subgraph iOS_DI["iOS — Swinject / Manual DI"]
        CONTAINER_IOS["DI Container\nSwinject Container"]
        CONTAINER_IOS -->|"resolve()"| VC["ViewController"]
        CONTAINER_IOS -->|"resolve()"| VM_IOS["ViewModel"]
        VM_IOS -->|"injected"| REPO_IOS["Repository"]
        REPO_IOS -->|"injected"| NET_IOS["NetworkClient"]
    end

    subgraph Android_DI["Android — Hilt / Dagger"]
        HILT["@HiltAndroidApp\nComponent Graph"]
        HILT -->|"@Inject"| VM_AND["@HiltViewModel\nViewModel"]
        VM_AND -->|"@Inject"| REPO_AND["Repository"]
        REPO_AND -->|"@Inject"| NET_AND["Retrofit"]
    end

    classDef ios fill:#0078D4,color:#fff
    classDef android fill:#22c55e,color:#fff
    class CONTAINER_IOS,VC,VM_IOS,REPO_IOS,NET_IOS ios
    class HILT,VM_AND,REPO_AND,NET_AND android
```

```kotlin
// Android Hilt DI
@HiltAndroidApp
class MyApplication : Application()

@Module
@InstallIn(SingletonComponent::class)
object NetworkModule {
    @Provides
    @Singleton
    fun provideRetrofit(): UserApi = Retrofit.Builder()
        .baseUrl(BuildConfig.API_URL)
        .build()
        .create(UserApi::class.java)
}

@HiltViewModel
class UserViewModel @Inject constructor(
    private val repository: UserRepository
) : ViewModel()
```

---

## 11. Data Storage

### Storage Decision Matrix

```mermaid
flowchart TD
    Q1{"Data Type?"} -->|"Simple key-value settings"| KV["UserDefaults (iOS)\nSharedPreferences (Android)"]
    Q1 -->|"Structured relational data"| DB["Core Data / SQLite (iOS)\nRoom / SQLite (Android)"]
    Q1 -->|"Sensitive credentials"| SEC["Keychain (iOS)\nEncryptedSharedPrefs / KeyStore (Android)"]
    Q1 -->|"Files, images"| FILE["FileManager (iOS)\nFile API (Android)"]
    Q1 -->|"Offline-first complex sync"| SYNC["Realm / WatermelonDB\nCloud sync (CloudKit / Firebase)"]

    classDef decision fill:#8b5cf6,color:#fff
    classDef solution fill:#22c55e,color:#fff
    class Q1 decision
    class KV,DB,SEC,FILE,SYNC solution
```

| Storage Type | iOS | Android | Use For |
|---|---|---|---|
| Key-Value | `UserDefaults` | `SharedPreferences` | Settings, flags, small values |
| Relational | Core Data, SQLite | Room (ORM), SQLite | Complex queries, relationships |
| Secure | Keychain | `EncryptedSharedPreferences`, Keystore | Tokens, passwords, biometric keys |
| File | `FileManager` | `File`, `Context.filesDir` | Media, documents, cache |
| Cloud Sync | CloudKit, iCloud | Firebase, Sync Manager | Cross-device sync |

```kotlin
// Room Database (Android)
@Entity(tableName = "users")
data class UserEntity(
    @PrimaryKey val id: String,
    val name: String,
    val email: String
)

@Dao
interface UserDao {
    @Query("SELECT * FROM users WHERE id = :id")
    suspend fun findById(id: String): UserEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(user: UserEntity)
}

@Database(entities = [UserEntity::class], version = 1)
abstract class AppDatabase : RoomDatabase() {
    abstract fun userDao(): UserDao
}
```

---

## 12. Observability & Testing

### Testing Pyramid

```mermaid
flowchart TD
    E2E["E2E Tests (top)\nXCUITest (iOS)\nEspresso / Detox (Android)\nSlow, expensive, full stack"]
    INT["Integration Tests (middle)\nXCTestCase with real DB\nRoom in-memory DB tests\nMedium cost"]
    UNIT["Unit Tests (base)\nXCTest (iOS)\nJUnit + Mockito (Android)\nFast, isolated, mocked deps"]

    UNIT --> INT --> E2E

    classDef e2e fill:#ef4444,color:#fff
    classDef int fill:#f59e0b,color:#fff
    classDef unit fill:#22c55e,color:#fff
    class E2E e2e
    class INT int
    class UNIT unit
```

| Tool | Platform | Type |
|---|---|---|
| XCTest | iOS | Unit + UI testing framework |
| Quick + Nimble | iOS | BDD-style specs |
| Espresso | Android | UI automation (in-process) |
| JUnit 4/5 + Mockito | Android | Unit tests |
| Detox | Cross-platform | E2E gray-box testing |

```kotlin
// Android Unit Test — ViewModel with coroutines
@OptIn(ExperimentalCoroutinesApi::class)
class UserViewModelTest {
    @get:Rule
    val coroutineRule = MainCoroutineRule()

    private val repository = mockk<UserRepository>()
    private lateinit var viewModel: UserViewModel

    @Before
    fun setup() {
        viewModel = UserViewModel(repository)
    }

    @Test
    fun `loads user successfully`() = runTest {
        val expected = User("1", "Alice", "alice@example.com")
        coEvery { repository.findById("1") } returns expected

        viewModel.loadUser("1")

        assertEquals(UiState.Success(expected), viewModel.uiState.value)
    }
}
```

---

## 13. Privacy & Security

### Security Architecture

```mermaid
flowchart TD
    subgraph Auth["Authentication"]
        BIO["Biometric\nFace ID / Touch ID\nFingerprint"]
        MFA["MFA\nOTP + biometric"]
        OAUTH["OAuth 2.0\nSocial login, SSO"]
    end

    subgraph Storage["Secure Storage"]
        KC["Keychain (iOS)\nHardware-backed encryption"]
        KS["KeyStore (Android)\nTEE / Strongbox"]
    end

    subgraph Network["Network Security"]
        PIN["Certificate Pinning\nPrevents MITM"]
        TLS["TLS 1.3\nHTTPS only"]
    end

    subgraph Data["Data Protection"]
        ENC["AES-256 Encryption\nSensitive data at rest"]
        ANON["Data Anonymization\nRemove PII before analytics"]
    end

    classDef auth fill:#22c55e,color:#fff
    classDef storage fill:#8b5cf6,color:#fff
    classDef network fill:#1e40af,color:#fff
    classDef data fill:#f59e0b,color:#fff
    class BIO,MFA,OAUTH auth
    class KC,KS storage
    class PIN,TLS network
    class ENC,ANON data
```

### Interview Talking Points

| Question | Answer |
|---|---|
| Where should you store auth tokens on iOS? | Keychain — hardware-backed, encrypted, survives app reinstall. Never `UserDefaults` (unencrypted, accessible in backups). |
| Where should you store auth tokens on Android? | `EncryptedSharedPreferences` (uses Android Keystore under the hood) or directly in Keystore with a symmetric key. |
| What is App Transport Security (ATS)? | iOS policy requiring HTTPS for all network connections (unless explicitly exempted). Enforced since iOS 9. |
| What is biometric authentication flow? | Use `LocalAuthentication` (iOS) / `BiometricPrompt` (Android) to authenticate. On success, retrieve the key from Keychain/Keystore to decrypt data or authenticate API calls. |
| What is jailbreak/root detection? | Checking if device is jailbroken (iOS) or rooted (Android). Jailbreaking bypasses OS security — sensitive apps (banking) refuse to run on compromised devices. |

---

## 14. Advanced Topics

### On-Device ML

| Platform | Framework | Use Case |
|---|---|---|
| iOS | CoreML | Object detection, NLP, image classification |
| iOS | Create ML | Training custom models on device |
| Android | ML Kit (Firebase) | Text recognition, face detection, barcode scanning |
| Cross-platform | TFLite (TensorFlow Lite) | Small models optimized for mobile inference |

### AR & VR

| Platform | SDK | Features |
|---|---|---|
| iOS | ARKit | Plane detection, face tracking, LiDAR depth |
| Android | ARCore | Motion tracking, environmental understanding |
| Cross-platform | Unity AR Foundation | Single codebase for iOS + Android AR |
| Vision Pro | RealityKit | Spatial computing, mixed reality |

### Server-Driven UI (SDUI)

```mermaid
flowchart LR
    SERVER["Backend\nReturns UI description as JSON\n{type: 'Button', label: 'Buy', action: 'checkout'}"]
    -->|"JSON payload"| CLIENT["Client Renderer\nInterprets JSON → native components"]
    CLIENT --> NATIVE["Renders native\nUIButton / Compose Button"]

    classDef server fill:#8b5cf6,color:#fff
    classDef client fill:#22c55e,color:#fff
    class SERVER server
    class CLIENT,NATIVE client
```

SDUI benefits: update UI without app release, A/B test layouts server-side, single source of truth for all clients.

### Cross-Platform Frameworks

| Framework | Language | Approach | Performance |
|---|---|---|---|
| React Native | JavaScript/TypeScript | JS bridge → native components | Good (JSI in new arch) |
| Flutter | Dart | Custom rendering engine (Skia/Impeller) | Excellent |
| Xamarin | C# | Compiled to native | Good |
| KMM (Kotlin Multiplatform) | Kotlin | Shared business logic, native UI | Excellent (native UI) |

---

## 15. Interview Strategy

### Mobile System Design Interview Framework

```mermaid
flowchart TD
    CLARIFY["1. Clarify Requirements\n5 minutes\nFunctional scope\nNon-functional (offline? real-time?)"]
    NFR["2. Non-Functional Requirements\nPerformance, Security, Battery\nData volume, User scale"]
    ARCH["3. High-Level Architecture\nChoose pattern (MVVM/MVI)\nIdentify key components"]
    DATA["4. Data Model\nEntities, storage choice\nOffline sync strategy"]
    API["5. API & Interfaces\nEndpoints, protocols\nPagination strategy"]
    DEEP["6. Deep Dive\nOne area in depth\nAlgorithm, caching, real-time"]
    TRADE["7. Trade-offs\nAcknowledge alternatives\nExplain your choices"]

    CLARIFY --> NFR --> ARCH --> DATA --> API --> DEEP --> TRADE

    classDef step fill:#8b5cf6,color:#fff
    class CLARIFY,NFR,ARCH,DATA,API,DEEP,TRADE step
```

### Common Trade-off Questions

| Trade-off | When to choose A | When to choose B |
|---|---|---|
| REST vs GraphQL | Simple stable APIs, RESTful semantics | Multiple clients with different data needs |
| Native vs Cross-platform | Max performance, deep platform features | Code sharing, limited iOS/Android teams |
| WebSocket vs SSE | Bidirectional (chat, gaming) | Server-only push (notifications, feeds) |
| Limit-Offset vs Cursor | Simple admin dashboards | High-scale feeds, infinite scroll |
| CoreData vs Realm | App-only data, CloudKit sync | Complex queries, faster migrations |

### Architecture Decision Template

When asked "How would you design X?":
1. **Clarify** — "Is offline support needed? Expected user scale?"
2. **State the pattern** — "I'd use MVVM because ViewModels survive rotation and are easily testable"
3. **Identify layers** — Presentation → Domain → Data
4. **Data flow** — "User action → ViewModel → Repository → API/DB → StateFlow → UI recompose"
5. **Trade-offs** — "MVVM is more boilerplate than MVC but the testability benefit justifies it at this scale"

### Red Flags to Avoid in Interviews

| Red Flag | Better Answer |
|---|---|
| "I'd put everything in the ViewController" | Describe MVVM/VIPER — logic belongs in ViewModel/Interactor |
| "I'd use global singletons for state" | Use scoped DI containers, ViewModel scoping |
| "I'd reload everything on app resume" | Implement smart refresh — check timestamp, only fetch if stale |
| "Pagination isn't needed, just load all data" | Always design for pagination — mobile bandwidth and memory are limited |
| "I'd store tokens in UserDefaults" | Keychain (iOS) / EncryptedSharedPreferences (Android) |

---

*Mobile System Design — Complete Guide | Generated July 2026*
