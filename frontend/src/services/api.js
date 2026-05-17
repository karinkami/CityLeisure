import axios from 'axios'
import { clearSession, getToken } from './session'

export const api = axios.create({
  baseURL: 'http://localhost:5001/api',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 10000
})

api.interceptors.request.use(
  config => {
    const token = getToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  error => Promise.reject(error)
)

api.interceptors.response.use(
  response => response,
  error => {
    console.error('API Error:', error)
    if (error.code === 'ECONNREFUSED') {
      console.error('Не удается подключиться к backend. Убедитесь, что сервер запущен на http://localhost:5001')
    }
    if (error.response?.status === 401) {
      clearSession()
      window.dispatchEvent(new Event('auth-changed'))
      window.location.href = '/auth'
    }
    return Promise.reject(error)
  }
)


