const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export function getToken() {
  return localStorage.getItem('token')
}

export function isAuthenticated() {
  return !!getToken()
}

export function getStoredUser() {
  try {
    return JSON.parse(localStorage.getItem('user') || 'null')
  } catch {
    return null
  }
}

export function clearSession() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
}

export function saveSession(token, user) {
  if (token) {
    localStorage.setItem('token', token)
  }
  localStorage.setItem('user', JSON.stringify(user))
}

export function setStoredUser(user) {
  localStorage.setItem('user', JSON.stringify(user))
}

export function getIsAdminFromToken(token) {
  if (!token) return false

  try {
    const parts = token.split('.')
    if (parts.length < 2) return false

    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4)
    const payload = JSON.parse(atob(padded))
    const roleValue = payload?.role || payload?.[ROLE_CLAIM]

    return Array.isArray(roleValue)
      ? String(roleValue[0] || '').toLowerCase() === 'admin'
      : String(roleValue || '').toLowerCase() === 'admin'
  } catch {
    return false
  }
}

export function getIsAdmin() {
  const user = getStoredUser()
  if (String(user?.role || '').toLowerCase() === 'admin') {
    return true
  }
  return getIsAdminFromToken(getToken())
}
