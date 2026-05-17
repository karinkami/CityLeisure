import { api } from '@/services/api'

export async function getFavoriteEventIds() {
  const { data } = await api.get('/favorite-events')
  return Array.isArray(data) ? data : []
}

export async function getFavoriteEventDetails() {
  const { data } = await api.get('/favorite-events/details')
  return Array.isArray(data) ? data : []
}

export async function addFavoriteEvent(eventId) {
  await api.post('/favorite-events', { eventId })
}

export async function removeFavoriteEvent(eventId) {
  await api.delete(`/favorite-events/${eventId}`)
}
