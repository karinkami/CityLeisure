import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/stores/auth'

export function useAuth() {
  const store = useAuthStore()
  const { token, user, isAuthenticated, isAdmin } = storeToRefs(store)

  return {
    token,
    user,
    isAuthenticated,
    isAdmin,
    hydrate: () => store.hydrate(),
    setSession: (t, u) => store.setSession(t, u),
    setUser: u => store.setUser(u),
    clear: () => store.clear()
  }
}
