# ADR-005: Use Session Storage and a Same-Origin API Proxy

## Status

Accepted.

## Context

The browser client needs authenticated API access during local development without requiring a broad cross-origin policy.

## Decision

Store the demonstration JWT in `sessionStorage`, so closing the browser tab clears the session.

During local development, Vite proxies `/api` and `/health` to the ASP.NET Core API. The container deployment preserves the same paths through Nginx.

## Consequences

- The browser uses same-origin API requests.
- Sessions are removed when the tab closes.
- Authentication and authorisation failures are shown explicitly.
- A public deployment should use secure HTTP-only cookies or an external identity provider.
