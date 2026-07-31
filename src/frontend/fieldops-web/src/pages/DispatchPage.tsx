import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { ApiError } from '../api/client'
import {
  assignWorkOrder,
  linkCustomerClient,
  listClients,
  listCustomerOwnership,
  listTechnicians,
  listWorkOrders,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import type {
  ClientOption,
  CustomerOwnership,
  TechnicianOption,
  WorkOrder,
} from '../types'

export function DispatchPage() {
  const { session } = useAuth()

  const [workOrders, setWorkOrders] =
    useState<WorkOrder[]>([])
  const [technicians, setTechnicians] =
    useState<TechnicianOption[]>([])
  const [clients, setClients] =
    useState<ClientOption[]>([])
  const [ownership, setOwnership] =
    useState<CustomerOwnership[]>([])
  const [selectedTechnician, setSelectedTechnician] =
    useState<Record<string, string>>({})
  const [selectedClient, setSelectedClient] =
    useState<Record<string, string>>({})
  const [loading, setLoading] =
    useState(true)
  const [savingId, setSavingId] =
    useState<string | null>(null)
  const [error, setError] =
    useState<string | null>(null)

  const load = useCallback(async () => {
    if (!session) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      const [
        workOrderPage,
        technicianItems,
        clientItems,
        ownershipItems,
      ] = await Promise.all([
        listWorkOrders(
          session.accessToken,
          {
            page: 1,
            pageSize: 100,
          },
        ),
        listTechnicians(
          session.accessToken,
        ),
        listClients(
          session.accessToken,
        ),
        listCustomerOwnership(
          session.accessToken,
        ),
      ])

      setWorkOrders(
        workOrderPage.items.filter(
          (item) =>
            item.status ===
              'Submitted' ||
            item.status ===
              'Assigned' ||
            item.status ===
              'Reopened',
        ),
      )
      setTechnicians(technicianItems)
      setClients(clientItems)
      setOwnership(ownershipItems)

      setSelectedTechnician(
        Object.fromEntries(
          workOrderPage.items.map(
            (item) => [
              item.id,
              item.assignedTechnicianId ??
                technicianItems[0]?.id ??
                '',
            ],
          ),
        ),
      )

      setSelectedClient(
        Object.fromEntries(
          ownershipItems.map(
            (item) => [
              item.customerId,
              item.clientUserId ?? '',
            ],
          ),
        ),
      )
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The dispatch workspace could not be loaded.',
      )
    } finally {
      setLoading(false)
    }
  }, [session])

  useEffect(() => {
    const timer = window.setTimeout(
      () => {
        void load()
      },
      0,
    )

    return () =>
      window.clearTimeout(timer)
  }, [load])

  async function assign(
    workOrder: WorkOrder,
  ) {
    if (!session) {
      return
    }

    const technicianId =
      selectedTechnician[workOrder.id]

    if (!technicianId) {
      setError(
        'Select a Technician before assigning.',
      )
      return
    }

    setSavingId(workOrder.id)
    setError(null)

    try {
      await assignWorkOrder(
        session.accessToken,
        workOrder.id,
        technicianId,
        workOrder.version,
      )
      await load()
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The work order could not be assigned.',
      )
    } finally {
      setSavingId(null)
    }
  }

  async function linkClient(
    customer: CustomerOwnership,
  ) {
    if (!session) {
      return
    }

    setSavingId(customer.customerId)
    setError(null)

    try {
      await linkCustomerClient(
        session.accessToken,
        customer.customerId,
        selectedClient[
          customer.customerId
        ] || null,
      )
      await load()
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The Client ownership could not be updated.',
      )
    } finally {
      setSavingId(null)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Dispatcher control"
        title="Dispatch"
        description="Link Client users to Customers and assign eligible work orders to active Technicians."
        actions={
          <button
            className="button button-secondary"
            type="button"
            onClick={() => void load()}
          >
            Refresh
          </button>
        }
      />

      {error ? (
        <Feedback
          tone="error"
          title="Dispatch action failed"
          message={error}
        />
      ) : null}

      {loading ? (
        <Feedback
          title="Loading dispatch"
          message="Reading technicians, clients and assignable work."
        />
      ) : null}

      {!loading ? (
        <section className="workflow-layout">
          <article className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Assignment queue
                </p>
                <h2>
                  Assignable work orders
                </h2>
              </div>
            </div>

            {workOrders.length === 0 ? (
              <Feedback
                title="No assignment required"
                message="All current work is already underway or complete."
              />
            ) : (
              <div className="workflow-card-list">
                {workOrders.map(
                  (workOrder) => (
                    <article
                      key={workOrder.id}
                      className="workflow-card"
                    >
                      <div>
                        <span className="record-reference">
                          {
                            workOrder.reference
                          }
                        </span>
                        <h3>
                          {workOrder.title}
                        </h3>
                        <p>
                          {
                            workOrder.customerName
                          }
                        </p>
                      </div>

                      <div className="badge-row badge-row-left">
                        <StatusBadge
                          value={
                            workOrder.status
                          }
                        />
                        <StatusBadge
                          value={
                            workOrder.priority
                          }
                        />
                      </div>

                      <label className="compact-field">
                        <span>
                          Technician
                        </span>
                        <select
                          value={
                            selectedTechnician[
                              workOrder.id
                            ] ?? ''
                          }
                          onChange={(event) =>
                            setSelectedTechnician(
                              (current) => ({
                                ...current,
                                [workOrder.id]:
                                  event.target
                                    .value,
                              }),
                            )
                          }
                        >
                          <option value="">
                            Select Technician
                          </option>
                          {technicians.map(
                            (technician) => (
                              <option
                                key={
                                  technician.id
                                }
                                value={
                                  technician.id
                                }
                              >
                                {
                                  technician.displayName
                                }
                              </option>
                            ),
                          )}
                        </select>
                      </label>

                      <button
                        className="button button-primary button-full"
                        type="button"
                        disabled={
                          savingId ===
                          workOrder.id
                        }
                        onClick={() =>
                          void assign(
                            workOrder,
                          )
                        }
                      >
                        {savingId ===
                        workOrder.id
                          ? 'Assigning…'
                          : workOrder.status ===
                              'Assigned'
                            ? 'Reassign'
                            : 'Assign'}
                      </button>
                    </article>
                  ),
                )}
              </div>
            )}
          </article>

          <article className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Client ownership
                </p>
                <h2>
                  Customer access
                </h2>
              </div>
            </div>

            <div className="ownership-list">
              {ownership.map((customer) => (
                <article
                  key={customer.customerId}
                  className="ownership-row"
                >
                  <div>
                    <strong>
                      {
                        customer.customerReference
                      }
                    </strong>
                    <span>
                      {customer.customerName}
                    </span>
                  </div>

                  <select
                    aria-label={`Client for ${customer.customerName}`}
                    value={
                      selectedClient[
                        customer.customerId
                      ] ?? ''
                    }
                    onChange={(event) =>
                      setSelectedClient(
                        (current) => ({
                          ...current,
                          [customer.customerId]:
                            event.target.value,
                        }),
                      )
                    }
                  >
                    <option value="">
                      No Client user
                    </option>
                    {clients.map(
                      (client) => (
                        <option
                          key={client.id}
                          value={client.id}
                        >
                          {
                            client.displayName
                          }
                        </option>
                      ),
                    )}
                  </select>

                  <button
                    className="button button-secondary"
                    type="button"
                    disabled={
                      savingId ===
                      customer.customerId
                    }
                    onClick={() =>
                      void linkClient(
                        customer,
                      )
                    }
                  >
                    Save
                  </button>
                </article>
              ))}
            </div>
          </article>
        </section>
      ) : null}
    </div>
  )
}
