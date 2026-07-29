# Role Permission Matrix

| Capability | Tenant Admin | Dispatcher | Technician | Client |
|---|---:|---:|---:|---:|
| Manage organisation users | Yes | No | No | No |
| Manage customers | Yes | Yes | No | No |
| Create work orders | Yes | Yes | No | Yes |
| Assign technicians | Yes | Yes | No | No |
| Update assigned work | View | View | Yes | No |
| Approve or reopen completed work | View | View | No | Yes |
| View organisation audit records | Yes | Limited | Own actions | Own requests |
| Access another tenant | Never | Never | Never | Never |

All permissions must be enforced by the API. Hiding a browser control is not an authorisation control.
