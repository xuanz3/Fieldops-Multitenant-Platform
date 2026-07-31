import {
  useCallback,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { login } from '../api/fieldOpsApi'
import type { Session } from '../types'
import {
  clearSession,
  readSession,
  saveSession,
  toSession,
} from './session'
import {
  AuthContext,
  type AuthContextValue,
} from './AuthContext'

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({
  children,
}: AuthProviderProps) {
  const [session, setSession] =
    useState<Session | null>(
      () => readSession(),
    )

  const signIn = useCallback(
    async (
      tenantSlug: string,
      email: string,
      password: string,
    ) => {
      const response = await login(
        tenantSlug,
        email,
        password,
      )

      const nextSession =
        toSession(response)

      saveSession(nextSession)
      setSession(nextSession)
    },
    [],
  )

  const signOut = useCallback(() => {
    clearSession()
    setSession(null)
  }, [])

  const value =
    useMemo<AuthContextValue>(
      () => ({
        session,
        isAuthenticated:
          session !== null,
        signIn,
        signOut,
      }),
      [
        session,
        signIn,
        signOut,
      ],
    )

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}
