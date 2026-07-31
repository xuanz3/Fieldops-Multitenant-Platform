# Phase 5 — Frontend Business Workspace

## Outcome

Phase 5 turns the tested backend into the first complete browser-operated product slice.

## Delivered

- JWT login page with fictional Dispatcher and Tenant Admin demo accounts
- SessionStorage authentication session
- Protected React Router routes
- Responsive application shell and navigation
- Tenant dashboard with live API metrics
- Customer search, pagination, create and update workflows
- WorkOrder search, status and priority filters
- WorkOrder create and version-protected update workflows
- Loading, empty, error, 401, 403 and 409 feedback
- Typed frontend API client and domain models
- Vite same-origin API proxy
- Vitest and Testing Library coverage
- One-command local demo launcher
- Browser-to-real-API smoke validation

## Security boundary

The UI never accepts or sends a selectable Tenant ID. Tenant scope remains derived from the signed JWT. The Vite proxy only forwards requests and does not alter the signed identity.

## Accessibility and responsive behaviour

Forms use explicit labels and validation messages. Navigation, buttons, tables and drawers are keyboard accessible. The layout adapts to smaller screens without removing business functions.

## Scope exclusions

Technician assignment, Client ownership, workflow transition controls, attachments, audit records, reporting and cloud deployment remain later Phases.

## Cost

USD 0. The implementation uses local Docker and GitHub Actions.
