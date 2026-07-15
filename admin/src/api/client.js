import axios from 'axios'

// In production the admin app is served by the API itself, so relative /api works;
// in dev it runs on 5174 and talks to the API on 5000.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || (import.meta.env.DEV ? 'http://localhost:5000/api' : '/api'),
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('adminToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401 && !error.config.url.includes('/auth/login')) {
      localStorage.removeItem('adminToken')
      localStorage.removeItem('adminUser')
      window.location.href = import.meta.env.BASE_URL + 'login'
    }
    return Promise.reject(error)
  }
)

export default api
