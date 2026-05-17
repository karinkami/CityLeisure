import { defineStore } from 'pinia'
import * as session from '@/services/session'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: session.getToken(),
    user: session.getStoredUser()
  }),
  getters: {
    isAuthenticated: state => !!state.token,
    isAdmin: () => session.getIsAdmin()
  },
  actions: {
    hydrate() {
      this.token = session.getToken()
      this.user = session.getStoredUser()
    },
    setSession(token, user) {
      session.saveSession(token, user)
      this.hydrate()
      window.dispatchEvent(new Event('auth-changed'))
    },
    setUser(user) {
      session.setStoredUser(user)
      this.user = user
    },
    clear() {
      session.clearSession()
      this.token = null
      this.user = null
      window.dispatchEvent(new Event('auth-changed'))
    }
  }
})
