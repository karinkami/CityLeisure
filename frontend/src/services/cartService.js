import { api } from '@/services/api'

export async function getCartItems() {
  const { data } = await api.get('/cart-items')
  return Array.isArray(data) ? data : []
}

export async function addCartItem(payload) {
  const { data } = await api.post('/cart-items', payload)
  return data
}

export async function updateCartItem(itemId, payload) {
  await api.put(`/cart-items/${itemId}`, payload)
}

export async function removeCartItem(itemId) {
  await api.delete(`/cart-items/${itemId}`)
}

export async function clearCartItems() {
  await api.delete('/cart-items')
}
