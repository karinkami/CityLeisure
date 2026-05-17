import { api } from '@/services/api'

export async function getTopPopularEvents() {
  const { data } = await api.get('/events/top-popular')
  return Array.isArray(data) ? data : []
}

export async function getRecommendedEvents(limit = 6) {
  const { data } = await api.get('/events/recommended', { params: { limit } })
  return Array.isArray(data) ? data : []
}

export async function getEventCategories() {
  const { data } = await api.get('/event-categories')
  return Array.isArray(data) ? data : []
}

export async function getVenues() {
  const { data } = await api.get('/venues')
  return Array.isArray(data) ? data : []
}

export async function getEvents(params = {}) {
  const { data } = await api.get('/events', { params })
  return Array.isArray(data) ? data : []
}

export async function getEventById(eventId) {
  const { data } = await api.get(`/events/${eventId}`)
  return data
}

export async function getEventRecommendations(eventId) {
  const { data } = await api.get(`/events/${eventId}/recommendations`)
  return Array.isArray(data) ? data : []
}

export async function getEventSeatMap(eventId) {
  const { data } = await api.get(`/events/${eventId}/seat-map`)
  return data
}
