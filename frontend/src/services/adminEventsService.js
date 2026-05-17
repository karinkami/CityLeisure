import { api } from '@/services/api'

export async function getAdminCategories() {
  const { data } = await api.get('/event-categories')
  return Array.isArray(data) ? data : []
}

export async function getAdminVenues() {
  const { data } = await api.get('/venues')
  return Array.isArray(data) ? data : []
}

export async function getAdminEvents() {
  const { data } = await api.get('/events')
  return Array.isArray(data) ? data : []
}

export async function createAdminEvent(payload) {
  await api.post('/events', payload)
}

export async function updateAdminEvent(eventId, payload) {
  await api.put(`/events/${eventId}`, payload)
}

export async function deleteAdminEvent(eventId) {
  await api.delete(`/events/${eventId}`)
}

export async function updateAdminVenue(venueId, payload) {
  await api.put(`/venues/${venueId}`, payload)
}
