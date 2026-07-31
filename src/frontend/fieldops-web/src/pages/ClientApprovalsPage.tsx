import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { ApiError } from '../api/client'
import {
  approveWorkOrder,
  listClientWorkOrders,
  reopenWorkOrder,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import type { WorkOrder } from '../types'

export function ClientApprovalsPage() {
  const { session } = useAuth()

  const [items, setItems] =
    useState<WorkOrder[]>([])
  const [reasons, setReasons] =
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
      setItems(
        await listClientWorkOrders(
          session.accessToken,
        ),
      )
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'Client work orders could not be loaded.',
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

  async function approve(
    workOrder: WorkOrder,
  ) {
    if (!session) {
      return
    }

    setSavingId(workOrder.id)
    setError(null)

    try {
      await approveWorkOrder(
        session.accessToken,
        workOrder.id,
        workOrder.version,
      )
      await load()
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The approval could not be recorded.',
      )
    } finally {
      setSavingId(null)
    }
  }

  async function reopen(
    workOrder: WorkOrder,
  ) {
    if (!session) {
      return
    }

    const reason =
      reasons[workOrder.id]?.trim()

    if (!reason) {
      setError(
        'Enter a reason before reopening the work order.',
      )
      return
    }

    setSavingId(workOrder.id)
    setError(null)

    try {
      await reopenWorkOrder(
        session.accessToken,
        workOrder.id,
        reason,
        workOrder.version,
      )
      setReasons((current) => ({
        ...current,
        [workOrder.id]: '',
      }))
      await load()
    } catch (reasonValue) {
      setError(
        reasonValue instanceof ApiError
          ? reasonValue.message
          : 'The work order could not be reopened.',
      )
    } finally {
      setSavingId(null)
    }
  }

  const awaiting =
    items.filter(
      (item) =>
        item.status ===
        'AwaitingClientApproval',
    )

  const history =
    items.filter(
      (item) =>
        item.status !==
        'AwaitingClientApproval',
    )

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Client workflow"
        title="Client approvals"
        description="Review completion notes for linked Customer records, then approve the work or reopen it with a reason."
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
          title="Client action failed"
          message={error}
        />
      ) : null}

      {loading ? (
        <Feedback
          title="Loading Client work"
          message="Reading records linked to this Client identity."
        />
      ) : null}

      {!loading &&
      awaiting.length === 0 ? (
        <Feedback
          title="Nothing awaiting approval"
          message="No linked work orders currently require a Client decision."
        />
      ) : null}

      {!loading &&
      awaiting.length > 0 ? (
        <section className="workflow-card-list workflow-card-list-wide">
          {awaiting.map((workOrder) => (
            <article
              key={workOrder.id}
              className="workflow-card workflow-card-decision"
            >
              <div>
                <span className="record-reference">
                  {workOrder.reference}
                </span>
                <h2>{workOrder.title}</h2>
                <p>
                  {workOrder.customerName}
                </p>
              </div>

              <div className="workflow-note">
                <strong>
                  Technician completion summary
                </strong>
                <span>
                  {workOrder.completionSummary ??
                    'No completion summary was supplied.'}
                </span>
              </div>

              <div className="badge-row badge-row-left">
                <StatusBadge
                  value={workOrder.status}
                />
                <StatusBadge
                  value={workOrder.priority}
                />
              </div>

              <button
                className="button button-primary button-full"
                type="button"
                disabled={
                  savingId ===
                  workOrder.id
                }
                onClick={() =>
                  void approve(workOrder)
                }
              >
                {savingId ===
                workOrder.id
                  ? 'Saving…'
                  : 'Approve completion'}
              </button>

              <label className="form-field">
                <span>Reopen reason</span>
                <textarea
                  rows={3}
                  value={
                    reasons[
                      workOrder.id
                    ] ?? ''
                  }
                  onChange={(event) =>
                    setReasons(
                      (current) => ({
                        ...current,
                        [workOrder.id]:
                          event.target.value,
                      }),
                    )
                  }
                  placeholder="Explain what still needs attention."
                />
              </label>

              <button
                className="button button-secondary button-full"
                type="button"
                disabled={
                  savingId ===
                  workOrder.id
                }
                onClick={() =>
                  void reopen(workOrder)
                }
              >
                Reopen work order
              </button>
            </article>
          ))}
        </section>
      ) : null}

      {!loading &&
      history.length > 0 ? (
        <section className="panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">
                Linked history
              </p>
              <h2>
                Other work orders
              </h2>
            </div>
          </div>

          <div className="record-list">
            {history.map((workOrder) => (
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
                    {workOrder.customerName}
                  </small>
                </div>
                <div className="record-meta">
                  <StatusBadge
                    value={workOrder.status}
                  />
                </div>
              </article>
            ))}
          </div>
        </section>
      ) : null}
    </div>
  )
}
