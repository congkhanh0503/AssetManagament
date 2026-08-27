import { apiClient } from './client'

const TOKEN_KEY = 'qlkho_auth_token'
const USER_KEY = 'qlkho_auth_user'

export const authApi = {
  login: async (credentials) => {
    const res = await apiClient.post('/auth/login', credentials)
    if (res && res.token) {
      setSession(res.token, {
        userID: res.userID,
        username: res.username,
        fullName: res.fullName,
        role: res.role,
        avatar: res.avatar,
        email: res.email
      })
    }
    return res
  },

  getMe: (username = 'admin') => apiClient.get(`/auth/me?username=${username}`),

  changePassword: (data, username = 'admin') => 
    apiClient.post(`/auth/change-password?username=${username}`, data),

  logout: () => {
    clearSession()
  }
}

export function setSession(token, user) {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USER_KEY, JSON.stringify(user))
}

export function getSession() {
  const token = localStorage.getItem(TOKEN_KEY)
  const userStr = localStorage.getItem(USER_KEY)
  let user = null
  try {
    user = userStr ? JSON.parse(userStr) : null
  } catch (e) {
    user = null
  }
  return { token, user }
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}

export function isAuthenticated() {
  const { token } = getSession()
  return !!token
}

export function getCurrentUser() {
  const { user } = getSession()
  return user || {
    username: 'admin',
    fullName: 'Quản Trị Viên (Admin)',
    role: 'Admin'
  }
}
