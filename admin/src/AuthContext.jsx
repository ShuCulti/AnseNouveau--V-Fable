import { createContext, useContext, useState } from 'react'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem('adminUser')
    return stored ? JSON.parse(stored) : null
  })

  function login(loginResponse) {
    localStorage.setItem('adminToken', loginResponse.token)
    const userInfo = {
      userId: loginResponse.userId,
      displayName: loginResponse.displayName,
      role: loginResponse.role,
      shopId: loginResponse.shopId,
    }
    localStorage.setItem('adminUser', JSON.stringify(userInfo))
    setUser(userInfo)
  }

  function logout() {
    localStorage.removeItem('adminToken')
    localStorage.removeItem('adminUser')
    setUser(null)
  }

  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>
}

export function useAuth() {
  return useContext(AuthContext)
}
