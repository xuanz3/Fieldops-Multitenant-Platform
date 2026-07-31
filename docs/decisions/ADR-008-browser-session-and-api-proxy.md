# ADR-008: Browser Session and Same-Origin API Proxy

## Status

Accepted in Phase 5.

## Decision

The React application stores the demonstration JWT session in `sessionStorage`, not persistent local storage. Closing the browser tab removes the session.

During local development, Vite proxies `/api` and `/health` to the ASP.NET Core API on port 5204. The browser therefore uses same-origin requests and the backend does not need a broad development CORS policy.

The production deployment will preserve the same `/api` path through a reverse proxy.

## Consequences

- The frontend can call authenticated APIs without embedding a separate backend origin.
- The token is cleared when the tab closes.
- A 401 or 403 is surfaced explicitly to the operator.
- The current approach is suitable for the portfolio demonstration.
- A production-grade deployment should prefer a secure HTTP-only cookie or a dedicated identity provider.
