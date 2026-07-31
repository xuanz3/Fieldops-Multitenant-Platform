import {
  useCallback,
  useEffect,
  useState,
  type FormEvent,
} from 'react'
import { ApiError } from '../api/client'
import {
  createCustomer,
  listCustomers,
  updateCustomer,
} from '../api/fieldOpsApi'
import { useAuth } from '../auth/useAuth'
import { Feedback } from '../components/Feedback'
import { InputField } from '../components/FormField'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import type {
  Customer,
  CustomerInput,
} from '../types'

const emptyInput: CustomerInput = {
  reference: '',
  name: '',
  email: '',
}

export function CustomersPage() {
  const { session } = useAuth()

  const [items, setItems] =
    useState<Customer[]>([])
  const [searchInput, setSearchInput] =
    useState('')
  const [search, setSearch] =
    useState('')
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
    useState<Customer | null>(null)
  const [input, setInput] =
    useState<CustomerInput>(emptyInput)
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
      const result = await listCustomers(
        session.accessToken,
        {
          search,
          page,
          pageSize: 10,
        },
      )

      setItems(result.items)
      setTotalPages(result.totalPages)
      setTotalCount(result.totalCount)
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'Customers could not be loaded.',
      )
    } finally {
      setLoading(false)
    }
  }, [page, search, session])

  useEffect(() => {
    void load()
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
    setInput(emptyInput)
    setFormError(null)
    setFieldErrors({})
    setFormOpen(true)
  }

  function openEdit(customer: Customer) {
    setEditing(customer)
    setInput({
      reference:
        customer.reference,
      name: customer.name,
      email: customer.email ?? '',
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
        await updateCustomer(
          session.accessToken,
          editing.id,
          {
            name: input.name,
            email: input.email,
          },
        )
      } else {
        await createCustomer(
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
        setFormError(reason.message)
        setFieldErrors(
          reason.fieldErrors,
        )
      } else {
        setFormError(
          'The customer could not be saved.',
        )
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Tenant directory"
        title="Customers"
        description="Search and maintain the customer records available to this signed tenant."
        actions={
          <button
            className="button button-primary"
            type="button"
            onClick={openCreate}
          >
            Add customer
          </button>
        }
      />

      <section className="panel">
        <form
          className="toolbar"
          onSubmit={submitSearch}
        >
          <label
            className="search-field"
            htmlFor="customer-search"
          >
            <span className="sr-only">
              Search customers
            </span>
            <input
              id="customer-search"
              type="search"
              placeholder="Search reference, name or email"
              value={searchInput}
              onChange={(event) =>
                setSearchInput(
                  event.target.value,
                )
              }
            />
          </label>
          <button
            className="button button-secondary"
            type="submit"
          >
            Search
          </button>
          {search ? (
            <button
              className="button button-quiet"
              type="button"
              onClick={() => {
                setSearchInput('')
                setSearch('')
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
          title="Customers unavailable"
          message={error}
        />
      ) : null}

      <section className="panel">
        {loading ? (
          <Feedback
            title="Loading customers"
            message="Reading the tenant customer directory."
          />
        ) : null}

        {!loading &&
        items.length === 0 ? (
          <Feedback
            title="No matching customers"
            message="Adjust the search or create a new customer."
          />
        ) : null}

        {!loading &&
        items.length > 0 ? (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Reference</th>
                  <th>Customer</th>
                  <th>Email</th>
                  <th>Updated</th>
                  <th>
                    <span className="sr-only">
                      Actions
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {items.map((customer) => (
                  <tr key={customer.id}>
                    <td>
                      <strong>
                        {customer.reference}
                      </strong>
                    </td>
                    <td>{customer.name}</td>
                    <td>
                      {customer.email ?? '—'}
                    </td>
                    <td>
                      {new Date(
                        customer.updatedAt,
                      ).toLocaleDateString()}
                    </td>
                    <td className="table-action">
                      <button
                        className="button button-quiet"
                        type="button"
                        onClick={() =>
                          openEdit(customer)
                        }
                      >
                        Edit
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
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
            className="drawer"
            role="dialog"
            aria-modal="true"
            aria-labelledby="customer-form-title"
          >
            <div className="drawer-heading">
              <div>
                <p className="eyebrow">
                  Customer record
                </p>
                <h2 id="customer-form-title">
                  {editing
                    ? 'Edit customer'
                    : 'Add customer'}
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
                label="Customer name"
                name="name"
                value={input.name}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    name:
                      event.target.value,
                  }))
                }
                error={
                  fieldErrors.name?.[0]
                }
                required
              />

              <InputField
                label="Email"
                name="email"
                type="email"
                value={input.email}
                onChange={(event) =>
                  setInput((current) => ({
                    ...current,
                    email:
                      event.target.value,
                  }))
                }
                error={
                  fieldErrors.email?.[0]
                }
              />

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
                      : 'Create customer'}
                </button>
              </div>
            </form>
          </section>
        </div>
      ) : null}
    </div>
  )
}
