# Session, State, Security Management — ASP.NET Core

---

## Session Management

### Why Sessions?

HTTP is **stateless** by default — each request knows nothing about the previous one. Sessions allow the server to maintain user-specific data across multiple HTTP requests.

### How Sessions Work

1. Client sends HTTP Request to Server.
2. Server processes the request and generates a response. It creates a **SessionId** and sends it back in the response header as a **cookie**. The SessionId is also stored in the server cache.
3. For all subsequent requests (while session is active), the client sends the cookie (containing the SessionId) in the request header.
4. The server reads the SessionId from the cookie, fetches the associated data from cache, and processes the request.

---

## Setting Up Session in ASP.NET Core

### Required Package
```
Microsoft.AspNetCore.Session
```
(Included implicitly by the framework)

### Configuration in `Program.cs`

```csharp
// 1. Register a distributed cache
builder.Services.AddDistributedMemoryCache();

// 2. Configure session options
builder.Services.AddSession(options => {
  options.IdleTimeout = TimeSpan.FromMinutes(20);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

// 3. Register session middleware (after Routing, before MVC)
app.UseSession();
```

### Reading and Writing Session Data

```csharp
// Set session data
HttpContext.Session.SetString(SessionName, "Jarvik");
HttpContext.Session.SetInt32(SessionAge, 24);

// Get session data
string name = HttpContext.Session.GetString(SessionName);
int? age = HttpContext.Session.GetInt32(SessionAge);
```

---

## Session Options

| Option | Description | Default |
|--------|-------------|---------|
| `Cookie` | Settings for the session cookie | |
| `IdleTimeout` | How long a session can be idle before its cache is cleared | 20 minutes |
| `IOTimeout` | Max time to load/save a session from/to store | 1 minute |

> `IdleTimeout` is independent of cookie expiration — each request through Session Middleware resets the timeout.
