import { api } from '@/services/api'

export async function getCurrentUserProfile() {
  const { data } = await api.get('/users/me')
  return data
}

export async function updateCurrentUserProfile(payload) {
  const { data } = await api.put('/users/me', payload)
  return data
}
