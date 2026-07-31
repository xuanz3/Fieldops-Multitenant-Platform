import { apiRequest } from './client'
import type {
  Customer,
  CustomerInput,
  CustomerUpdateInput,
  LoginResponse,
  PagedResponse,
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
