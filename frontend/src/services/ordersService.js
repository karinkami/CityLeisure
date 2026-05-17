import { api } from '@/services/api'

export async function getUserOrders() {
  const { data } = await api.get('/orders')
  return Array.isArray(data) ? data : []
}

export async function createOrder(payload) {
  const { data } = await api.post('/orders', payload)
  return data
}
