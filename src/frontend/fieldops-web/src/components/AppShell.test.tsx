import {
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  render,
  screen,
} from '@testing-library/react'
import {
  MemoryRouter,
  Route,
  Routes,
} from 'react-router'
import { AuthContext } from '../auth/AuthContext'
import { AppShell } from './AppShell'

describe('AppShell role navigation', () => {
  it('shows Technician navigation without Dispatcher pages', () => {
    render(
      <AuthContext.Provider
        value={{
          session: {
            accessToken: 'token',
            expiresAt:
              new Date(
                Date.now() + 60_000,
              ).toISOString(),
            user: {
              id: 'technician-1',
              tenantId: 'tenant-1',
              tenantSlug: 'northside',
              tenantName: 'Northside',
              email:
                'technician@example.test',
              displayName:
                'Test Technician',
              role: 'Technician',
            },
          },
          isAuthenticated: true,
          signIn: vi.fn(),
          signOut: vi.fn(),
        }}
      >
        <MemoryRouter>
          <Routes>
            <Route
              element={<AppShell />}
            >
              <Route
                index
                element={<div>Home</div>}
              />
            </Route>
          </Routes>
        </MemoryRouter>
      </AuthContext.Provider>,
    )

    expect(
      screen.getByRole('link', {
        name: 'My work',
      }),
    ).toBeInTheDocument()

    expect(
      screen.queryByRole('link', {
        name: 'Customers',
      }),
    ).not.toBeInTheDocument()

    expect(
      screen.queryByRole('link', {
        name: 'Dispatch',
      }),
    ).not.toBeInTheDocument()
  })

  it('shows Client approvals for Client role', () => {
    render(
      <AuthContext.Provider
        value={{
          session: {
            accessToken: 'token',
            expiresAt:
              new Date(
                Date.now() + 60_000,
              ).toISOString(),
            user: {
              id: 'client-1',
              tenantId: 'tenant-1',
              tenantSlug: 'northside',
              tenantName: 'Northside',
              email:
                'client@example.test',
              displayName:
                'Test Client',
              role: 'Client',
            },
          },
          isAuthenticated: true,
          signIn: vi.fn(),
          signOut: vi.fn(),
        }}
      >
        <MemoryRouter>
          <Routes>
            <Route
              element={<AppShell />}
            >
              <Route
                index
                element={<div>Home</div>}
              />
            </Route>
          </Routes>
        </MemoryRouter>
      </AuthContext.Provider>,
    )

    expect(
      screen.getByRole('link', {
        name: 'Client approvals',
      }),
    ).toBeInTheDocument()

    expect(
      screen.queryByRole('link', {
        name: 'Work orders',
      }),
    ).not.toBeInTheDocument()
  })
})
