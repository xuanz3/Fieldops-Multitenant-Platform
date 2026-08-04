# ADR-001: Use a Modular Monolith

## Status

Accepted.

## Context

FieldOps Hub needs clear business boundaries, reliable transactions, automated testing and straightforward deployment. The current workload does not require services to scale or deploy independently.

## Decision

Use one ASP.NET Core deployable application with separate Domain, Application and Infrastructure projects. Keep the React application as a separate build artifact served through Nginx in the container deployment.

## Consequences

- Deployment and local development remain simple.
- Database transactions and debugging remain straightforward.
- Module boundaries must be maintained through project references and tests.
- A future service split requires evidence of independent scaling, availability or ownership needs.
