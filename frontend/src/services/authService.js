import { api } from '@/services/api'
import { clearSession as clearAuthSession, saveSession as persistSession } from './session'

export async function loginUser(credentials) {
  const response = await api.post('/auth/login', credentials)
  return response.data
}

export async function registerUser(payload) {
  const response = await api.post('/auth/register', payload)
  return response.data
}

export async function fetchCurrentUser() {
  const response = await api.get('/users/me')
  return response.data
}

export function clearSession() {
  clearAuthSession()
}

export function saveSession(token, user) {
  persistSession(token, user)
}
