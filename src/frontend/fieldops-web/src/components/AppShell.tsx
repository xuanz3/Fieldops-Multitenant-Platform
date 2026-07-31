import {
  NavLink,
  Outlet,
} from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import type { UserRole } from '../types'

interface NavigationItem {
  to: string
  label: string
  end?: boolean
  roles: UserRole[]
}

const navigation: NavigationItem[] = [
  {
    to: '/',
    label: 'Dashboard',
    end: true,
    roles: [
      'TenantAdmin',
      'Dispatcher',
      'Technician',
      'Client',
    ],
  },
  {
    to: '/customers',
    label: 'Customers',
    roles: [
      'TenantAdmin',
      'Dispatcher',
    ],
  },
  {
    to: '/work-orders',
    label: 'Work orders',
    roles: [
      'TenantAdmin',
      'Dispatcher',
    ],
  },
  {
    to: '/dispatch',
    label: 'Dispatch',
    roles: [
      'TenantAdmin',
      'Dispatcher',
    ],
  },
  {
    to: '/technician',
    label: 'My work',
    roles: [
      'TenantAdmin',
      'Technician',
    ],
  },
  {
    to: '/client-approvals',
    label: 'Client approvals',
    roles: [
      'TenantAdmin',
      'Client',
    ],
  },
  {
    to: '/evidence',
    label: 'Evidence',
    roles: [
      'TenantAdmin',
      'Dispatcher',
      'Technician',
      'Client',
    ],
  },
  {
    to: '/audit-log',
    label: 'Audit log',
    roles: [
      'TenantAdmin',
      'Dispatcher',
    ],
  },
  {
    to: '/reports',
    label: 'Reports',
    roles: [
      'TenantAdmin',
      'Dispatcher',
    ],
  },
]

export function AppShell() {
  const { session, signOut } =
    useAuth()

  if (!session) {
    return null
  }

  const visibleNavigation =
    navigation.filter((item) =>
      item.roles.includes(
        session.user.role,
      ),
    )

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
          {visibleNavigation.map(
            (item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className={({
                  isActive,
                }) =>
                  isActive
                    ? 'active'
                    : undefined
                }
              >
                {item.label}
              </NavLink>
            ),
          )}
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
