import type {
  LoginResponse,
  Session,
} from '../types'

const storageKey = 'fieldops.session'

export function toSession(
  response: LoginResponse,
): Session {
  return {
    accessToken: response.accessToken,
    expiresAt: response.expiresAt,
    user: response.user,
  }
}

export function isExpired(
  session: Session,
  now = Date.now(),
): boolean {
  const expiresAt =
    Date.parse(session.expiresAt)

  return (
    Number.isNaN(expiresAt) ||
    expiresAt <= now
  )
}

export function readSession(
  storage: Storage = sessionStorage,
): Session | null {
  const value = storage.getItem(storageKey)

  if (!value) {
    return null
  }

  try {
    const session =
      JSON.parse(value) as Session

    if (
      !session.accessToken ||
      !session.user ||
      isExpired(session)
    ) {
      storage.removeItem(storageKey)
      return null
    }

    return session
  } catch {
    storage.removeItem(storageKey)
    return null
  }
}

export function saveSession(
  session: Session,
  storage: Storage = sessionStorage,
): void {
  storage.setItem(
    storageKey,
    JSON.stringify(session),
  )
}

export function clearSession(
  storage: Storage = sessionStorage,
): void {
  storage.removeItem(storageKey)
}
