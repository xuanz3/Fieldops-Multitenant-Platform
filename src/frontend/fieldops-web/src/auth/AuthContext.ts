import { createContext } from 'react'
import type { Session } from '../types'

export interface AuthContextValue {
  session: Session | null
  isAuthenticated: boolean
  signIn: (
    tenantSlug: string,
    email: string,
    password: string,
  ) => Promise<void>
  signOut: () => void
}

export const AuthContext =
  createContext<AuthContextValue | null>(null)
