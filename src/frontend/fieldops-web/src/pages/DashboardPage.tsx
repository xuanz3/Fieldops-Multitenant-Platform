import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { ApiError } from '../api/client'
import {
  listCustomers,
  listWorkOrders,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import type {
  Customer,
  WorkOrder,
} from '../types'

interface DashboardData {
  customerCount: number
  workOrderCount: number
  recentWorkOrders: WorkOrder[]
  customers: Customer[]
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
      const [
        customerPage,
        workOrderPage,
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
        customerCount:
          customerPage.totalCount,
        workOrderCount:
          workOrderPage.totalCount,
        recentWorkOrders:
          workOrderPage.items,
        customers:
          customerPage.items,
      })
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
    const loadTimer =
      window.setTimeout(() => {
        void load()
      }, 0)

    return () =>
      window.clearTimeout(loadTimer)
  }, [load])

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Operations overview"
        title="Dashboard"
        description={
          session
            ? `Live tenant-scoped data for ${session.user.tenantName}.`
            : 'Live tenant-scoped operational data.'
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
          message="Reading current tenant data from the API."
        />
      ) : null}

      {!loading && data ? (
        <>
          <section
            className="metric-grid"
            aria-label="Tenant metrics"
          >
            <article className="metric-card">
              <span>Customers</span>
              <strong>
                {data.customerCount}
              </strong>
              <small>
                Active tenant records
              </small>
            </article>

            <article className="metric-card">
              <span>Work orders</span>
              <strong>
                {data.workOrderCount}
              </strong>
              <small>
                All current statuses
              </small>
            </article>

            <article className="metric-card">
              <span>Access role</span>
              <strong className="metric-text">
                {session?.user.role}
              </strong>
              <small>
                API policy identity
              </small>
            </article>
          </section>

          <section className="content-grid">
            <article className="panel panel-large">
              <div className="panel-heading">
                <div>
                  <p className="eyebrow">
                    Work queue
                  </p>
                  <h2>Recent work orders</h2>
                </div>
              </div>

              {data.recentWorkOrders.length === 0 ? (
                <Feedback
                  title="No work orders"
                  message="Create the first work order from the Work orders page."
                />
              ) : (
                <div className="record-list">
                  {data.recentWorkOrders.map(
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
            </article>

            <article className="panel">
              <div className="panel-heading">
                <div>
                  <p className="eyebrow">
                    Customer directory
                  </p>
                  <h2>Current customers</h2>
                </div>
              </div>

              {data.customers.length === 0 ? (
                <Feedback
                  title="No customers"
                  message="Create a customer before adding work orders."
                />
              ) : (
                <div className="compact-list">
                  {data.customers.map(
                    (customer) => (
                      <div key={customer.id}>
                        <strong>
                          {customer.name}
                        </strong>
                        <span>
                          {customer.reference}
                        </span>
                      </div>
                    ),
                  )}
                </div>
              )}
            </article>
          </section>
        </>
      ) : null}
    </div>
  )
}
