import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { ApiError } from '../api/client'
import {
  listClientWorkOrders,
  listCustomers,
  listTechnicianWorkOrders,
  listWorkOrders,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import type { WorkOrder } from '../types'

interface DashboardData {
  primaryLabel: string
  primaryCount: number
  secondaryLabel: string
  secondaryCount: number
  workOrders: WorkOrder[]
}

export function DashboardPage() {
  const { session } = useAuth()

  const [data, setData] =
    useState<DashboardData | null>(null)
  const [loading, setLoading] =
    useState(true)
  const [error, setError] =
    useState<string | null>(null)

  const load = useCallback(async () => {
    if (!session) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      if (
        session.user.role ===
          'TenantAdmin' ||
        session.user.role ===
          'Dispatcher'
      ) {
        const [
          customers,
          workOrders,
        ] = await Promise.all([
          listCustomers(
            session.accessToken,
            {
              page: 1,
              pageSize: 5,
            },
          ),
          listWorkOrders(
            session.accessToken,
            {
              page: 1,
              pageSize: 5,
            },
          ),
        ])

        setData({
          primaryLabel: 'Customers',
          primaryCount:
            customers.totalCount,
          secondaryLabel: 'Work orders',
          secondaryCount:
            workOrders.totalCount,
          workOrders:
            workOrders.items,
        })
      } else if (
        session.user.role ===
        'Technician'
      ) {
        const workOrders =
          await listTechnicianWorkOrders(
            session.accessToken,
          )

        setData({
          primaryLabel: 'Assigned tasks',
          primaryCount:
            workOrders.filter(
              (item) =>
                item.status ===
                  'Assigned' ||
                item.status ===
                  'InProgress',
            ).length,
          secondaryLabel:
            'Awaiting client',
          secondaryCount:
            workOrders.filter(
              (item) =>
                item.status ===
                'AwaitingClientApproval',
            ).length,
          workOrders:
            workOrders.slice(0, 5),
        })
      } else {
        const workOrders =
          await listClientWorkOrders(
            session.accessToken,
          )

        setData({
          primaryLabel:
            'Awaiting approval',
          primaryCount:
            workOrders.filter(
              (item) =>
                item.status ===
                'AwaitingClientApproval',
            ).length,
          secondaryLabel:
            'Completed work',
          secondaryCount:
            workOrders.filter(
              (item) =>
                item.status ===
                'Completed',
            ).length,
          workOrders:
            workOrders.slice(0, 5),
        })
      }
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The dashboard could not be loaded.',
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

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Role workspace"
        title="Dashboard"
        description={
          session
            ? `${session.user.role} view for ${session.user.tenantName}.`
            : 'Role-aware operational overview.'
        }
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
          title="Dashboard unavailable"
          message={error}
        />
      ) : null}

      {loading ? (
        <Feedback
          title="Loading dashboard"
          message="Reading the role-authorised workflow."
        />
      ) : null}

      {!loading && data ? (
        <>
          <section
            className="metric-grid"
            aria-label="Role metrics"
          >
            <article className="metric-card">
              <span>
                {data.primaryLabel}
              </span>
              <strong>
                {data.primaryCount}
              </strong>
              <small>
                Current tenant scope
              </small>
            </article>

            <article className="metric-card">
              <span>
                {data.secondaryLabel}
              </span>
              <strong>
                {data.secondaryCount}
              </strong>
              <small>
                Live workflow status
              </small>
            </article>

            <article className="metric-card">
              <span>Access role</span>
              <strong className="metric-text">
                {session?.user.role}
              </strong>
              <small>
                Signed JWT policy
              </small>
            </article>
          </section>

          <section className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Current queue
                </p>
                <h2>
                  Recent work orders
                </h2>
              </div>
            </div>

            {data.workOrders.length === 0 ? (
              <Feedback
                title="No work orders"
                message="There are no records in this role queue."
              />
            ) : (
              <div className="record-list">
                {data.workOrders.map(
                  (workOrder) => (
                    <article
                      key={workOrder.id}
                      className="record-row"
                    >
                      <div>
                        <strong>
                          {workOrder.reference}
                        </strong>
                        <span>
                          {workOrder.title}
                        </span>
                        <small>
                          {
                            workOrder.customerName
                          }
                        </small>
                      </div>
                      <div className="record-meta">
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
                    </article>
                  ),
                )}
              </div>
            )}
          </section>
        </>
      ) : null}
    </div>
  )
}
