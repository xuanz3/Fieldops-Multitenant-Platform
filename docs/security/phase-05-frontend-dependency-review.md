# Phase 5 Frontend Dependency Review

## Trigger

The initial Phase 5 installation selected React Router DOM 7.18.2 and npm reported two High advisories.

## Decision

FieldOps uses Declarative Mode through `BrowserRouter`; it does not use React Router Framework Mode, Data Mode, server actions or unstable RSC APIs.

The dependency is pinned to the maintained version-6 release `react-router-dom@6.30.4`, which provides the routing APIs required by this SPA without introducing the affected version-7 execution paths.

## Validation

- Exact dependency version recorded in package-lock
- ESLint
- Vitest
- TypeScript and Vite production build
- `npm audit --audit-level=high`
- Browser-to-real-API smoke test

## Review rule

Any future router upgrade must pass the same audit, build, component-test and real-API smoke gates before merge.
