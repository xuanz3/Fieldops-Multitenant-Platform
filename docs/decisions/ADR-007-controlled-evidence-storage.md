# ADR-007: Store Controlled Work-Order Attachments

## Status

Accepted.

## Context

Small work-order files must be available in the local and container environments with predictable access control and integrity metadata.

## Decision

Store attachment bytes in PostgreSQL with:

- Tenant and work-order identity
- Safe original file name
- Allow-listed content type
- File size
- SHA-256 digest
- Uploader identity snapshot
- Upload timestamp

Uploads are limited to 5 MB and to PDF, PNG, JPEG and TXT. The API validates the declared content type and extension and reduces file names to the final path component.

Read and upload access follows tenant, role and work-order ownership.

## Consequences

- Local and container deployments remain deterministic.
- Attachment integrity can be checked through SHA-256 metadata.
- Large-scale deployment can retain the metadata contract while moving bytes to object storage with malware scanning and signed URLs.
