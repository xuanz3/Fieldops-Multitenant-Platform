import {
  useCallback,
  useEffect,
  useState,
  type FormEvent,
} from 'react'
import { ApiError } from '../api/client'
import {
  listAuditEvents,
  verifyAuditChain,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import type {
  AuditEvent,
  AuditVerification,
  PagedResponse,
} from '../types'

const actions = [
  '',
  'CustomerCreated',
  'CustomerUpdated',
  'CustomerClientLinked',
  'WorkOrderCreated',
  'WorkOrderUpdated',
  'WorkOrderAssigned',
  'WorkOrderStarted',
  'WorkOrderSubmitted',
  'WorkOrderApproved',
  'WorkOrderReopened',
  'AttachmentUploaded',
  'AuditChainEnabled',
  'ReportingEnabled',
]

export function AuditLogPage() {
  const { session } = useAuth()
  const [searchInput, setSearchInput] =
    useState('')
  const [search, setSearch] =
    useState('')
  const [action, setAction] =
    useState('')
  const [page, setPage] =
    useState(1)
  const [result, setResult] =
    useState<PagedResponse<AuditEvent> | null>(
      null,
    )
  const [
    verification,
    setVerification,
  ] =
    useState<AuditVerification | null>(
      null,
    )
  const [loading, setLoading] =
    useState(true)
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
        const [
          events,
          chain,
        ] = await Promise.all([
          listAuditEvents(
            session.accessToken,
            {
              search,
              action,
              page,
              pageSize: 20,
            },
          ),
          verifyAuditChain(
            session.accessToken,
          ),
        ])

        setResult(events)
        setVerification(chain)
      } catch (reason) {
        setError(
          reason instanceof ApiError
            ? reason.message
            : 'The audit log could not be loaded.',
        )
      } finally {
        setLoading(false)
      }
    }, [
      action,
      page,
      search,
      session,
    ])

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

  function handleSearch(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setPage(1)
    setSearch(
      searchInput.trim(),
    )
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Append-only evidence"
        title="Audit log"
        description="Review tenant-scoped business events and verify the complete SHA-256 chain from genesis to the latest sequence."
        actions={
          <button
            className="button button-secondary"
            type="button"
            onClick={() =>
              void load()
            }
          >
            Verify again
          </button>
        }
      />

      {verification ? (
        <Feedback
          tone={
            verification.isValid
              ? 'success'
              : 'error'
          }
          title={
            verification.isValid
              ? 'Audit chain verified'
              : 'Audit chain failed'
          }
          message={
            verification.isValid
              ? `${verification.eventCount} events verified from sequence ${verification.firstSequence ?? '—'} to ${verification.lastSequence ?? '—'}.`
              : verification.failure ??
                'The chain could not be verified.'
          }
        />
      ) : null}

      {error ? (
        <Feedback
          tone="error"
          title="Audit log unavailable"
          message={error}
        />
      ) : null}

      <section className="panel">
        <form
          className="filter-bar audit-filter"
          onSubmit={handleSearch}
        >
          <label className="form-field">
            <span>Search</span>
            <input
              value={searchInput}
              onChange={(event) =>
                setSearchInput(
                  event.target.value,
                )
              }
              placeholder="Actor, entity or summary"
            />
          </label>

          <label className="form-field">
            <span>Action</span>
            <select
              value={action}
              onChange={(event) => {
                setPage(1)
                setAction(
                  event.target.value,
                )
              }}
            >
              <option value="">
                All actions
              </option>
              {actions
                .filter(Boolean)
                .map((value) => (
                  <option
                    key={value}
                    value={value}
                  >
                    {humanise(value)}
                  </option>
                ))}
            </select>
          </label>

          <button
            className="button button-primary"
            type="submit"
          >
            Apply
          </button>
        </form>
      </section>

      {loading ? (
        <Feedback
          title="Loading audit events"
          message="Reading the immutable tenant chain."
        />
      ) : null}

      {!loading &&
      result?.items.length === 0 ? (
        <Feedback
          title="No audit events"
          message="No events match the current filters."
        />
      ) : null}

      {!loading &&
      result &&
      result.items.length > 0 ? (
        <section className="panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">
                Tenant chain
              </p>
              <h2>
                Recorded events
              </h2>
            </div>
            <span className="count-pill">
              {result.totalCount}
            </span>
          </div>

          <div className="audit-list">
            {result.items.map(
              (event) => (
                <article
                  key={event.id}
                  className="audit-row"
                >
                  <div className="audit-sequence">
                    #{event.sequence}
                  </div>

                  <div className="audit-content">
                    <div className="audit-title-row">
                      <strong>
                        {humanise(
                          event.action,
                        )}
                      </strong>
                      <span>
                        {event.actorDisplayName}
                        {' · '}
                        {event.actorRole}
                      </span>
                    </div>

                    <p>
                      {event.summary}
                    </p>

                    <div className="audit-meta">
                      <span>
                        {event.entityType}
                      </span>
                      <span>
                        {new Date(
                          event.occurredAt,
                        ).toLocaleString()}
                      </span>
                    </div>

                    <code>
                      {event.eventHash}
                    </code>
                  </div>
                </article>
              ),
            )}
          </div>

          <div className="pagination-row">
            <button
              className="button button-secondary"
              type="button"
              disabled={page <= 1}
              onClick={() =>
                setPage(
                  (current) =>
                    Math.max(
                      1,
                      current - 1,
                    ),
                )
              }
            >
              Previous
            </button>

            <span>
              Page {result.page}
              {' of '}
              {Math.max(
                result.totalPages,
                1,
              )}
            </span>

            <button
              className="button button-secondary"
              type="button"
              disabled={
                page >=
                result.totalPages
              }
              onClick={() =>
                setPage(
                  (current) =>
                    current + 1,
                )
              }
            >
              Next
            </button>
          </div>
        </section>
      ) : null}
    </div>
  )
}

function humanise(
  value: string,
): string {
  return value.replace(
    /([a-z])([A-Z])/g,
    '$1 $2',
  )
}
