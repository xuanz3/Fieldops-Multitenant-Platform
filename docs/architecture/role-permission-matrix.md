# Role Permission Matrix

| Capability | Tenant Admin | Dispatcher | Technician | Client |
|---|---:|---:|---:|---:|
| Manage tenant users | Yes | No | No | No |
| Manage customers | Yes | Yes | No | No |
| Create and update work orders | Yes | Yes | No | No |
| Assign or reassign technicians | Yes | Yes | No | No |
| Start assigned work | Yes | No | Assigned only | No |
| Submit completion details | Yes | No | Assigned only | No |
| Upload work-order files | Yes | Yes | Assigned only | No |
| Read work-order files | Tenant-wide | Tenant-wide | Assigned only | Linked customer only |
| Approve or reopen completed work | Yes | View only | No | Linked customer only |
| View audit history | Yes | Yes | No | No |
| View operations reports | Yes | Yes | No | No |
| Access another tenant | Never | Never | Never | Never |

The API enforces every permission. Hiding a browser control is not an authorisation boundary.
