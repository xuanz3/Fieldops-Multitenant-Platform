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
import userEvent from '@testing-library/user-event'
import {
  MemoryRouter,
} from 'react-router-dom'
import { AuthContext } from '../auth/AuthContext'
import { LoginPage } from './LoginPage'

describe('LoginPage', () => {
  it('submits the selected demo credentials', async () => {
    const signIn = vi.fn().mockResolvedValue(
      undefined,
    )

    render(
      <AuthContext.Provider
        value={{
          session: null,
          isAuthenticated: false,
          signIn,
          signOut: vi.fn(),
        }}
      >
        <MemoryRouter
          initialEntries={['/login']}
        >
          <LoginPage />
        </MemoryRouter>
      </AuthContext.Provider>,
    )

    const user = userEvent.setup()

    await user.click(
      screen.getByRole('button', {
        name: 'Sign in to workspace',
      }),
    )

    expect(signIn).toHaveBeenCalledWith(
      'northside-property-services',
      'dispatcher@northside.example.test',
      'FieldOps-Demo-2026!',
    )
  })
})
