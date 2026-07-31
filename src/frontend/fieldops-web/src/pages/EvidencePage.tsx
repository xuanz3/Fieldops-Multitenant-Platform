import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from 'react'
import { ApiError } from '../api/client'
import {
  downloadAttachment,
  listAttachments,
  listClientWorkOrders,
  listTechnicianWorkOrders,
  listWorkOrders,
  uploadAttachment,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import type {
  WorkOrder,
  WorkOrderAttachment,
} from '../types'

export function EvidencePage() {
  const { session } = useAuth()
  const [workOrders, setWorkOrders] =
    useState<WorkOrder[]>([])
  const [
    selectedWorkOrderId,
    setSelectedWorkOrderId,
  ] = useState('')
  const [attachments, setAttachments] =
    useState<WorkOrderAttachment[]>([])
  const [selectedFile, setSelectedFile] =
    useState<File | null>(null)
  const [loading, setLoading] =
    useState(true)
  const [saving, setSaving] =
    useState(false)
  const [error, setError] =
    useState<string | null>(null)
  const [message, setMessage] =
    useState<string | null>(null)

  const canUpload =
    session?.user.role !== 'Client'

  const selectedWorkOrder =
    useMemo(
      () =>
        workOrders.find(
          (item) =>
            item.id ===
            selectedWorkOrderId,
        ) ?? null,
      [
        workOrders,
        selectedWorkOrderId,
      ],
    )

  const loadWorkOrders =
    useCallback(async () => {
      if (!session) {
        return
      }

      setLoading(true)
      setError(null)

      try {
        let records: WorkOrder[]

        if (
          session.user.role ===
            'TenantAdmin' ||
          session.user.role ===
            'Dispatcher'
        ) {
          const page =
            await listWorkOrders(
              session.accessToken,
              {
                page: 1,
                pageSize: 100,
              },
            )
          records = page.items
        } else if (
          session.user.role ===
          'Technician'
        ) {
          records =
            await listTechnicianWorkOrders(
              session.accessToken,
            )
        } else {
          records =
            await listClientWorkOrders(
              session.accessToken,
            )
        }

        setWorkOrders(records)
        setSelectedWorkOrderId(
          (current) =>
            records.some(
              (item) =>
                item.id === current,
            )
              ? current
              : records[0]?.id ?? '',
        )
      } catch (reason) {
        setError(
          reason instanceof ApiError
            ? reason.message
            : 'Work orders could not be loaded.',
        )
      } finally {
        setLoading(false)
      }
    }, [session])

  const loadAttachments =
    useCallback(async () => {
      if (
        !session ||
        !selectedWorkOrderId
      ) {
        setAttachments([])
        return
      }

      setError(null)

      try {
        setAttachments(
          await listAttachments(
            session.accessToken,
            selectedWorkOrderId,
          ),
        )
      } catch (reason) {
        setError(
          reason instanceof ApiError
            ? reason.message
            : 'Attachments could not be loaded.',
        )
      }
    }, [
      session,
      selectedWorkOrderId,
    ])

  useEffect(() => {
    const timer =
      window.setTimeout(
        () => {
          void loadWorkOrders()
        },
        0,
      )

    return () =>
      window.clearTimeout(timer)
  }, [loadWorkOrders])

  useEffect(() => {
    const timer =
      window.setTimeout(
        () => {
          void loadAttachments()
        },
        0,
      )

    return () =>
      window.clearTimeout(timer)
  }, [loadAttachments])

  async function handleUpload(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (
      !session ||
      !selectedWorkOrderId ||
      !selectedFile
    ) {
      setError(
        'Select a work order and a file.',
      )
      return
    }

    setSaving(true)
    setError(null)
    setMessage(null)

    try {
      const uploaded =
        await uploadAttachment(
          session.accessToken,
          selectedWorkOrderId,
          selectedFile,
        )

      setMessage(
        `${uploaded.fileName} uploaded and hashed.`,
      )
      setSelectedFile(null)
      await loadAttachments()
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The attachment could not be uploaded.',
      )
    } finally {
      setSaving(false)
    }
  }

  async function handleDownload(
    attachment: WorkOrderAttachment,
  ) {
    if (
      !session ||
      !selectedWorkOrderId
    ) {
      return
    }

    setError(null)

    try {
      const blob =
        await downloadAttachment(
          session.accessToken,
          selectedWorkOrderId,
          attachment.id,
        )

      const url =
        URL.createObjectURL(blob)
      const link =
        document.createElement('a')

      link.href = url
      link.download =
        attachment.fileName
      document.body.append(link)
      link.click()
      link.remove()
      URL.revokeObjectURL(url)
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The attachment could not be downloaded.',
      )
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Controlled evidence"
        title="Evidence"
        description="Upload and retrieve tenant-authorised work-order evidence with file-type, size and SHA-256 integrity controls."
        actions={
          <button
            className="button button-secondary"
            type="button"
            onClick={() =>
              void loadWorkOrders()
            }
          >
            Refresh
          </button>
        }
      />

      {error ? (
        <Feedback
          tone="error"
          title="Evidence action failed"
          message={error}
        />
      ) : null}

      {message ? (
        <Feedback
          tone="success"
          title="Evidence saved"
          message={message}
        />
      ) : null}

      {loading ? (
        <Feedback
          title="Loading evidence"
          message="Reading the work orders authorised for this role."
        />
      ) : null}

      {!loading &&
      workOrders.length === 0 ? (
        <Feedback
          title="No authorised work"
          message="This role has no work orders available for evidence access."
        />
      ) : null}

      {!loading &&
      workOrders.length > 0 ? (
        <section className="evidence-layout">
          <article className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Work order
                </p>
                <h2>
                  Evidence record
                </h2>
              </div>
            </div>

            <label className="form-field">
              <span>Select work order</span>
              <select
                value={
                  selectedWorkOrderId
                }
                onChange={(event) =>
                  setSelectedWorkOrderId(
                    event.target.value,
                  )
                }
              >
                {workOrders.map(
                  (workOrder) => (
                    <option
                      key={workOrder.id}
                      value={workOrder.id}
                    >
                      {workOrder.reference}
                      {' — '}
                      {workOrder.title}
                    </option>
                  ),
                )}
              </select>
            </label>

            {selectedWorkOrder ? (
              <div className="evidence-summary">
                <div>
                  <strong>
                    {
                      selectedWorkOrder
                        .customerName
                    }
                  </strong>
                  <span>
                    {
                      selectedWorkOrder
                        .assignedTechnicianName ??
                      'Unassigned'
                    }
                  </span>
                </div>
                <div className="badge-row badge-row-left">
                  <StatusBadge
                    value={
                      selectedWorkOrder
                        .status
                    }
                  />
                  <StatusBadge
                    value={
                      selectedWorkOrder
                        .priority
                    }
                  />
                </div>
              </div>
            ) : null}

            {canUpload ? (
              <form
                className="evidence-upload"
                onSubmit={handleUpload}
              >
                <label className="form-field">
                  <span>
                    Upload evidence
                  </span>
                  <input
                    type="file"
                    accept=".pdf,.png,.jpg,.jpeg,.txt,application/pdf,image/png,image/jpeg,text/plain"
                    onChange={(event) =>
                      setSelectedFile(
                        event.target
                          .files?.[0] ??
                          null,
                      )
                    }
                  />
                </label>

                <small>
                  PDF, PNG, JPEG or TXT.
                  Maximum 5 MB.
                </small>

                <button
                  className="button button-primary button-full"
                  type="submit"
                  disabled={
                    saving ||
                    !selectedFile
                  }
                >
                  {saving
                    ? 'Uploading…'
                    : 'Upload evidence'}
                </button>
              </form>
            ) : (
              <div className="workflow-note">
                <strong>
                  Read-only Client access
                </strong>
                <span>
                  Clients can inspect and
                  download evidence linked to
                  their Customer records.
                </span>
              </div>
            )}
          </article>

          <article className="panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  Integrity records
                </p>
                <h2>
                  Attachments
                </h2>
              </div>
              <span className="count-pill">
                {attachments.length}
              </span>
            </div>

            {attachments.length === 0 ? (
              <Feedback
                title="No attachments"
                message="No evidence has been uploaded for this work order."
              />
            ) : (
              <div className="attachment-list">
                {attachments.map(
                  (attachment) => (
                    <article
                      key={attachment.id}
                      className="attachment-row"
                    >
                      <div>
                        <strong>
                          {
                            attachment.fileName
                          }
                        </strong>
                        <span>
                          {formatBytes(
                            attachment.sizeBytes,
                          )}
                          {' · '}
                          {
                            attachment.uploadedByDisplayName
                          }
                        </span>
                        <code>
                          SHA-256{' '}
                          {attachment.sha256}
                        </code>
                      </div>

                      <button
                        className="button button-secondary"
                        type="button"
                        onClick={() =>
                          void handleDownload(
                            attachment,
                          )
                        }
                      >
                        Download
                      </button>
                    </article>
                  ),
                )}
              </div>
            )}
          </article>
        </section>
      ) : null}
    </div>
  )
}

function formatBytes(
  bytes: number,
): string {
  if (bytes < 1024) {
    return `${bytes} B`
  }

  if (bytes < 1024 * 1024) {
    return `${(
      bytes / 1024
    ).toFixed(1)} KB`
  }

  return `${(
    bytes /
    (1024 * 1024)
  ).toFixed(1)} MB`
}
