# Project Scope

## Problem

Field-service teams need a reliable way to receive requests, assign technicians, track execution and retain an auditable record without exposing one organisation's data to another.

## Roles

| Role | Responsibilities |
|---|---|
| Tenant Admin | Tenant-wide users, audit and reporting |
| Dispatcher | Customers, work orders and assignments |
| Technician | Assigned work, status updates and completion files |
| Client | Linked work-order review and approval |

## Included

- Multi-tenant data model
- Authentication and role policies
- Customer and work-order management
- Assignment and completion workflow
- Work-order attachments
- Audit history
- Operational reports
- Automated tests
- Local and container deployment

## Excluded

- Billing
- SMS delivery
- Native mobile applications
- Public cloud hosting
- Object-storage malware scanning
- MFA
- Managed backups
- Kubernetes

## Constraints

- Demonstration data is fictional.
- Secrets and personal information are not committed.
- Changes are reviewed through pull requests and automated checks.
