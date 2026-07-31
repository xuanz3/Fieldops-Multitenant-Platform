import {
  useState,
  type FormEvent,
} from 'react'
import {
  Navigate,
  useLocation,
  useNavigate,
} from 'react-router-dom'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/useAuth'
import { InputField } from '../components/FormField'

interface LocationState {
  from?: string
}

const demo = {
  tenantSlug:
    'northside-property-services',
  dispatcher:
    'dispatcher@northside.example.test',
  admin:
    'admin@northside.example.test',
  technician:
    'technician@northside.example.test',
  client:
    'client@northside.example.test',
  password: 'FieldOps-Demo-2026!',
}

type DemoAccount =
  | 'dispatcher'
  | 'admin'
  | 'technician'
  | 'client'

export function LoginPage() {
  const {
    signIn,
    isAuthenticated,
  } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [tenantSlug, setTenantSlug] =
    useState(demo.tenantSlug)
  const [email, setEmail] =
    useState(demo.dispatcher)
  const [password, setPassword] =
    useState(demo.password)
  const [error, setError] =
    useState<string | null>(null)
  const [submitting, setSubmitting] =
    useState(false)

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  const destination =
    (
      location.state as
        | LocationState
        | null
    )?.from ?? '/'

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)

    try {
      await signIn(
        tenantSlug,
        email,
        password,
      )

      navigate(destination, {
        replace: true,
      })
    } catch (reason) {
      setError(
        reason instanceof ApiError
          ? reason.message
          : 'The sign-in request could not be completed.',
      )
    } finally {
      setSubmitting(false)
    }
  }

  function chooseAccount(
    account: DemoAccount,
  ) {
    setTenantSlug(demo.tenantSlug)
    setEmail(demo[account])
    setPassword(demo.password)
    setError(null)
  }

  return (
    <main className="login-page">
      <section className="login-intro">
        <p className="eyebrow">
          Multi-tenant field operations
        </p>
        <h1>FieldOps Hub</h1>
        <p>
          A secure role-based workflow from
          dispatch through field execution and
          client approval.
        </p>

        <div className="login-feature-grid">
          <article>
            <strong>Dispatcher control</strong>
            <span>
              Link clients, assign technicians
              and manage operational records.
            </span>
          </article>
          <article>
            <strong>Technician execution</strong>
            <span>
              Start assigned work and submit
              completion notes for approval.
            </span>
          </article>
          <article>
            <strong>Client decision</strong>
            <span>
              Approve completed work or reopen
              it with a recorded reason.
            </span>
          </article>
        </div>
      </section>

      <section
        className="login-card"
        aria-labelledby="login-title"
      >
        <div>
          <p className="eyebrow">
            Demonstration access
          </p>
          <h2 id="login-title">
            Sign in
          </h2>
          <p className="muted">
            Choose one of four fictional roles.
          </p>
        </div>

        <div className="demo-account-grid">
          <button
            type="button"
            className="button button-secondary"
            onClick={() =>
              chooseAccount('dispatcher')
            }
          >
            Dispatcher
          </button>
          <button
            type="button"
            className="button button-secondary"
            onClick={() =>
              chooseAccount('technician')
            }
          >
            Technician
          </button>
          <button
            type="button"
            className="button button-secondary"
            onClick={() =>
              chooseAccount('client')
            }
          >
            Client
          </button>
          <button
            type="button"
            className="button button-secondary"
            onClick={() =>
              chooseAccount('admin')
            }
          >
            Tenant Admin
          </button>
        </div>

        <form
          className="stack-form"
          onSubmit={handleSubmit}
        >
          <InputField
            label="Tenant slug"
            name="tenantSlug"
            autoComplete="organization"
            value={tenantSlug}
            onChange={(event) =>
              setTenantSlug(
                event.target.value,
              )
            }
            required
          />

          <InputField
            label="Email"
            name="email"
            type="email"
            autoComplete="username"
            value={email}
            onChange={(event) =>
              setEmail(
                event.target.value,
              )
            }
            required
          />

          <InputField
            label="Password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) =>
              setPassword(
                event.target.value,
              )
            }
            required
          />

          {error ? (
            <div
              className="inline-error"
              role="alert"
            >
              {error}
            </div>
          ) : null}

          <button
            className="button button-primary button-full"
            type="submit"
            disabled={submitting}
          >
            {submitting
              ? 'Signing in…'
              : 'Sign in to workspace'}
          </button>
        </form>
      </section>
    </main>
  )
}
