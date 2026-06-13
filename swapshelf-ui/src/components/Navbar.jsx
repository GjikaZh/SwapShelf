import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Navbar() {
  const { user, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  const authLinks = [
    { to: '/my-listings', label: 'My Listings' },
    { to: '/wanted', label: 'Wanted' },
    { to: '/matches', label: 'Matches' },
    { to: '/swaps', label: 'Swaps' },
    ...(user?.role === 'Admin' ? [{ to: '/admin', label: 'Admin' }] : [])
  ]

  return (
    <nav style={{
      background: 'var(--surface)',
      borderBottom: '1px solid var(--border)',
      padding: '0 1.5rem',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      height: '60px',
      position: 'sticky',
      top: 0,
      zIndex: 100
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '2rem' }}>
        <Link to="/" style={{
          fontFamily: 'Playfair Display, serif',
          fontSize: '1.3rem',
          color: 'var(--amber)',
          fontWeight: 700,
          textDecoration: 'none'
        }}>
          SwapShelf
        </Link>

        <div style={{ display: 'flex', gap: '0.25rem' }}>
          <Link to="/" style={{
            padding: '0.4rem 0.9rem',
            borderRadius: '6px',
            fontSize: '0.9rem',
            color: location.pathname === '/' ? 'var(--amber)' : 'var(--text-muted)',
            background: location.pathname === '/' ? 'rgba(240,165,0,0.1)' : 'transparent',
            textDecoration: 'none',
            transition: 'all 0.2s',
            fontWeight: location.pathname === '/' ? 500 : 400
          }}>
            Browse
          </Link>

          {user && authLinks.map(link => (
            <Link
              key={link.to}
              to={link.to}
              style={{
                padding: '0.4rem 0.9rem',
                borderRadius: '6px',
                fontSize: '0.9rem',
                color: location.pathname === link.to ? 'var(--amber)' : 'var(--text-muted)',
                background: location.pathname === link.to ? 'rgba(240,165,0,0.1)' : 'transparent',
                textDecoration: 'none',
                transition: 'all 0.2s',
                fontWeight: location.pathname === link.to ? 500 : 400
              }}
            >
              {link.label}
            </Link>
          ))}
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
        {user ? (
          <>
            <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)' }}>
              {user.fullName}
            </span>
            <button className="btn btn-outline btn-sm" onClick={handleLogout}>
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" className="btn btn-outline btn-sm">Sign In</Link>
            <Link to="/register" className="btn btn-primary btn-sm">Register</Link>
          </>
        )}
      </div>
    </nav>
  )
}