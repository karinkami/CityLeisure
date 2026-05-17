import { api } from '@/services/api'

export async function getRecommendationCategories() {
  const { data } = await api.get('/event-categories')
  return Array.isArray(data) ? data : []
}

export async function runWizardRecommendations(payload, timeout = 120000) {
  const { data } = await api.post('/events/wizard-rank', payload, { timeout })
  return data
}
