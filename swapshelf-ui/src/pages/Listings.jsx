import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { useAuth } from '../context/AuthContext'

const conditionMap = { 0: 'New', 1: 'Good', 2: 'Fair', 3: 'Poor' }
const statusMap = { 0: 'Available', 1: 'Locked', 2: 'Swapped' }
const statusClassMap = { 0: 'available', 1: 'locked', 2: 'swapped' }

export default function Listings() {
  const { user } = useAuth()
  const [listings, setListings] = useState([])
  const [loading, setLoading] = useState(true)
  const [filters, setFilters] = useState({
    genre: '', condition: '', location: '', author: ''
  })

  const fetchListings = async (activeFilters) => {
    setLoading(true)
    try {
      const f = activeFilters ?? filters
      const params = new URLSearchParams()
      if (f.genre) params.append('genre', f.genre)
      if (f.condition) params.append('condition', f.condition)
      if (f.location) params.append('location', f.location)
      if (f.author) params.append('author', f.author)
      const res = await api.get(`/listings?${params.toString()}`)
      setListings(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchListings() }, [])

  const handleConditionChange = (e) => {
    const newFilters = { ...filters, condition: e.target.value }
    setFilters(newFilters)
    fetchListings(newFilters)
  }

  const handleGenreChange = (e) => {
    const newFilters = { ...filters, genre: e.target.value }
    setFilters(newFilters)
    fetchListings(newFilters)
  }

  return (
    <div className="page">
      <h1 className="page-title">Browse Listings</h1>
      <p className="page-subtitle">Find books available for swapping near you</p>

      {!user && (
        <div style={{
          background: 'rgba(240,165,0,0.08)',
          border: '1px solid var(--amber-dim)',
          borderRadius: '10px',
          padding: '0.9rem 1.25rem',
          marginBottom: '1.5rem',
          fontSize: '0.9rem'
        }}>
          Want to swap books?{' '}
          <Link to="/login" style={{ color: 'var(--amber)', fontWeight: 600 }}>Sign in</Link>
          {' '}or{' '}
          <Link to="/register" style={{ color: 'var(--amber)', fontWeight: 600 }}>create an account</Link>
          {' '}to list books, add to your wanted list, and find matches.
        </div>
      )}

      <div className="filters">
        <input
          placeholder="Search by author..."
          value={filters.author}
          onChange={e => setFilters({ ...filters, author: e.target.value })}
        />
        <input
          placeholder="Location..."
          value={filters.location}
          onChange={e => setFilters({ ...filters, location: e.target.value })}
        />
        <select value={filters.genre} onChange={handleGenreChange}>
          <option value="">All Genres</option>
          <option value="Fiction">Fiction</option>
          <option value="Non-Fiction">Non-Fiction</option>
          <option value="Science">Science</option>
          <option value="History">History</option>
          <option value="Fantasy">Fantasy</option>
          <option value="Biography">Biography</option>
          <option value="Mystery">Mystery</option>
          <option value="Romance">Romance</option>
        </select>
        <select value={filters.condition} onChange={handleConditionChange}>
          <option value="">All Conditions</option>
          <option value="New">New</option>
          <option value="Good">Good</option>
          <option value="Fair">Fair</option>
          <option value="Poor">Poor</option>
        </select>
        <button className="btn btn-primary btn-sm" onClick={() => fetchListings()}>
          Search
        </button>
      </div>

      {loading ? (
        <p style={{ color: 'var(--text-muted)' }}>Loading listings...</p>
      ) : listings.length === 0 ? (
        <div className="empty">
          <h3>No listings found</h3>
          <p>Try adjusting your filters</p>
        </div>
      ) : (
        <div className="grid">
          {listings.map(listing => (
            <div key={listing.id} className="card">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                <span className={`badge badge-${statusClassMap[listing.status] || 'available'}`}>
                  {statusMap[listing.status] || listing.status}
                </span>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                  {conditionMap[listing.condition] || listing.condition}
                </span>
              </div>

              <h3 style={{ fontSize: '1.05rem', marginBottom: '0.25rem', lineHeight: 1.3 }}>
                {listing.book.title}
              </h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: '0.5rem' }}>
                by {listing.book.author}
              </p>

              <div style={{ marginBottom: '0.75rem' }}>
                <span style={{
                  background: 'var(--surface2)',
                  padding: '0.15rem 0.6rem',
                  borderRadius: '4px',
                  fontSize: '0.78rem',
                  color: 'var(--text-muted)'
                }}>
                  {listing.book.genre}
                </span>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                  {listing.location}
                </span>
                <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                  {listing.userFullName}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
