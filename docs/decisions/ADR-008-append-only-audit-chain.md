# ADR-008: Append-Only Tenant Audit Chain

## Status

Accepted in Evidence, audit and reporting.

## Decision

Business changes are captured automatically by `FieldOpsDbContext` before persistence. Each Tenant receives an independent ordered chain of `AuditEvent` records.

Every event includes:

- Tenant sequence
- actor identity and role snapshot
- action and entity identity
- optional WorkOrder identity
- summary
- timestamp
- previous event hash
- SHA-256 event hash

The event hash is calculated from canonical event data and the previous hash. The first event uses `GENESIS`.

Audit timestamps are normalised to PostgreSQL microsecond precision before hashing. This keeps the stored timestamp and the canonical hash input identical across macOS and Linux.

The API exposes read-only listing and full-chain verification. No update or delete endpoint exists. PostgreSQL also installs a trigger that rejects every `UPDATE` or `DELETE` against `audit_events`.

## Consequences

Application code cannot silently edit prior audit records, and direct database mutation is rejected. The chain verifier detects sequence gaps, previous-hash mismatches and event-content changes.

This provides tamper-evident portfolio evidence. A production compliance system would additionally export hashes to an external trust boundary and define formal retention controls.
