import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { ApiError } from '../api/client'
import {
  listTechnicianWorkOrders,
  startWorkOrder,
  submitWorkOrder,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import type { WorkOrder } from '../types'

export function TechnicianPage() {
  const { session } = useAuth()

  const [items, setItems] =
    useState<WorkOrder[]>([])
  const [summaries, setSummaries] =
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
      const workOrders =
        await listTechnicianWorkOrders(
          session.accessToken,
        )

      setItems(workOrders)
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The technician queue could not be loaded.',
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

  async function start(
    workOrder: WorkOrder,
  ) {
    if (!session) {
      return
    }

    setSavingId(workOrder.id)
    setError(null)

    try {
      await startWorkOrder(
        session.accessToken,
        workOrder.id,
        workOrder.version,
      )
      await load()
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The work order could not be started.',
      )
    } finally {
      setSavingId(null)
    }
  }

  async function submit(
    workOrder: WorkOrder,
  ) {
    if (!session) {
      return
    }

    const summary =
      summaries[workOrder.id]?.trim()

    if (!summary) {
      setError(
        'Enter a completion summary before submitting.',
      )
      return
    }

    setSavingId(workOrder.id)
    setError(null)

    try {
      await submitWorkOrder(
        session.accessToken,
        workOrder.id,
        summary,
        workOrder.version,
      )
      setSummaries((current) => ({
        ...current,
        [workOrder.id]: '',
      }))
      await load()
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The work order could not be submitted.',
      )
    } finally {
      setSavingId(null)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Technician workflow"
        title="My work"
        description="Start assigned tasks and submit clear completion notes for Client review."
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
          title="Technician action failed"
          message={error}
        />
      ) : null}

      {loading ? (
        <Feedback
          title="Loading assigned work"
          message="Reading the signed Technician queue."
        />
      ) : null}

      {!loading &&
      items.length === 0 ? (
        <Feedback
          title="No assigned work"
          message="There are no work orders assigned to this Technician."
        />
      ) : null}

      {!loading &&
      items.length > 0 ? (
        <section className="workflow-card-list workflow-card-list-wide">
          {items.map((workOrder) => (
            <article
              key={workOrder.id}
              className="workflow-card"
            >
              <div>
                <span className="record-reference">
                  {workOrder.reference}
                </span>
                <h2>{workOrder.title}</h2>
                <p>
                  {workOrder.description ??
                    'No description provided.'}
                </p>
              </div>

              <dl className="record-details">
                <div>
                  <dt>Customer</dt>
                  <dd>
                    {workOrder.customerName}
                  </dd>
                </div>
                <div>
                  <dt>Version</dt>
                  <dd>
                    {workOrder.version}
                  </dd>
                </div>
                <div>
                  <dt>Assigned</dt>
                  <dd>
                    {workOrder.assignedAt
                      ? new Date(
                          workOrder.assignedAt,
                        ).toLocaleDateString()
                      : '—'}
                  </dd>
                </div>
              </dl>

              <div className="badge-row badge-row-left">
                <StatusBadge
                  value={workOrder.status}
                />
                <StatusBadge
                  value={workOrder.priority}
                />
              </div>

              {workOrder.status ===
              'Assigned' ? (
                <button
                  className="button button-primary button-full"
                  type="button"
                  disabled={
                    savingId ===
                    workOrder.id
                  }
                  onClick={() =>
                    void start(workOrder)
                  }
                >
                  {savingId ===
                  workOrder.id
                    ? 'Starting…'
                    : 'Start work'}
                </button>
              ) : null}

              {workOrder.status ===
              'InProgress' ? (
                <div className="workflow-action-stack">
                  <label className="form-field">
                    <span>
                      Completion summary
                    </span>
                    <textarea
                      rows={4}
                      value={
                        summaries[
                          workOrder.id
                        ] ?? ''
                      }
                      onChange={(event) =>
                        setSummaries(
                          (current) => ({
                            ...current,
                            [workOrder.id]:
                              event.target
                                .value,
                          }),
                        )
                      }
                      placeholder="Describe the work completed and checks performed."
                    />
                  </label>

                  <button
                    className="button button-primary button-full"
                    type="button"
                    disabled={
                      savingId ===
                      workOrder.id
                    }
                    onClick={() =>
                      void submit(
                        workOrder,
                      )
                    }
                  >
                    {savingId ===
                    workOrder.id
                      ? 'Submitting…'
                      : 'Submit for Client approval'}
                  </button>
                </div>
              ) : null}

              {workOrder.status ===
              'AwaitingClientApproval' ? (
                <div className="workflow-note">
                  <strong>
                    Awaiting Client decision
                  </strong>
                  <span>
                    {
                      workOrder.completionSummary
                    }
                  </span>
                </div>
              ) : null}

              {workOrder.status ===
              'Completed' ? (
                <div className="workflow-note workflow-note-success">
                  <strong>
                    Client approved
                  </strong>
                  <span>
                    This work order is complete.
                  </span>
                </div>
              ) : null}
            </article>
          ))}
        </section>
      ) : null}
    </div>
  )
}
