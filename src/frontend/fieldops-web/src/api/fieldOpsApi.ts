import { apiRequest } from './client'
import type {
  ClientOption,
  Customer,
  CustomerInput,
  CustomerOwnership,
  CustomerUpdateInput,
  LoginResponse,
  PagedResponse,
  TechnicianOption,
  WorkOrder,
  WorkOrderInput,
  WorkOrderPriority,
  WorkOrderStatus,
  WorkOrderUpdateInput,
} from '../types'

export interface CustomerQuery {
  search?: string
  page?: number
  pageSize?: number
}

export interface WorkOrderQuery {
  search?: string
  status?: WorkOrderStatus | ''
  priority?: WorkOrderPriority | ''
  customerId?: string
  page?: number
  pageSize?: number
}

function queryString(
  values: Record<
    string,
    string | number | undefined
  >,
): string {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(values)) {
    if (
      value !== undefined &&
      String(value).trim() !== ''
    ) {
      params.set(key, String(value))
    }
  }

  const query = params.toString()
  return query ? `?${query}` : ''
}

export function login(
  tenantSlug: string,
  email: string,
  password: string,
): Promise<LoginResponse> {
  return apiRequest<LoginResponse>(
    '/api/auth/login',
    {
      method: 'POST',
      body: {
        tenantSlug,
        email,
        password,
      },
    },
  )
}

export function listCustomers(
  token: string,
  query: CustomerQuery,
): Promise<PagedResponse<Customer>> {
  return apiRequest<PagedResponse<Customer>>(
    `/api/customers${queryString({
      search: query.search,
      page: query.page,
      pageSize: query.pageSize,
    })}`,
    { token },
  )
}

export function createCustomer(
  token: string,
  input: CustomerInput,
): Promise<Customer> {
  return apiRequest<Customer>(
    '/api/customers',
    {
      method: 'POST',
      token,
      body: {
        reference: input.reference,
        name: input.name,
        email: input.email || null,
      },
    },
  )
}

export function updateCustomer(
  token: string,
  customerId: string,
  input: CustomerUpdateInput,
): Promise<Customer> {
  return apiRequest<Customer>(
    `/api/customers/${customerId}`,
    {
      method: 'PUT',
      token,
      body: {
        name: input.name,
        email: input.email || null,
      },
    },
  )
}

export function listWorkOrders(
  token: string,
  query: WorkOrderQuery,
): Promise<PagedResponse<WorkOrder>> {
  return apiRequest<PagedResponse<WorkOrder>>(
    `/api/work-orders${queryString({
      search: query.search,
      status: query.status || undefined,
      priority: query.priority || undefined,
      customerId: query.customerId,
      page: query.page,
      pageSize: query.pageSize,
    })}`,
    { token },
  )
}

export function createWorkOrder(
  token: string,
  input: WorkOrderInput,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    '/api/work-orders',
    {
      method: 'POST',
      token,
      body: {
        customerId: input.customerId,
        reference: input.reference,
        title: input.title,
        description: input.description || null,
        priority: input.priority,
      },
    },
  )
}

export function updateWorkOrder(
  token: string,
  workOrderId: string,
  input: WorkOrderUpdateInput,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    `/api/work-orders/${workOrderId}`,
    {
      method: 'PUT',
      token,
      body: {
        customerId: input.customerId,
        title: input.title,
        description: input.description || null,
        priority: input.priority,
        version: input.version,
      },
    },
  )
}

export function listTechnicians(
  token: string,
): Promise<TechnicianOption[]> {
  return apiRequest<TechnicianOption[]>(
    '/api/workflow/technicians',
    { token },
  )
}

export function listClients(
  token: string,
): Promise<ClientOption[]> {
  return apiRequest<ClientOption[]>(
    '/api/workflow/clients',
    { token },
  )
}

export function listCustomerOwnership(
  token: string,
): Promise<CustomerOwnership[]> {
  return apiRequest<CustomerOwnership[]>(
    '/api/workflow/customer-ownership',
    { token },
  )
}

export function linkCustomerClient(
  token: string,
  customerId: string,
  clientUserId: string | null,
): Promise<CustomerOwnership> {
  return apiRequest<CustomerOwnership>(
    `/api/workflow/customers/${customerId}/client`,
    {
      method: 'PUT',
      token,
      body: { clientUserId },
    },
  )
}

export function assignWorkOrder(
  token: string,
  workOrderId: string,
  technicianUserId: string,
  version: number,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    `/api/workflow/work-orders/${workOrderId}/assign`,
    {
      method: 'POST',
      token,
      body: {
        technicianUserId,
        version,
      },
    },
  )
}

export function listTechnicianWorkOrders(
  token: string,
): Promise<WorkOrder[]> {
  return apiRequest<WorkOrder[]>(
    '/api/technician/work-orders',
    { token },
  )
}

export function startWorkOrder(
  token: string,
  workOrderId: string,
  version: number,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    `/api/technician/work-orders/${workOrderId}/start`,
    {
      method: 'POST',
      token,
      body: { version },
    },
  )
}

export function submitWorkOrder(
  token: string,
  workOrderId: string,
  completionSummary: string,
  version: number,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    `/api/technician/work-orders/${workOrderId}/submit`,
    {
      method: 'POST',
      token,
      body: {
        completionSummary,
        version,
      },
    },
  )
}

export function listClientWorkOrders(
  token: string,
): Promise<WorkOrder[]> {
  return apiRequest<WorkOrder[]>(
    '/api/client/work-orders',
    { token },
  )
}

export function approveWorkOrder(
  token: string,
  workOrderId: string,
  version: number,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    `/api/client/work-orders/${workOrderId}/approve`,
    {
      method: 'POST',
      token,
      body: { version },
    },
  )
}

export function reopenWorkOrder(
  token: string,
  workOrderId: string,
  reason: string,
  version: number,
): Promise<WorkOrder> {
  return apiRequest<WorkOrder>(
    `/api/client/work-orders/${workOrderId}/reopen`,
    {
      method: 'POST',
      token,
      body: {
        reason,
        version,
      },
    },
  )
}
