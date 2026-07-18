# Security Concepts

---

## JWT — JSON Web Token

JWT (pronounced "jot") is an open standard (RFC 7519) that defines a compact, self-contained way for securely transmitting information between parties as a JSON object.

> JWT contains all required information about an entity — the recipient does not need to query a database to validate the token.

---

## JWT Benefits

| Benefit | Description |
|---------|-------------|
| **Compact** | JSON is less verbose than XML — JWTs are smaller than SAML tokens |
| **Secure** | Can use public/private key pairs (X.509) or HMAC algorithm |
| **Common** | JSON parsers exist in most languages and map directly to objects |
| **Easy to process** | Designed for internet scale; works well on mobile devices |

---

## JWT Usage

| Use Case | Description |
|----------|-------------|
| **Authentication** | ID token returned on successful login (always a JWT per OIDC spec) |
| **Authorization** | Access token passed in every request to access routes/resources |
| **Information Exchange** | Securely transmit information between parties; signed for verification |

JWT is widely used in **Single Sign-On (SSO)** due to its small overhead and cross-domain flexibility.

---

## JWT Security

1. Use encrypted JWT with algorithms like AES or Triple DES — never include sensitive info in payload.
2. Always include an expiry time (`exp` claim).
3. Enforce **HTTPS** for secure communication.

---

## JWT Structure

A JWT consists of three Base64url-encoded strings separated by dots:

```
header.payload.signature
```

### 1. Header
Contains the token type and hashing algorithm.

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### 2. Payload (Claims)
Verifiable security statements about the user.

```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "admin": true
}
```

### 3. Signature
Used to validate the token has not been tampered with.

---

## JWT Claims

Claims are pieces of information asserted about a subject.

### Registered (Standard) Claims

| Claim | Name | Description |
|-------|------|-------------|
| `iss` | Issuer | Who issued the JWT |
| `sub` | Subject | User the JWT is about |
| `aud` | Audience | Intended recipient |
| `exp` | Expiration Time | When the JWT expires |
| `nbf` | Not Before | Earliest valid time |
| `iat` | Issued At | When the JWT was issued |
| `jti` | JWT ID | Unique identifier (prevents replay) |

### Custom Claims

- **Public claims** — Generic info like `name`, `email`. Must be registered or use collision-resistant namespacing.
- **Private claims** — App-specific info like `employeeId`, `department`.

> **Note:** Custom claims must be at most **100KB**.

---

## Access Token vs Refresh Token

| | Access Token | Refresh Token |
|--|--------------|---------------|
| **Purpose** | Authorizes access | Extends access token life |
| **Expiry** | Short-lived | Long-lived |
| **On Expiry** | Must re-authenticate | Auto-regenerates access token |
| **Use Case** | Short sessions | Long sessions |

---

## OAuth 2.0

OAuth 2.0 is the industry-standard protocol for **authorization**. It allows third-party applications to access protected resources on behalf of a user **without sharing passwords**.

> **Use Case:** Login to Facebook using a Google account.

### Key Components

| Role | Description |
|------|-------------|
| **APP** | UI that the user interacts with |
| **Identity Server** | Handles authentication/authorization |
| **Resource Server** | Provides protected data/resources |

### Client Types
- **Public clients** — Web applications interacting with users.
- **Confidential clients** — Applications running on web servers.

### Communication Channels
- **Front channel** — User input flows from app to server.
- **Back channel** — Server-to-app communication without user intervention.

### OAuth 2.0 Flows

| Flow | Approach | Description |
|------|----------|-------------|
| **Authorization Code** | 3-legged | Most common + recommended for SSO |
| **Client Credential** | 2-legged | App-to-app (no user) |
| **Resource Owner Password** | 2-legged | User submits credentials to app → app to server |
| **Implicit** | 2-legged | Least secure; token returned directly |

#### Authorization Code Flow (Recommended for SSO)
1. User clicks login → App sends authorization request to Identity Server.
2. User enters credentials → Identity Server returns authorization code to App.
3. App exchanges authorization code for access token (JWT).
4. App uses access token to get data from Resource Server.

> **Note:** OAuth 2.0 + JWT enables SSO implementation.

---

## SAML (Security Assertion Markup Language)

- Used for exchanging **authorization/authentication data** between an identity provider (IdP) and service provider (SP).
- Standard for **SSO** — authenticate once with identity server, access multiple service providers.

---

## OpenIAM

An identity and access management platform providing:
- Unified identity across all applications.
- Flexible **role-based access control (RBAC)**.
- Authentication and **SSO support**.

---

## IGA — Identity Governance and Administration

A policy framework and set of security solutions for:
- Managing large identity chaos.
- Mitigating access-related risks.
- Critical component of any security strategy.
