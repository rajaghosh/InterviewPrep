# Angular Interview Prep

---

## Polyfill 1

In Angular, a polyfill is a piece of code that allows our application to run in older version browsers that do not support certain features.

`Polyfills.ts` is used for browser support. It loads a set of libraries which are used in the application and are not supported by browsers by default.

```bash
npm install core-js
```

---

## Package.json vs Angular.json

- **Package.json** — holds all of the `npm` packages installed for the project.
- **Angular.json** — holds the configuration for the project.

---

## How Angular Application Loads into the Browser

**Flow:** `Angular.json → Main.ts → App.Module.ts → App.Component → Index.html`

### 1. Angular.json
The first file referenced by the Angular builder. The `main` entry points to `src/main.ts`.

```json
"options": {
  "outputPath": "dist/hello-world",
  "index": "src/index.html",
  "main": "src/main.ts",
  "polyfills": "src/polyfills.ts"
}
```

### 2. Main.ts — Bootstrapping starts here
Entry point of the application. Creates the browser environment.

```ts
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
platformBrowserDynamic().bootstrapModule(AppModule);
```

### 3. App.Module.ts
Decorated with `@NgModule`. Declares all components Angular should be aware of.

```ts
@NgModule({
  declarations: [AppComponent, TestComponent],
  imports: [BrowserModule, FormsModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

### 4. App.Component.ts + App.Component.html
Decorated with `@Component`. Has a selector (`app-root`) and a template.

### 5. Index.html
Uses the `<app-root>` selector to load the component content.

---

## Subject vs BehaviourSubject vs ReplaySubject vs AsyncSubject

| Type | Behaviour |
|------|-----------|
| **Subject** | Each subscriber receives only upcoming values |
| **BehaviourSubject** | Each subscriber receives one previous value + upcoming values |
| **ReplaySubject** | Each subscriber receives all previous values + upcoming values |
| **AsyncSubject** | Emits the latest value when stream closes |

- `BehaviorSubject` keeps in memory the last emitted value (like `ReplaySubject` with buffer size 1).
- `Subject` does not hold a value; `BehaviorSubject` requires an initial value.

---

## Angular Route Guards

Route Guards prevent unauthorized access to a specific route.

> **Note:** Add route guard service instances to `NgModule` or `AppModule` in the `providers` array.

### Types of Route Guards

1. **CanActivate** — Controls whether a route can be activated.
2. **CanActivateChild** — Guards child routes.
3. **CanDeactivate** — Handles navigation away from a route (e.g., unsaved changes).
4. **CanLoad** — Prevents loading lazy-loaded modules conditionally.

```ts
export class PermissionGuard implements CanActivate {
  canActivate(route: ActivatedRouteSnapshot): boolean {
    // custom logic
    return true;
  }
}
```

---

## Directives

Directives are special markers in HTML that tell Angular to do something with a DOM element.

### Types of Directives

| Type | Description | Examples |
|------|-------------|---------|
| **Component** | Directive with a template | `@Component` |
| **Attribute** | Modifies appearance/behavior | `ngClass`, `ngStyle` |
| **Structural** | Modifies DOM structure | `*ngIf`, `*ngFor` |

```html
<!-- Attribute Directive -->
<div [ngClass]="{'active': isActive}">...</div>

<!-- Structural Directive -->
<li *ngFor="let item of items">{{ item }}</li>
```

### Custom Directive Example

```ts
@Directive({ selector: '[appHighlight]' })
export class HighlightDirective {
  constructor(private el: ElementRef, private renderer: Renderer2) {}

  @HostListener('mouseenter') onMouseEnter() {
    this.renderer.setStyle(this.el.nativeElement, 'background-color', 'yellow');
  }

  @HostListener('mouseleave') onMouseLeave() {
    this.renderer.removeStyle(this.el.nativeElement, 'background-color');
  }
}
```

---

## Updating Angular Version

```bash
ng update
ng update --force
```

---

## RxJS Library

RxJS (Reactive Extensions for JavaScript) handles **asynchronous** data streams.

### Key Concepts

1. **Observable** — Represents a data stream; lazy (won't emit until subscribed).

```ts
const observable = new Observable(observer => {
  observer.next(1);
  observer.next(2);
  observer.complete();
});
```

2. **Observer** — Watches values emitted by an Observable.

```ts
const observer = {
  next: value => console.log(`Next: ${value}`),
  error: error => console.error(`Error: ${error}`),
  complete: () => console.log('Complete'),
};
```

3. **Subscribing** — Connects Observer to Observable.

```ts
const subscription = observable.subscribe(observer);
subscription.unsubscribe();
```

4. **Operators** — Transform/filter streams (`map`, `filter`, `switchMap`, etc.)

   | LINQ | RxJS |
   |------|------|
   | Select | map |
   | Where | filter |
   | OrderBy | sort |
   | FirstOrDefault | find |

5. **Async Pipe** — Automatically subscribes/unsubscribes in templates.

---

## Observable vs Promise

| Feature | Observable | Promise |
|---------|-----------|---------|
| Values | Multiple over time | Single value |
| Execution | Lazy (starts on subscribe) | Eager (starts immediately) |
| Cancellation | Yes (unsubscribe) | No |
| Chaining | Yes | Limited |
| Use Case | Continuous streams, events | One-time async operations |

---

## Angular Lifecycle Hooks

```
Constructor → ngOnChanges → ngOnInit → ngDoCheck
  → ngAfterContentInit → ngAfterContentChecked
  → ngAfterViewInit → ngAfterViewChecked
→ ngOnDestroy
```

| Hook | When Called |
|------|-------------|
| `constructor` | When component/directive is created |
| `ngOnChanges` | When `@Input()` property changes (parent-child) |
| `ngOnInit` | Once, after component initialization |
| `ngDoCheck` | Every change detection cycle |
| `ngOnDestroy` | Just before component is destroyed |
| `ngAfterContentInit` | After content projection |
| `ngAfterViewInit` | After component view + child views initialized |

---

## Data Binding

| Syntax | Type | Direction |
|--------|------|-----------|
| `{{ expression }}` / `[target]="expr"` | Property Binding | Data Source → View |
| `(target)="statement"` | Event Binding | View → Data Source |
| `[(target)]="expression"` | Two-way Binding | Both directions |

---

## Inter-Component Communication

### Parent → Child: `@Input`

```ts
// Child component
@Input() uNameChild: string;
```
```html
<!-- Parent template -->
<child [uNameChild]="userNameParent"></child>
```

### Child → Parent: `@Output` + `EventEmitter`

```ts
// Child component
@Output() notifyObj = new EventEmitter<string>();
GetVal() { this.notifyObj.emit("MESSAGE"); }
```
```html
<!-- Parent template -->
<child (notifyObj)="parentMethod($event)"></child>
```

### Sibling Communication
Use a **shared service**.

---

## Angular Decorators

| Decorator | Purpose |
|-----------|---------|
| `@Component` | Define a component |
| `@Directive` | Create custom directives |
| `@Injectable` | Mark a class as injectable service |
| `@NgModule` | Define an Angular module |
| `@Input` / `@Output` | Data flow between components |
| `@ViewChild` / `@ViewChildren` | Access child elements/components |

---

## @ViewChild vs @ViewChildren

- **`@ViewChild`** — Access a single child element/component (first match).
- **`@ViewChildren`** — Access multiple child elements/components (returns `QueryList`).

Both are accessed in `ngAfterViewInit`.

---

## Template Reference Variable

```html
<input #myInput>
<button (click)="logInputValue(myInput.value)">Log</button>
```

---

## JIT vs AOT Compilation

| Feature | JIT (Just-in-Time) | AOT (Ahead-of-Time) |
|---------|-------------------|---------------------|
| When compiled | At runtime in browser | During build |
| Bundle size | Larger | Smaller |
| Performance | Slightly slower startup | Faster startup |
| Use case | Development | Production |

```bash
ng build          # JIT (default)
ng build --aot    # AOT
ng build --prod   # Production optimizations (does NOT enable AOT by itself)
ng build --aot --prod --output-path=dist/my-app
```

---

## Angular Routing

```bash
ng new my-app --routing
```

```ts
const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'about', component: AboutComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
```

```html
<a routerLink="/">Home</a>
<a routerLink="/about">About</a>
<router-outlet></router-outlet>
```

---

## Lazy Loading in Angular

Lazy loading = loading modules on demand.

**Steps:**
1. Divide project into different modules.
2. Use `loadChildren` in the main module route file.
3. Use `forRoot` in the main module route, `forChild` in child module routes.

```ts
// app-routing.module.ts
{
  path: 'lazy-loading',
  loadChildren: () => import('./lazy-loading/lazy-loading.module').then(m => m.LazyLoadingModule)
}
```

```ts
// lazy-loading-routing.module.ts
const routes: Routes = [{ path: '', component: LazyDemoComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
```

---

## Constructor vs ngOnInit

| | Constructor | ngOnInit |
|--|-------------|----------|
| Type | TypeScript/OOP | Angular lifecycle hook |
| When | Before Angular initializes | After Angular initializes + inputs set |
| Use for | DI injection, basic setup | Angular-specific initialization, async ops |

---

## Guards and Interceptors

### Guard (Route Access Control)

```ts
export class AuthGuardService implements CanActivate {
  canActivate(): boolean {
    return /* custom logic */;
  }
}
```

```ts
// In routing module
{ path: 'MY_PATH', component: MyComponent, canActivate: [AuthGuardService] }
```

### Interceptor (HTTP Request Middleware)

```ts
export class AuthInterceptorService implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const authReq = this.AddTokenHeader(req, 'JWT_TOKEN');
    return next.handle(authReq).pipe(
      catchError(errordata => {
        if (errordata.status == 401) { /* clear localStorage */ }
        return throwError(errordata);
      })
    );
  }
}
```

```ts
// Register in module providers
{ provide: HTTP_INTERCEPTORS, useClass: AuthInterceptorService, multi: true }
```

---

## Router vs ActivatedRoute

- **Router** — Navigates between views based on URL.
- **ActivatedRoute** — Provides access to info about the current route (path, URL params, query params).

---

## Interpolation vs Property Binding

Both display data in templates, but are suited to different scenarios.

| | Interpolation `{{ }}` | Property Binding `[prop]` |
|--|----------------------|--------------------------|
| Syntax | `{{ expression }}` | `[property]="expression"` |
| Output | Converted to string | Passes any type (boolean, object) |
| Use for | Text content | DOM properties, attributes |
| Null | Renders empty string | Passes `null` |

```html
<!-- Interpolation — renders text -->
<h1>{{ title }}</h1>
<p>Welcome, {{ user.name }}</p>

<!-- Property Binding — binds to DOM property -->
<img [src]="imageUrl" />
<button [disabled]="isLoading">Submit</button>
<input [value]="searchTerm" />
```

> Use interpolation for text. Use property binding when the target expects a non-string value or when you need to set a DOM property (not attribute).

---

## Content Projection — `<ng-content>`

Content projection lets a parent component inject HTML into a child component's template.

```ts
// child.component.ts
@Component({
  selector: 'app-card',
  template: `
    <div class="card">
      <div class="header"><ng-content select="[header]"></ng-content></div>
      <div class="body"><ng-content></ng-content></div>
      <div class="footer"><ng-content select="[footer]"></ng-content></div>
    </div>
  `
})
export class CardComponent {}
```

```html
<!-- parent template -->
<app-card>
  <h2 header>Card Title</h2>
  <p>Card body content goes here.</p>
  <button footer>OK</button>
</app-card>
```

- `<ng-content>` — projects all content.
- `<ng-content select="[attr]">` — projects only matching elements (multi-slot projection).
- Enables reusable container components (cards, modals, panels).

---

## Standalone Components (Angular 14+)

Standalone components don't need to be declared in any `NgModule`.

```ts
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],  // imports declared here instead of NgModule
  template: `<p>Profile works!</p>`
})
export class ProfileComponent {}
```

### Bootstrap standalone app

```ts
// main.ts
import { bootstrapApplication } from '@angular/platform-browser';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient()
  ]
});
```

### Lazy loading a standalone component

```ts
{
  path: 'profile',
  loadComponent: () =>
    import('./profile/profile.component').then(m => m.ProfileComponent)
}
```

**Benefits:** Less boilerplate, tree-shakeable, simpler testing, no NgModule needed.

---

## Singleton Services — `providedIn: 'root'`

By default, Angular creates **one instance** of a service per injector.

```ts
@Injectable({
  providedIn: 'root'  // registered in the root injector → singleton across the whole app
})
export class AuthService {
  private currentUser: User | null = null;
}
```

**How it works:**
- `providedIn: 'root'` registers the service in the **root injector**.
- The same instance is shared by all components and services that inject it.
- Tree-shakeable — if not injected anywhere, it is excluded from the bundle.

```ts
// Scoped to a feature module (not singleton across app)
@Injectable({
  providedIn: FeatureModule
})
export class FeatureScopedService {}
```

---

## `let` vs `var` (TypeScript / JavaScript)

| | `var` | `let` |
|--|-------|-------|
| Scope | Function-scoped | Block-scoped |
| Hoisting | Hoisted + initialized as `undefined` | Hoisted but NOT initialized (TDZ) |
| Re-declaration | Allowed in same scope | Not allowed in same scope |
| Loop | Shares one binding across all iterations | New binding per iteration |

```ts
// var — function scope
function example() {
  for (var i = 0; i < 3; i++) { }
  console.log(i);  // 3 — i leaks out of the for block
}

// let — block scope
function example2() {
  for (let j = 0; j < 3; j++) { }
  // console.log(j);  // ReferenceError — j not accessible here
}

// Classic closure bug with var
for (var k = 0; k < 3; k++) {
  setTimeout(() => console.log(k), 0);  // prints 3, 3, 3
}

// Fixed with let
for (let k = 0; k < 3; k++) {
  setTimeout(() => console.log(k), 0);  // prints 0, 1, 2
}
```

> Always prefer `const` for values that won't change, `let` for variables that will. Avoid `var`.

---

## Spread Operator (`...`)

Expands an iterable (array, object) into its individual elements.

```ts
// Arrays — combine / clone
const a = [1, 2, 3];
const b = [4, 5, 6];
const combined = [...a, ...b];          // [1, 2, 3, 4, 5, 6]
const copy = [...a];                    // shallow clone

// Arrays — pass as function arguments
function sum(x: number, y: number, z: number) { return x + y + z; }
sum(...a);  // sum(1, 2, 3) = 6

// Objects — merge / override
const defaults = { theme: 'light', lang: 'en' };
const user = { lang: 'fr', name: 'Alice' };
const config = { ...defaults, ...user };
// { theme: 'light', lang: 'fr', name: 'Alice' }  — user.lang overrides defaults.lang

// Objects — immutable state update (common in NgRx reducers)
const state = { count: 0, loading: false };
const newState = { ...state, count: state.count + 1 };
```

---

## Detect Route Change

Subscribe to `Router.events` to react when navigation occurs.

```ts
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({ selector: 'app-root', template: `<router-outlet></router-outlet>` })
export class AppComponent implements OnInit {
  constructor(private router: Router) {}

  ngOnInit() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      console.log('Navigated to:', event.urlAfterRedirects);
      // track analytics, reset scroll, update breadcrumbs, etc.
    });
  }
}
```

**Router event types in order:**
1. `NavigationStart` — navigation begins
2. `RoutesRecognized` — URL matched a route
3. `GuardsCheckStart` / `GuardsCheckEnd` — route guards evaluated
4. `ResolveStart` / `ResolveEnd` — resolvers run
5. `NavigationEnd` — navigation complete
6. `NavigationCancel` / `NavigationError` — navigation stopped

---

## Sanitization — DomSanitizer

Angular sanitizes all values inserted into the DOM to prevent **XSS attacks**. Use `DomSanitizer` only when you trust the content.

```ts
import { DomSanitizer, SafeHtml, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-safe',
  template: `
    <div [innerHTML]="safeHtml"></div>
    <a [href]="safeUrl">Download</a>
    <iframe [src]="safeResource"></iframe>
  `
})
export class SafeComponent {
  safeHtml: SafeHtml;
  safeUrl: SafeUrl;
  safeResource: any;

  constructor(private sanitizer: DomSanitizer) {
    // Only use when content is genuinely trusted
    this.safeHtml = sanitizer.bypassSecurityTrustHtml('<b>Bold content</b>');
    this.safeUrl = sanitizer.bypassSecurityTrustUrl('javascript:void(0)');
    this.safeResource = sanitizer.bypassSecurityTrustResourceUrl('https://youtube.com/embed/abc');
  }
}
```

**Security types:**

| Method | Use for |
|--------|---------|
| `bypassSecurityTrustHtml` | `[innerHTML]` |
| `bypassSecurityTrustStyle` | `[style]` |
| `bypassSecurityTrustScript` | `<script>` src |
| `bypassSecurityTrustUrl` | `[href]`, `[src]` |
| `bypassSecurityTrustResourceUrl` | `<iframe [src]>`, `<object>` |

> Never bypass sanitization on user-generated content.

---

## Form Builder

`FormBuilder` is a helper service that reduces boilerplate when creating Reactive Forms.

```ts
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-register',
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <input formControlName="email" placeholder="Email" />
      <span *ngIf="form.get('email')?.errors?.['required']">Email required</span>

      <input type="password" formControlName="password" />
      <span *ngIf="form.get('password')?.errors?.['minlength']">Min 8 chars</span>

      <button type="submit" [disabled]="form.invalid">Register</button>
    </form>
  `
})
export class RegisterComponent {
  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      address: this.fb.group({       // nested form group
        city: [''],
        zip: ['']
      })
    });
  }

  onSubmit() {
    if (this.form.valid) {
      console.log(this.form.value);
    }
  }
}
```

**Without FormBuilder** (more verbose):
```ts
this.form = new FormGroup({
  email: new FormControl('', [Validators.required, Validators.email]),
  password: new FormControl('', [Validators.required, Validators.minLength(8)])
});
```

**`FormBuilder` methods:**
- `fb.group({...})` — creates a `FormGroup`
- `fb.control(value, validators)` — creates a `FormControl`
- `fb.array([...])` — creates a `FormArray` (dynamic list of controls)
