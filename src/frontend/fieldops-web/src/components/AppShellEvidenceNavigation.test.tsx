import {
  afterEach,
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  cleanup,
  render,
  screen,
} from '@testing-library/react'
import {
  MemoryRouter,
  Route,
  Routes,
} from 'react-router'
import {
  AuthContext,
} from '../auth/AuthContext'
import { AppShell } from './AppShell'
import type {
  UserRole,
} from '../types'

afterEach(() => {
  cleanup()
})

function renderRole(
  role: UserRole,
) {
  render(
    <AuthContext.Provider
      value={{
        session: {
          accessToken: 'token',
          expiresAt:
            new Date(
              Date.now() +
              60_000,
            ).toISOString(),
          user: {
            id: 'user-1',
            tenantId: 'tenant-1',
            tenantSlug:
              'northside',
            tenantName:
              'Northside',
            email:
              'user@example.test',
            displayName:
              'Test User',
            role,
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
              element={
                <div>Home</div>
              }
            />
          </Route>
        </Routes>
      </MemoryRouter>
    </AuthContext.Provider>,
  )
}

describe(
  'Evidence, audit and reporting navigation',
  () => {
    it('shows evidence, audit and reports to Dispatcher', () => {
      renderRole('Dispatcher')

      expect(
        screen.getByRole(
          'link',
          { name: 'Evidence' },
        ),
      ).toBeInTheDocument()

      expect(
        screen.getByRole(
          'link',
          { name: 'Audit log' },
        ),
      ).toBeInTheDocument()

      expect(
        screen.getByRole(
          'link',
          { name: 'Reports' },
        ),
      ).toBeInTheDocument()
    })

    it('shows evidence but not audit or reports to Client', () => {
      renderRole('Client')

      expect(
        screen.getByRole(
          'link',
          { name: 'Evidence' },
        ),
      ).toBeInTheDocument()

      expect(
        screen.queryByRole(
          'link',
          { name: 'Audit log' },
        ),
      ).not.toBeInTheDocument()

      expect(
        screen.queryByRole(
          'link',
          { name: 'Reports' },
        ),
      ).not.toBeInTheDocument()
    })
  },
)
