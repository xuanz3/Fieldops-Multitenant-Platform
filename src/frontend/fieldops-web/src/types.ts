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
