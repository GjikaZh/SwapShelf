import { createContext, useContext, useState, useEffect } from 'react'

const AuthContext = createContext()

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)

  useEffect(() => {
    const token = localStorage.getItem('token')
    const fullName = localStorage.getItem('fullName')
    const role = localStorage.getItem('role')
    if (token) setUser({ token, fullName, role })
  }, [])

  const login = (data) => {
    localStorage.setItem('token', data.token)
    localStorage.setItem('fullName', data.fullName)
    localStorage.setItem('role', data.role)
    setUser(data)
  }

  const logout = () => {
    localStorage.clear()
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext)