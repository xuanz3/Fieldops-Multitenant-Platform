import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { ApiError } from '../api/client'
import {
  downloadOperationsReport,
  getOperationsReport,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import type {
  NamedCount,
  OperationsReport,
} from '../types'

export function ReportsPage() {
  const { session } = useAuth()
  const [report, setReport] =
    useState<OperationsReport | null>(
      null,
    )
  const [loading, setLoading] =
    useState(true)
  const [downloading, setDownloading] =
    useState(false)
  const [error, setError] =
    useState<string | null>(null)

  const load =
    useCallback(async () => {
      if (!session) {
        return
      }

      setLoading(true)
      setError(null)

      try {
        setReport(
          await getOperationsReport(
            session.accessToken,
          ),
        )
      } catch (reason) {
        setError(
          reason instanceof ApiError
            ? reason.message
            : 'The operations report could not be loaded.',
        )
      } finally {
        setLoading(false)
      }
    }, [session])

  useEffect(() => {
    const timer =
      window.setTimeout(
        () => {
          void load()
        },
        0,
      )

    return () =>
      window.clearTimeout(timer)
  }, [load])

  async function downloadCsv() {
    if (!session) {
      return
    }

    setDownloading(true)
    setError(null)

    try {
      const blob =
        await downloadOperationsReport(
          session.accessToken,
        )
      const url =
        URL.createObjectURL(blob)
      const link =
        document.createElement('a')

      link.href = url
      link.download =
        'fieldops-operations-report.csv'
      document.body.append(link)
      link.click()
      link.remove()
      URL.revokeObjectURL(url)
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The CSV report could not be downloaded.',
      )
    } finally {
      setDownloading(false)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Operational intelligence"
        title="Reports"
        description="Tenant-scoped work-order, Technician, Customer, evidence and audit metrics generated from live PostgreSQL data."
        actions={
          <>
            <button
              className="button button-secondary"
              type="button"
              onClick={() =>
                void load()
              }
            >
              Refresh
            </button>
            <button
              className="button button-primary"
              type="button"
              disabled={
                downloading ||
                !report
              }
              onClick={() =>
                void downloadCsv()
              }
            >
              {downloading
                ? 'Preparing…'
                : 'Download CSV'}
            </button>
          </>
        }
      />

      {error ? (
        <Feedback
          tone="error"
          title="Report unavailable"
          message={error}
        />
      ) : null}

      {loading ? (
        <Feedback
          title="Generating report"
          message="Aggregating the current tenant operations."
        />
      ) : null}

      {!loading && report ? (
        <>
          <section className="metric-grid report-metric-grid">
            <MetricCard
              label="Total work orders"
              value={
                report.totalWorkOrders
              }
              detail="Current tenant"
            />
            <MetricCard
              label="Open work"
              value={
                report.openWorkOrders
              }
              detail="Excludes completed and cancelled"
            />
            <MetricCard
              label="Completion rate"
              value={`${report.completionRate}%`}
              detail="Completed / total"
            />
            <MetricCard
              label="Evidence files"
              value={
                report.attachmentCount
              }
              detail="Integrity hashed"
            />
            <MetricCard
              label="Audit events"
              value={
                report.auditEventCount
              }
              detail="Append-only chain"
            />
            <MetricCard
              label="Average completion"
              value={
                report.averageCompletionHours ===
                null
                  ? '—'
                  : `${report.averageCompletionHours}h`
              }
              detail="Created to approved"
            />
          </section>

          <section className="report-grid">
            <CountPanel
              title="Work-order status"
              items={
                report.statusCounts
              }
            />
            <CountPanel
              title="Priority mix"
              items={
                report.priorityCounts
              }
            />
          </section>

          <section className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Workforce
                </p>
                <h2>
                  Technician workload
                </h2>
              </div>
            </div>

            <div className="report-table-wrap">
              <table className="report-table">
                <thead>
                  <tr>
                    <th>Technician</th>
                    <th>Assigned</th>
                    <th>In progress</th>
                    <th>
                      Awaiting client
                    </th>
                    <th>Completed</th>
                  </tr>
                </thead>
                <tbody>
                  {report.technicians.map(
                    (technician) => (
                      <tr
                        key={
                          technician.technicianId
                        }
                      >
                        <td>
                          {
                            technician.technicianName
                          }
                        </td>
                        <td>
                          {
                            technician.assigned
                          }
                        </td>
                        <td>
                          {
                            technician.inProgress
                          }
                        </td>
                        <td>
                          {
                            technician.awaitingClientApproval
                          }
                        </td>
                        <td>
                          {
                            technician.completed
                          }
                        </td>
                      </tr>
                    ),
                  )}
                </tbody>
              </table>
            </div>
          </section>

          <section className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Customer operations
                </p>
                <h2>
                  Workload by Customer
                </h2>
              </div>
            </div>

            <div className="report-table-wrap">
              <table className="report-table">
                <thead>
                  <tr>
                    <th>Reference</th>
                    <th>Customer</th>
                    <th>Total</th>
                    <th>Open</th>
                    <th>Completed</th>
                  </tr>
                </thead>
                <tbody>
                  {report.customers.map(
                    (customer) => (
                      <tr
                        key={
                          customer.customerId
                        }
                      >
                        <td>
                          {
                            customer.customerReference
                          }
                        </td>
                        <td>
                          {
                            customer.customerName
                          }
                        </td>
                        <td>
                          {customer.total}
                        </td>
                        <td>
                          {customer.open}
                        </td>
                        <td>
                          {
                            customer.completed
                          }
                        </td>
                      </tr>
                    ),
                  )}
                </tbody>
              </table>
            </div>
          </section>

          <p className="report-generated">
            Generated{' '}
            {new Date(
              report.generatedAt,
            ).toLocaleString()}
          </p>
        </>
      ) : null}
    </div>
  )
}

function MetricCard({
  label,
  value,
  detail,
}: {
  label: string
  value: string | number
  detail: string
}) {
  return (
    <article className="metric-card">
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{detail}</small>
    </article>
  )
}

function CountPanel({
  title,
  items,
}: {
  title: string
  items: NamedCount[]
}) {
  const maximum =
    Math.max(
      ...items.map(
        (item) => item.count,
      ),
      1,
    )

  return (
    <article className="panel">
      <div className="panel-heading">
        <h2>{title}</h2>
      </div>

      <div className="report-bars">
        {items.map((item) => (
          <div
            key={item.name}
            className="report-bar-row"
          >
            <div>
              <span>{item.name}</span>
              <strong>
                {item.count}
              </strong>
            </div>
            <div className="report-bar-track">
              <span
                style={{
                  width: `${
                    item.count === 0
                      ? 0
                      : Math.max(
                          8,
                          item.count /
                            maximum *
                            100,
                        )
                  }%`,
                }}
              />
            </div>
          </div>
        ))}
      </div>
    </article>
  )
}
