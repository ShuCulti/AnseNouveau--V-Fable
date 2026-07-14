import { createContext, useContext, useState } from 'react'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem('user')
    return stored ? JSON.parse(stored) : null
  })

  function login(loginResponse) {
    localStorage.setItem('token', loginResponse.token)
    const userInfo = {
      userId: loginResponse.userId,
      displayName: loginResponse.displayName,
      role: loginResponse.role,
      shopId: loginResponse.shopId,
    }
    localStorage.setItem('user', JSON.stringify(userInfo))
    setUser(userInfo)
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }

  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>
}

export function useAuth() {
  return useContext(AuthContext)
}
