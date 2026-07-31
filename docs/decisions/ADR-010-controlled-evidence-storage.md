# ADR-010: Controlled Evidence Storage

## Status

Accepted in Phase 7.

## Decision

FieldOps stores small portfolio demonstration attachments directly in PostgreSQL together with immutable metadata:

- WorkOrder and Tenant identity
- original safe file name
- allow-listed content type
- file size
- SHA-256 digest
- uploader identity snapshot
- upload timestamp
- file bytes

Uploads are limited to 5 MB and to PDF, PNG, JPEG and TXT. The API validates both the declared content type and extension. File names are reduced to the final path component.

Read access follows the WorkOrder boundary:

- Tenant Admin and Dispatcher may read all Tenant evidence.
- A Technician may read and upload evidence only for assigned WorkOrders.
- A Client may read evidence only for WorkOrders belonging to linked Customer records.
- Clients cannot upload.

## Consequences

PostgreSQL byte storage keeps the local-first demonstration deterministic and avoids paid object storage. For a production-scale deployment, the metadata contract can remain while the file bytes move behind an object-storage adapter with malware scanning and signed download URLs.
