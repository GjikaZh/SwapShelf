import { useState, useEffect } from 'react'
import api from '../api/axios'

export default function Admin() {
  const [users, setUsers] = useState([])
  const [listings, setListings] = useState([])
  const [activeTab, setActiveTab] = useState('users')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const fetchData = async () => {
    setLoading(true)
    try {
      const [usersRes, listingsRes] = await Promise.all([
        api.get('/admin/users'),
        api.get('/admin/listings')
      ])
      setUsers(usersRes.data)
      setListings(listingsRes.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchData() }, [])

  const handleBan = async (id, isBanned) => {
    const action = isBanned ? 'unban' : 'ban'
    if (!window.confirm(`${isBanned ? 'Unban' : 'Ban'} this user?`)) return
    try {
      await api.put(`/admin/users/${id}/${action}`)
      setSuccess(`User ${action}ned successfully.`)
      fetchData()
    } catch (err) {
      setError(err.response?.data?.message || `Failed to ${action} user.`)
    }
  }

  const handleDeleteListing = async (id) => {
    if (!window.confirm('Delete this listing?')) return
    try {
      await api.delete(`/admin/listings/${id}`)
      setSuccess('Listing deleted.')
      fetchData()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete listing.')
    }
  }

  const tabStyle = (tab) => ({
    padding: '0.55rem 1.25rem',
    borderRadius: '8px',
    border: 'none',
    fontSize: '0.9rem',
    fontWeight: 500,
    cursor: 'pointer',
    background: activeTab === tab ? 'var(--amber)' : 'transparent',
    color: activeTab === tab ? '#0f0f13' : 'var(--text-muted)',
    transition: 'all 0.2s'
  })

  return (
    <div className="page">
      <h1 className="page-title">Admin Panel</h1>
      <p className="page-subtitle">Manage users and listings across the platform</p>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
        <div className="card" style={{ textAlign: 'center' }}>
          <p style={{ fontSize: '2rem', fontFamily: 'Playfair Display, serif', color: 'var(--amber)' }}>
            {users.length}
          </p>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Total Users</p>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <p style={{ fontSize: '2rem', fontFamily: 'Playfair Display, serif', color: 'var(--amber)' }}>
            {users.filter(u => u.isBanned).length}
          </p>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Banned Users</p>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <p style={{ fontSize: '2rem', fontFamily: 'Playfair Display, serif', color: 'var(--amber)' }}>
            {listings.length}
          </p>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Total Listings</p>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <p style={{ fontSize: '2rem', fontFamily: 'Playfair Display, serif', color: 'var(--amber)' }}>
            {listings.filter(l => l.status === 0 || l.status === 'Available').length}
          </p>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Available</p>
        </div>
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem' }}>
        <button style={tabStyle('users')} onClick={() => setActiveTab('users')}>
          Users ({users.length})
        </button>
        <button style={tabStyle('listings')} onClick={() => setActiveTab('listings')}>
          Listings ({listings.length})
        </button>
      </div>

      {loading ? (
        <p style={{ color: 'var(--text-muted)' }}>Loading...</p>
      ) : activeTab === 'users' ? (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {users.map(user => (
            <div key={user.id} className="card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.25rem' }}>
                  <p style={{ fontWeight: 500 }}>{user.fullName}</p>
                  <span style={{
                    fontSize: '0.75rem',
                    padding: '0.15rem 0.6rem',
                    borderRadius: '999px',
                    background: user.role === 1 ? 'rgba(240,165,0,0.15)' : 'var(--surface2)',
                    color: user.role === 1 ? 'var(--amber)' : 'var(--text-muted)'
                  }}>
                    {user.role === 1 ? 'Admin' : 'User'}
                  </span>
                  {user.isBanned && (
                    <span className="badge" style={{ background: '#2e1a1a', color: 'var(--danger)' }}>
                      Banned
                    </span>
                  )}
                </div>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{user.email}</p>
                <p style={{ fontSize: '0.82rem', color: 'var(--amber)' }}>
                  ★ {user.trustScore.toFixed(1)} trust score
                </p>
              </div>
              {user.role !== 1 && (
                <button
                  className={`btn btn-sm ${user.isBanned ? 'btn-success' : 'btn-danger'}`}
                  onClick={() => handleBan(user.id, user.isBanned)}
                >
                  {user.isBanned ? 'Unban' : 'Ban'}
                </button>
              )}
            </div>
          ))}
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {listings.map(listing => (
            <div key={listing.id} className="card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.25rem' }}>
                  <p style={{ fontWeight: 500 }}>{listing.book.title}</p>
                  <span className={`badge badge-${typeof listing.status === 'string' ? listing.status.toLowerCase() : 'available'}`}>
                    {typeof listing.status === 'number'
                      ? ['Available', 'Locked', 'Swapped'][listing.status]
                      : listing.status}
                  </span>
                </div>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                  by {listing.book.author} · {listing.book.genre}
                </p>
                <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                  Listed by {listing.userFullName} · 📍 {listing.location}
                </p>
              </div>
              <button
                className="btn btn-danger btn-sm"
                onClick={() => handleDeleteListing(listing.id)}
                disabled={listing.status === 'Locked' || listing.status === 1}
              >
                Delete
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}