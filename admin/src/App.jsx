import { BrowserRouter, Routes, Route, NavLink, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './AuthContext'
import LoginPage from './pages/LoginPage'
import ProductsPage from './pages/ProductsPage'
import GridPage from './pages/GridPage'
import MonthEndPage from './pages/MonthEndPage'

function Layout({ children }) {
  const { user, logout } = useAuth()
  if (!user) {
    return <Navigate to="/login" replace />
  }
  return (
    <div className="layout">
      <nav className="topnav">
        <span className="brand">Anse Nouveau — Back Office</span>
        <NavLink to="/">Products</NavLink>
        <NavLink to="/grid">Price Grid</NavLink>
        <NavLink to="/month-end">Month End</NavLink>
        <button className="nav-logout" onClick={logout}>
          {user.displayName} — log out
        </button>
      </nav>
      <main className="page">{children}</main>
    </div>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter basename={import.meta.env.BASE_URL}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<Layout><ProductsPage /></Layout>} />
          <Route path="/grid" element={<Layout><GridPage /></Layout>} />
          <Route path="/month-end" element={<Layout><MonthEndPage /></Layout>} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
