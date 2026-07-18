# Angular — Advanced Topics

---

## Angular Application Bootstrapping Process

**Flow:** `Angular.json → Main.ts → App.Module.ts → App.Component → Index.html`

### 1. Angular.json
Configuration file read first by the builder. The `main` property points to `src/main.ts`.

### 2. Main.ts
```ts
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
platformBrowserDynamic().bootstrapModule(AppModule);
```

### 3. App.Module.ts
```ts
@NgModule({
  declarations: [AppComponent, TestComponent],
  imports: [BrowserModule, FormsModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

### 4. App.Component + Index.html
Component has selector `<app-root>` used in `index.html`.

---

## State Management in Angular — NgRx

NgRx is a framework for building reactive applications in Angular, based on the **Redux** pattern (unidirectional data flow).

**Application State** = data received via API calls, user inputs, UI state, preferences.

### NgRx 5 Components

| Component | Role |
|-----------|------|
| **Store** | Holds the entire application state |
| **Action** | Unique event describing how state should change (e.g., `AddCustomer`) |
| **Reducer** | Pure function that creates new immutable state based on the action |
| **Selector** | Function to retrieve a specific slice of state from the store |
| **Effect** | Handles side effects (API calls); listens to actions, returns new actions |

### Redux Core Concepts
- **Actions** — JavaScript objects with a `type` property describing the state change.
- **Store** — Holds all app state.
- **Reducers** — Run when the store dispatches an action; update state.

---

## Route Guards in Angular

Route Guards prevent unauthorized access to specific routes.

### 4 Types

1. **CanActivate** — Guards route activation.
2. **CanActivateChild** — Guards child routes.
3. **CanDeactivate** — Handles navigation away (e.g., unsaved form changes).
4. **CanLoad** — Prevents lazy-loaded module from loading.

```ts
export class PermissionGuard implements CanActivate {
  canActivate(route: ActivatedRouteSnapshot): boolean {
    return true; // custom logic
  }
}
```

> **Note:** `CanActivate` is only called once when child routes change.

---

## Lazy Loading in Angular

Lazy loading = loading NgModules on demand to reduce initial bundle size.

### Steps

```bash
# S1: Create module with routing
ng g m lazy-loading --routing

# S2: Create component
ng g c lazy-demo
```

```ts
// app-routing.module.ts (S4)
{
  path: 'lazy-loading',
  loadChildren: () => import('./lazy-loading/lazy-loading.module').then(m => m.LazyLoadingModule)
}
```

```ts
// lazy-loading-routing.module.ts (S5)
const routes: Routes = [{ path: '', component: LazyDemoComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
```

### Benefits
- **Faster initial load** — only essential modules loaded upfront.
- **Reduced bundle size** — especially beneficial on mobile.
- **Improved performance** — fewer resources to manage.
- **Enhanced modularity** — encourages clean, independent modules.

---

## Eager Loading vs Lazy Loading vs Pre-Loading

| Strategy | When Modules Load | Use Case |
|----------|------------------|----------|
| **Eager Loading** | Before app starts | Core modules, startup features |
| **Lazy Loading** | On demand | Rarely-accessed features |
| **Pre-Loading** | In background after app starts | Features likely needed soon |

---

## Angular Router: `children` vs `loadChildren`

| Property | Description |
|----------|-------------|
| `path` | URL fragment(s) the route matches |
| `component` | Component rendered for the route |
| `children` | Nested routes — loaded eagerly |
| `loadChildren` | Nested routes — loaded lazily |

---

## Pipes

Pipes transform output representation of values in templates.

```html
{{ NAMEVAL | titlecase }}  <!-- "raja" → "Raja" -->
```

Pipes support chaining:
```html
{{ value | pipe1 | pipe2 }}
```

### Built-in Pipes

| Pipe | Description |
|------|-------------|
| `date` | Format dates |
| `titlecase` / `uppercase` / `lowercase` | String case formatting |
| `currency` | Currency formatting |
| `json` | Convert object to JSON string |

```html
<pre>{{ myObject | json }}</pre>
```

---

## Custom Pipes

```ts
@Pipe({ name: 'custom' })
export class CustomPipe implements PipeTransform {
  transform(value: any, ...args: any[]): any {
    return transformedValue;
  }
}
```

```html
<p>{{ 'World' | custom:'Hello' }}</p>
```

---

## Pure vs Impure Pipes

```ts
@Pipe({ name: 'filterPipe', pure: false })
```

| Type | When Executed |
|------|---------------|
| **Pure** | Only on a "pure change" to input value or parameters |
| **Impure** | Every change detection cycle (bad for performance) |

---

## Local Storage vs Session Storage vs Cache Storage

| Storage | Capacity | Accessible From | Expiry |
|---------|----------|-----------------|--------|
| **Local Storage** | 10 MB | Any window | Never auto expires |
| **Session Storage** | 5 MB | Same tab only | Expires on tab close |
| **Cache Storage** | 5 KB | Any window | Must be manually set |

---

## RxJS Deep Dive

### Key Concepts

| Concept | Description |
|---------|-------------|
| **Stream** | A sequence of data from any source |
| **Observable** | A function that returns a stream over time |
| **Observer** | An object that receives notifications from an Observable |
| **Operators** | Functions to transform/filter Observable streams |

**Observable Lifecycle:** `Creation → Subscription → Execution → Destruction`

```ts
const observable = Observable.create((observer: any) => {
  observer.next('Hello World');
});

observable.subscribe((message: any) => console.log(message));
```

### Observer Callbacks

- `next(value)` — Called when a new value is emitted.
- `error(error)` — Called on error; nothing else delivered after this.
- `complete()` — Called when Observable is done emitting.

### Destroying an Observable

```ts
subscription.unsubscribe();
```

RxJS auto-unsubscribes after error or complete notifications.

---

## Promises

A promise represents the eventual completion or failure of an async operation.

**States:** `Pending → Fulfilled | Rejected`

```ts
const promise = new Promise((resolve, reject) => {
  setTimeout(() => {
    if (error) reject('error');
    else resolve('done');
  }, 1000);
});

promise.then(val => console.log(val), err => console.error(err));
```

---

## Observable vs Promise

| Feature | Observable | Promise |
|---------|-----------|---------|
| Values | Multiple over time | Single value |
| Execution | Lazy | Eager |
| Cancellation | Yes | No |
| State change | Can be cancelled | Resolved/Rejected is final |

---

## Subject vs BehaviourSubject vs ReplaySubject

### Subject — No memory for late subscribers

```ts
let mySubject = new Subject<number>();
mySubject.subscribe(x => console.log("First: " + x));
mySubject.next(1); mySubject.next(2); mySubject.next(3);
mySubject.subscribe(x => console.log("Second: " + x));
mySubject.next(4);
// Output: First: 1, First: 2, First: 3, First: 4, Second: 4
```

### ReplaySubject — Holds all previous values

```ts
let mySubject = new ReplaySubject<number>();
// Second subscriber receives ALL previous values (1, 2, 3) upon subscribing
```

### BehaviourSubject — ReplaySubject with buffer size 1

Requires an initial value. Late subscribers receive the **last emitted value**.

```ts
let mySubject = new BehaviorSubject<number>(1);
// Second subscriber receives "3" (current value) upon subscribing
```

### Comparison

| Feature | Subject | BehaviourSubject |
|---------|---------|-----------------|
| Initial value required | No | Yes |
| Current value on subscribe | No | Yes (last value) |
| Memory | None | Last value only |

---

## Calling a Service from Angular

```ts
// S1: Import in module
import { HttpClientModule } from '@angular/common/http';

// S2: Create service
@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  getPosts(): Observable<any[]> {
    return this.http.get<any[]>('https://api.example.com/posts');
  }
}

// S3: Use in component
ngOnInit() {
  this.apiService.getPosts().subscribe((data: any[]) => {
    this.posts = data;
  });
}
```

---

## Calling Multiple APIs in Parallel — forkJoin

```ts
import { forkJoin } from 'rxjs';

callApis() {
  const api1 = this.http.get('https://api.example.com/endpoint1');
  const api2 = this.http.get('https://api.example.com/endpoint2');
  const api3 = this.http.get('https://api.example.com/endpoint3');

  forkJoin([api1, api2, api3]).subscribe(results => {
    const [response1, response2, response3] = results;
    console.log('API 1:', response1);
  });
}
```

`forkJoin` waits for all observables to complete before emitting.

---

## Exception Handling in Angular

```ts
async fetchData() {
  try {
    const response = await this.http.get('https://api.example.com/data').toPromise();
    console.log('Data:', response);
  } catch (error) {
    console.error('Error:', error);
  } finally {
    console.log('Fetch attempt finished.');
  }
}
```

---

## Array Operations

### Map (Select)
```ts
let modifiedArray = this.users.map(user => ({
  username: user.username,
  name: user.name
}));
```

### Filter (Where)
```ts
const filtered = data.filter(value => value > 5);
```
