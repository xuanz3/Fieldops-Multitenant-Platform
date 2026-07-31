import {
  useCallback,
  useEffect,
  useState,
  type FormEvent,
} from 'react'
import { ApiError } from '../api/client'
import {
  createWorkOrder,
  listCustomers,
  listWorkOrders,
  updateWorkOrder,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import {
  InputField,
  SelectField,
  TextAreaField,
} from '../components/FormField'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import { StatusBadge } from '../components/StatusBadge'
import type {
  Customer,
  WorkOrder,
  WorkOrderInput,
  WorkOrderPriority,
  WorkOrderStatus,
} from '../types'

const priorities: WorkOrderPriority[] = [
  'Low',
  'Normal',
  'High',
  'Urgent',
]

const statuses: WorkOrderStatus[] = [
  'Submitted',
  'Assigned',
  'InProgress',
  'AwaitingClientApproval',
  'Completed',
  'Reopened',
  'Cancelled',
]

const emptyInput: WorkOrderInput = {
  customerId: '',
  reference: '',
  title: '',
  description: '',
  priority: 'Normal',
}

export function WorkOrdersPage() {
  const { session } = useAuth()

  const [items, setItems] =
    useState<WorkOrder[]>([])
  const [customers, setCustomers] =
    useState<Customer[]>([])
  const [searchInput, setSearchInput] =
    useState('')
  const [search, setSearch] =
    useState('')
  const [status, setStatus] =
    useState<WorkOrderStatus | ''>('')
  const [priority, setPriority] =
    useState<WorkOrderPriority | ''>('')
  const [page, setPage] =
    useState(1)
  const [totalPages, setTotalPages] =
    useState(0)
  const [totalCount, setTotalCount] =
    useState(0)
  const [loading, setLoading] =
    useState(true)
  const [error, setError] =
    useState<string | null>(null)
  const [formOpen, setFormOpen] =
    useState(false)
  const [editing, setEditing] =
    useState<WorkOrder | null>(null)
  const [input, setInput] =
    useState<WorkOrderInput>(emptyInput)
  const [formError, setFormError] =
    useState<string | null>(null)
  const [fieldErrors, setFieldErrors] =
    useState<Record<string, string[]>>({})
  const [saving, setSaving] =
    useState(false)

  const load = useCallback(async () => {
    if (!session) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      const [workOrders, customerPage] =
        await Promise.all([
          listWorkOrders(
            session.accessToken,
            {
              search,
              status,
              priority,
              page,
              pageSize: 10,
            },
          ),
          listCustomers(
            session.accessToken,
            {
              page: 1,
              pageSize: 100,
            },
          ),
        ])

      setItems(workOrders.items)
      setTotalPages(
        workOrders.totalPages,
      )
      setTotalCount(
        workOrders.totalCount,
      )
      setCustomers(customerPage.items)
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'Work orders could not be loaded.',
      )
    } finally {
      setLoading(false)
    }
  }, [
    page,
    priority,
    search,
    session,
    status,
  ])

  useEffect(() => {
    const loadTimer =
      window.setTimeout(() => {
        void load()
      }, 0)

    return () =>
      window.clearTimeout(loadTimer)
  }, [load])

  function submitSearch(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setPage(1)
    setSearch(searchInput.trim())
  }

  function openCreate() {
    setEditing(null)
    setInput({
      ...emptyInput,
      customerId:
        customers[0]?.id ?? '',
    })
    setFormError(null)
    setFieldErrors({})
    setFormOpen(true)
  }

  function openEdit(
    workOrder: WorkOrder,
  ) {
    setEditing(workOrder)
    setInput({
      customerId:
        workOrder.customerId,
      reference:
        workOrder.reference,
      title: workOrder.title,
      description:
        workOrder.description ?? '',
      priority:
        workOrder.priority,
    })
    setFormError(null)
    setFieldErrors({})
    setFormOpen(true)
  }

  function closeForm() {
    if (saving) {
      return
    }

    setFormOpen(false)
    setEditing(null)
    setInput(emptyInput)
    setFormError(null)
    setFieldErrors({})
  }

  async function save(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (!session) {
      return
    }

    setSaving(true)
    setFormError(null)
    setFieldErrors({})

    try {
      if (editing) {
        await updateWorkOrder(
          session.accessToken,
          editing.id,
          {
            customerId:
              input.customerId,
            title: input.title,
            description:
              input.description,
            priority:
              input.priority,
            version:
              editing.version,
          },
        )
      } else {
        await createWorkOrder(
          session.accessToken,
          input,
        )
      }

      setFormOpen(false)
      setEditing(null)
      setInput(emptyInput)
      await load()
    } catch (reason) {
      if (reason instanceof ApiError) {
        setFormError(
          reason.status === 409
            ? `${reason.message} Reload the latest record before trying again.`
            : reason.message,
        )
        setFieldErrors(
          reason.fieldErrors,
        )
      } else {
        setFormError(
          'The work order could not be saved.',
        )
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Dispatch workspace"
        title="Work orders"
        description="Search, filter and maintain work orders within the authenticated tenant boundary."
        actions={
          <button
            className="button button-primary"
            type="button"
            onClick={openCreate}
            disabled={
              customers.length === 0
            }
          >
            Add work order
          </button>
        }
      />

      <section className="panel">
        <form
          className="filter-toolbar"
          onSubmit={submitSearch}
        >
          <label
            className="search-field"
            htmlFor="work-order-search"
          >
            <span className="sr-only">
              Search work orders
            </span>
            <input
              id="work-order-search"
              type="search"
              placeholder="Search reference, title or customer"
              value={searchInput}
              onChange={(event) =>
                setSearchInput(
                  event.target.value,
                )
              }
            />
          </label>

          <label className="compact-field">
            <span>Status</span>
            <select
              value={status}
              onChange={(event) => {
                setStatus(
                  event.target
                    .value as
                    | WorkOrderStatus
                    | '',
                )
                setPage(1)
              }}
            >
              <option value="">
                All statuses
              </option>
              {statuses.map((value) => (
                <option
                  key={value}
                  value={value}
                >
                  {value}
                </option>
              ))}
            </select>
          </label>

          <label className="compact-field">
            <span>Priority</span>
            <select
              value={priority}
              onChange={(event) => {
                setPriority(
                  event.target
                    .value as
                    | WorkOrderPriority
                    | '',
                )
                setPage(1)
              }}
            >
              <option value="">
                All priorities
              </option>
              {priorities.map((value) => (
                <option
                  key={value}
                  value={value}
                >
                  {value}
                </option>
              ))}
            </select>
          </label>

          <button
            className="button button-secondary"
            type="submit"
          >
            Search
          </button>

          {search ||
          status ||
          priority ? (
            <button
              className="button button-quiet"
              type="button"
              onClick={() => {
                setSearchInput('')
                setSearch('')
                setStatus('')
                setPriority('')
                setPage(1)
              }}
            >
              Clear
            </button>
          ) : null}
        </form>
      </section>

      {error ? (
        <Feedback
          tone="error"
          title="Work orders unavailable"
          message={error}
        />
      ) : null}

      {customers.length === 0 &&
      !loading ? (
        <Feedback
          title="Customer required"
          message="Create a customer before creating a work order."
        />
      ) : null}

      <section className="panel">
        {loading ? (
          <Feedback
            title="Loading work orders"
            message="Reading the current tenant work queue."
          />
        ) : null}

        {!loading &&
        items.length === 0 ? (
          <Feedback
            title="No matching work orders"
            message="Adjust the filters or create a new work order."
          />
        ) : null}

        {!loading &&
        items.length > 0 ? (
          <div className="work-order-grid">
            {items.map((workOrder) => (
              <article
                key={workOrder.id}
                className="work-order-card"
              >
                <div className="work-order-card-heading">
                  <div>
                    <span className="record-reference">
                      {workOrder.reference}
                    </span>
                    <h2>
                      {workOrder.title}
                    </h2>
                  </div>
                  <button
                    className="button button-quiet"
                    type="button"
                    onClick={() =>
                      openEdit(workOrder)
                    }
                  >
                    Edit
                  </button>
                </div>

                <p>
                  {workOrder.description ??
                    'No description provided.'}
                </p>

                <dl className="record-details">
                  <div>
                    <dt>Customer</dt>
                    <dd>
                      {
                        workOrder.customerName
                      }
                    </dd>
                  </div>
                  <div>
                    <dt>Version</dt>
                    <dd>
                      {workOrder.version}
                    </dd>
                  </div>
                  <div>
                    <dt>Updated</dt>
                    <dd>
                      {new Date(
                        workOrder.updatedAt,
                      ).toLocaleDateString()}
                    </dd>
                  </div>
                </dl>

                <div className="badge-row">
                  <StatusBadge
                    value={workOrder.status}
                  />
                  <StatusBadge
                    value={
                      workOrder.priority
                    }
                  />
                </div>
              </article>
            ))}
          </div>
        ) : null}

        {!loading ? (
          <Pagination
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={setPage}
          />
        ) : null}
      </section>

      {formOpen ? (
        <div
          className="overlay"
          role="presentation"
          onMouseDown={(event) => {
            if (
              event.target ===
              event.currentTarget
            ) {
              closeForm()
            }
          }}
        >
          <section
            className="drawer drawer-wide"
            role="dialog"
            aria-modal="true"
            aria-labelledby="work-order-form-title"
          >
            <div className="drawer-heading">
              <div>
                <p className="eyebrow">
                  Work order record
                </p>
                <h2 id="work-order-form-title">
                  {editing
                    ? 'Edit work order'
                    : 'Add work order'}
                </h2>
              </div>
              <button
                className="button button-quiet"
                type="button"
                onClick={closeForm}
              >
                Close
              </button>
            </div>

            <form
              className="stack-form"
              onSubmit={save}
            >
              <SelectField
                label="Customer"
                name="customerId"
                value={input.customerId}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    customerId:
                      event.target.value,
                  }))
                }
                error={
                  fieldErrors.customerId?.[0]
                }
                required
              >
                <option value="">
                  Select a customer
                </option>
                {customers.map(
                  (customer) => (
                    <option
                      key={customer.id}
                      value={customer.id}
                    >
                      {customer.reference} ·{' '}
                      {customer.name}
                    </option>
                  ),
                )}
              </SelectField>

              <InputField
                label="Reference"
                name="reference"
                value={input.reference}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    reference:
                      event.target.value,
                  }))
                }
                disabled={Boolean(editing)}
                hint={
                  editing
                    ? 'References cannot be changed after creation.'
                    : 'Letters, numbers, hyphens and underscores only.'
                }
                error={
                  fieldErrors.reference?.[0]
                }
                required
              />

              <InputField
                label="Title"
                name="title"
                value={input.title}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    title:
                      event.target.value,
                  }))
                }
                error={
                  fieldErrors.title?.[0]
                }
                required
              />

              <TextAreaField
                label="Description"
                name="description"
                rows={5}
                value={input.description}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    description:
                      event.target.value,
                  }))
                }
                error={
                  fieldErrors.description?.[0]
                }
              />

              <SelectField
                label="Priority"
                name="priority"
                value={input.priority}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    priority:
                      event.target
                        .value as WorkOrderPriority,
                  }))
                }
                error={
                  fieldErrors.priority?.[0]
                }
              >
                {priorities.map((value) => (
                  <option
                    key={value}
                    value={value}
                  >
                    {value}
                  </option>
                ))}
              </SelectField>

              {editing ? (
                <div className="version-note">
                  Editing version{' '}
                  <strong>
                    {editing.version}
                  </strong>
                  . A newer server version will
                  return a conflict instead of
                  being overwritten.
                </div>
              ) : null}

              {formError ? (
                <div
                  className="inline-error"
                  role="alert"
                >
                  {formError}
                </div>
              ) : null}

              <div className="form-actions">
                <button
                  className="button button-secondary"
                  type="button"
                  onClick={closeForm}
                >
                  Cancel
                </button>
                <button
                  className="button button-primary"
                  type="submit"
                  disabled={saving}
                >
                  {saving
                    ? 'Saving…'
                    : editing
                      ? 'Save changes'
                      : 'Create work order'}
                </button>
              </div>
            </form>
          </section>
        </div>
      ) : null}
    </div>
  )
}
