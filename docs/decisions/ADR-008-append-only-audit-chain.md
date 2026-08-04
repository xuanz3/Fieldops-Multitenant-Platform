# ADR-008: Maintain an Append-Only Tenant Audit Chain

## Status

Accepted.

## Context

Business changes require a searchable history that detects sequence gaps and record modification.

## Decision

Capture business changes before persistence and maintain an independent ordered chain of `AuditEvent` records for each tenant.

Each event contains actor, action, entity, timestamp, sequence, previous hash and SHA-256 event hash. The first event uses `GENESIS`. Timestamps are normalised to PostgreSQL microsecond precision before hashing.

The API exposes read-only listing and chain verification. PostgreSQL rejects every `UPDATE` and `DELETE` against `audit_events`.

## Consequences

- Application code cannot silently edit prior audit records.
- Direct database mutation is rejected.
- Verification detects sequence gaps, previous-hash mismatches and content changes.
- A regulated deployment should additionally anchor hashes outside the application database and define formal retention controls.
