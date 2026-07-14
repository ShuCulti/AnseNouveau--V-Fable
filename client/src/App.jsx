import { BrowserRouter, Routes, Route, NavLink, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './AuthContext'
import LoginPage from './pages/LoginPage'
import PosPage from './pages/PosPage'
import PricesPage from './pages/PricesPage'
import InventoryPage from './pages/InventoryPage'
import ReportsPage from './pages/ReportsPage'
import StockCountPage from './pages/StockCountPage'

function Layout({ children }) {
  const { user, logout } = useAuth()
  if (!user) {
    return <Navigate to="/login" replace />
  }
  const isAdmin = user.role === 'Admin'
  return (
    <div className="layout">
      <nav className="topnav">
        <NavLink to="/">Sell</NavLink>
        {isAdmin && <NavLink to="/prices">Prices</NavLink>}
        <NavLink to="/inventory">Stock</NavLink>
        {isAdmin && <NavLink to="/reports">Reports</NavLink>}
        <NavLink to="/count">Count</NavLink>
        <button className="nav-logout" onClick={logout}>
          {user.displayName} ⏻
        </button>
      </nav>
      <main className="page">{children}</main>
    </div>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<Layout><PosPage /></Layout>} />
          <Route path="/prices" element={<Layout><PricesPage /></Layout>} />
          <Route path="/inventory" element={<Layout><InventoryPage /></Layout>} />
          <Route path="/reports" element={<Layout><ReportsPage /></Layout>} />
          <Route path="/count" element={<Layout><StockCountPage /></Layout>} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
