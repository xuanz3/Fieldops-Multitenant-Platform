# Project Charter

## Problem

Small field service businesses need a reliable way to receive service requests, assign technicians, track work and retain an auditable record without exposing one organisation's data to another.

## Product scope

FieldOps Hub will provide a multi-tenant web application for four roles:

- Tenant Admin: organisation settings, users, audit and reporting
- Dispatcher: customers, work orders, priority and assignments
- Technician: assigned work, status updates, notes and attachments
- Client: request submission, progress visibility and completion approval

## Core workflow

Client submits request → Dispatcher assigns technician → Technician performs work → Client approves or reopens → Tenant Admin reviews operations and audit records.

## Repository foundation scope

- Repository governance and planning
- Modular backend and frontend foundations
- Initial domain workflow model
- PostgreSQL development environment
- Continuous integration

## Non-goals

Repository foundation does not implement production authentication, billing, maps, SMS, mobile applications, microservices, Kubernetes or long-running paid cloud infrastructure.

## Constraints

- Free and open-source local tools are preferred
- Demonstration data must be fictional
- No secrets or personal data may be committed
- Work must be traceable through issues, commits and pull requests
