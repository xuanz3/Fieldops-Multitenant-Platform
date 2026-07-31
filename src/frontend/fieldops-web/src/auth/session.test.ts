import {
  beforeEach,
  describe,
  expect,
  it,
} from 'vitest'
import {
  clearSession,
  isExpired,
  readSession,
  saveSession,
} from './session'
import type { Session } from '../types'

function makeSession(
  expiresAt: string,
): Session {
  return {
    accessToken: 'token',
    expiresAt,
    user: {
      id: 'user-1',
      tenantId: 'tenant-1',
      tenantSlug: 'northside',
      tenantName: 'Northside',
      email: 'dispatcher@example.test',
      displayName: 'Dispatcher',
      role: 'Dispatcher',
    },
  }
}

describe('session storage', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  it('round-trips a current session', () => {
    const value = makeSession(
      new Date(
        Date.now() + 60_000,
      ).toISOString(),
    )

    saveSession(value)

    expect(readSession()).toEqual(value)
  })

  it('removes an expired session', () => {
    const value = makeSession(
      new Date(
        Date.now() - 60_000,
      ).toISOString(),
    )

    saveSession(value)

    expect(readSession()).toBeNull()
    expect(sessionStorage.length).toBe(0)
  })

  it('detects expiry against a supplied time', () => {
    const value = makeSession(
      '2026-07-31T00:00:00.000Z',
    )

    expect(
      isExpired(
        value,
        Date.parse(
          '2026-07-31T00:00:01.000Z',
        ),
      ),
    ).toBe(true)
  })

  it('clears the session explicitly', () => {
    saveSession(
      makeSession(
        new Date(
          Date.now() + 60_000,
        ).toISOString(),
      ),
    )

    clearSession()

    expect(readSession()).toBeNull()
  })
})
