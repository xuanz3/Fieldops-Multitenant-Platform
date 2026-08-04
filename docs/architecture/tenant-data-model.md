# Tenant Data Model

```mermaid
erDiagram
    TENANT ||--o{ USER_ACCOUNT : contains
    TENANT ||--o{ CUSTOMER : owns
    TENANT ||--o{ WORK_ORDER : owns
    TENANT ||--o{ WORK_ORDER_ATTACHMENT : owns
    TENANT ||--o{ AUDIT_EVENT : records

    CUSTOMER ||--o{ WORK_ORDER : receives
    CUSTOMER o|--o| USER_ACCOUNT : links_client
    USER_ACCOUNT o|--o{ WORK_ORDER : assigned_technician
    WORK_ORDER ||--o{ WORK_ORDER_ATTACHMENT : contains
    WORK_ORDER o|--o{ AUDIT_EVENT : references

    TENANT {
        uuid Id PK
        string Name
        string Slug UK
        datetime CreatedAt
    }

    USER_ACCOUNT {
        uuid Id PK
        uuid TenantId FK
        string Email
        string DisplayName
        string Role
        bool IsActive
    }

    CUSTOMER {
        uuid Id PK
        uuid TenantId FK
        uuid ClientUserId FK
        string Reference
        string Name
        string Email
    }

    WORK_ORDER {
        uuid Id PK
        uuid TenantId FK
        uuid CustomerId FK
        uuid AssignedTechnicianId FK
        string Reference
        string Priority
        string Status
        long Version
    }

    WORK_ORDER_ATTACHMENT {
        uuid Id PK
        uuid TenantId FK
        uuid WorkOrderId FK
        uuid UploadedByUserId FK
        string FileName
        long SizeBytes
        string Sha256
    }

    AUDIT_EVENT {
        uuid Id PK
        uuid TenantId FK
        uuid WorkOrderId FK
        long Sequence
        string Action
        string PreviousHash
        string EventHash
    }
```

## Integrity Rules

- Tenant slugs are globally unique.
- Customer and work-order references are unique within a tenant.
- Tenant-owned relationships include `TenantId` in their database keys.
- A linked Client and assigned Technician must belong to the same tenant as the related record.
- Work-order `Version` is an optimistic concurrency token.
- Attachment metadata includes size, content type and SHA-256 digest.
- Audit sequences are unique per tenant.
- Audit updates and deletes are rejected by PostgreSQL.
- Timestamps are stored in UTC.
