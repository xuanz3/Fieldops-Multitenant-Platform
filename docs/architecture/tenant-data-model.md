# Multi-tenant data foundation Data Model

```mermaid
erDiagram
    TENANT ||--o{ CUSTOMER : owns
    TENANT ||--o{ WORK_ORDER : owns
    CUSTOMER ||--o{ WORK_ORDER : receives

    TENANT {
        uuid Id PK
        string Name
        string Slug UK
        datetime CreatedAt
    }

    CUSTOMER {
        uuid Id PK
        uuid TenantId FK
        string Reference
        string Name
        string Email
        datetime CreatedAt
        datetime UpdatedAt
    }

    WORK_ORDER {
        uuid Id PK
        uuid TenantId FK
        uuid CustomerId FK
        string Reference
        string Title
        string Description
        string Priority
        string Status
        long Version
        datetime CreatedAt
        datetime UpdatedAt
    }
```

## Key rules

- `Customer(TenantId, Reference)` is unique.
- `WorkOrder(TenantId, Reference)` is unique.
- The work-order customer relationship uses both `TenantId` and `CustomerId`.
- `Version` supports later optimistic concurrency handling.
- Timestamps use UTC.

The diagram is intentionally limited to Multi-tenant data foundation. Users, roles, assignments, attachments and audit events are added in later phases.
