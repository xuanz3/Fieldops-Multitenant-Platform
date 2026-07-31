export type UserRole =
  | 'TenantAdmin'
  | 'Dispatcher'
  | 'Technician'
  | 'Client'

export type WorkOrderPriority =
  | 'Low'
  | 'Normal'
  | 'High'
  | 'Urgent'

export type WorkOrderStatus =
  | 'Submitted'
  | 'Assigned'
  | 'InProgress'
  | 'AwaitingClientApproval'
  | 'Completed'
  | 'Reopened'
  | 'Cancelled'

export interface AuthenticatedUser {
  id: string
  tenantId: string
  tenantSlug: string
  tenantName: string
  email: string
  displayName: string
  role: UserRole
}

export interface LoginResponse {
  accessToken: string
  tokenType: 'Bearer'
  expiresAt: string
  user: AuthenticatedUser
}

export interface Session {
  accessToken: string
  expiresAt: string
  user: AuthenticatedUser
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface Customer {
  id: string
  reference: string
  name: string
  email: string | null
  createdAt: string
  updatedAt: string
}

export interface CustomerInput {
  reference: string
  name: string
  email: string
}

export interface CustomerUpdateInput {
  name: string
  email: string
}

export interface WorkOrder {
  id: string
  customerId: string
  customerName: string
  reference: string
  title: string
  description: string | null
  priority: WorkOrderPriority
  status: WorkOrderStatus
  assignedTechnicianId: string | null
  assignedTechnicianName: string | null
  assignedAt: string | null
  startedAt: string | null
  submittedForApprovalAt: string | null
  completionSummary: string | null
  completedAt: string | null
  clientReopenReason: string | null
  version: number
  createdAt: string
  updatedAt: string
}

export interface WorkOrderInput {
  customerId: string
  reference: string
  title: string
  description: string
  priority: WorkOrderPriority
}

export interface WorkOrderUpdateInput {
  customerId: string
  title: string
  description: string
  priority: WorkOrderPriority
  version: number
}

export interface TechnicianOption {
  id: string
  displayName: string
  email: string
}

export interface ClientOption {
  id: string
  displayName: string
  email: string
}

export interface CustomerOwnership {
  customerId: string
  customerReference: string
  customerName: string
  clientUserId: string | null
  clientDisplayName: string | null
}

export interface WorkOrderAttachment {
  id: string
  workOrderId: string
  fileName: string
  contentType: string
  sizeBytes: number
  sha256: string
  uploadedByUserId: string
  uploadedByDisplayName: string
  uploadedAt: string
}

export interface AuditEvent {
  id: string
  sequence: number
  action: string
  entityType: string
  entityId: string
  workOrderId: string | null
  summary: string
  actorUserId: string | null
  actorDisplayName: string
  actorRole: string
  occurredAt: string
  previousHash: string
  eventHash: string
}

export interface AuditVerification {
  isValid: boolean
  eventCount: number
  firstSequence: number | null
  lastSequence: number | null
  failure: string | null
}

export interface NamedCount {
  name: string
  count: number
}

export interface TechnicianReport {
  technicianId: string
  technicianName: string
  assigned: number
  inProgress: number
  awaitingClientApproval: number
  completed: number
}

export interface CustomerReport {
  customerId: string
  customerReference: string
  customerName: string
  total: number
  open: number
  completed: number
}

export interface OperationsReport {
  totalWorkOrders: number
  openWorkOrders: number
  completedWorkOrders: number
  completionRate: number
  averageCompletionHours: number | null
  attachmentCount: number
  auditEventCount: number
  statusCounts: NamedCount[]
  priorityCounts: NamedCount[]
  technicians: TechnicianReport[]
  customers: CustomerReport[]
  generatedAt: string
}
