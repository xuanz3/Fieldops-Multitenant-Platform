# ADR-001: Use a modular monolith

- Status: Accepted
- Date: 2026-07-29

## Context

The platform needs clear business modules, automated testing and deployment evidence, but does not need independent service scaling during the portfolio stage.

## Decision

Use one ASP.NET Core deployable application with separate Domain, Application and Infrastructure projects. The React client remains a separate build artifact.

## Consequences

- Lower operational cost and simpler local reproduction
- Transactions and debugging remain straightforward
- Module boundaries must be protected through references and tests
- A future service split requires evidence of independent scaling or ownership needs
