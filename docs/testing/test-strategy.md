# Test Strategy

Phase 1 establishes build and domain unit tests. Later phases add database integration, API authorisation, tenant isolation, Playwright end-to-end, accessibility and k6 performance tests.

## Test principles

- Test business and security boundaries, not only line coverage
- Include negative cases for invalid states and cross-tenant access
- Record environment and sample size for performance evidence
- Preserve failed test evidence when it explains a meaningful fix
