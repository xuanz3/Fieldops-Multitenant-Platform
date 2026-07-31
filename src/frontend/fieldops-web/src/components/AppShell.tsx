import {
  NavLink,
  Outlet,
} from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

const navigation = [
  {
    to: '/',
    label: 'Dashboard',
    end: true,
  },
  {
    to: '/customers',
    label: 'Customers',
  },
  {
    to: '/work-orders',
    label: 'Work orders',
  },
]

export function AppShell() {
  const { session, signOut } = useAuth()

  if (!session) {
    return null
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="product-kicker">
            Field service operations
          </p>
          <div className="brand-row">
            <span className="brand-mark">
              FO
            </span>
            <div>
              <strong>FieldOps Hub</strong>
              <span>
                {session.user.tenantName}
              </span>
            </div>
          </div>
        </div>

        <nav
          className="primary-nav"
          aria-label="Primary navigation"
        >
          {navigation.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                isActive ? 'active' : undefined
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-session">
          <div>
            <span className="muted-label">
              Signed in as
            </span>
            <strong>
              {session.user.displayName}
            </strong>
            <span>
              {session.user.role}
            </span>
          </div>
          <button
            className="button button-secondary button-full"
            type="button"
            onClick={signOut}
          >
            Sign out
          </button>
        </div>
      </aside>

      <div className="workspace">
        <header className="mobile-header">
          <strong>FieldOps Hub</strong>
          <button
            className="button button-quiet"
            type="button"
            onClick={signOut}
          >
            Sign out
          </button>
        </header>

        <main className="workspace-main">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
