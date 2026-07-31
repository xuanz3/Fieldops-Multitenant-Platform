# FieldOps Hub Portfolio Guide

## Project summary

FieldOps Hub is a multi-tenant field service operations platform built with React, TypeScript, ASP.NET Core, Entity Framework Core and PostgreSQL.

It demonstrates secure authentication, role-owned workflow, tenant isolation, controlled evidence, tamper-evident audit history, reporting, automated tests and container deployment.

## Resume bullets

- Built a multi-tenant field service operations platform with React, TypeScript, ASP.NET Core, Entity Framework Core and PostgreSQL.
- Enforced tenant boundaries through signed JWT claims, EF Core query filters and composite database relationships.
- Implemented Dispatcher, Technician and Client workflow with optimistic concurrency and role-owned actions.
- Added controlled evidence uploads with file allow-lists, size limits, authorised downloads and SHA-256 integrity metadata.
- Designed an append-only per-tenant audit chain protected by PostgreSQL triggers and cross-platform hash verification.
- Delivered operational reporting, CSV export, automated CI, production container builds and end-to-end smoke validation.

## Interview explanation

> I built an end-to-end multi-tenant field service platform. Dispatchers manage customers and assign work, technicians execute and submit completion evidence, and linked clients approve or reopen the work. Tenant identity comes only from validated JWT claims and is enforced again through Entity Framework query filters and composite PostgreSQL relationships. Attachments receive SHA-256 integrity metadata, and business changes are written to an append-only tenant audit chain. The final release includes unit tests, PostgreSQL integration tests, API security tests, frontend tests, full smoke tests and a production-style Docker deployment.

## Suggested demonstration order

1. Dashboard
2. Work orders
3. Dispatch
4. Technician workspace
5. Evidence
6. Client approval
7. Audit log
8. Reports
9. Architecture
10. CI and deployment evidence

## Honest limitations

This is a portfolio deployment, not a permanently hosted commercial service.

Production object storage, malware scanning, MFA, refresh tokens, managed secrets, managed backups and public HTTPS infrastructure are documented but not claimed as implemented.
