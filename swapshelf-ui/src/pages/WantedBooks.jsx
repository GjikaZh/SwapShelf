import { useState, useEffect } from 'react'
import api from '../api/axios'

export default function WantedBooks() {
  const [wanted, setWanted] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [searchResults, setSearchResults] = useState([])
  const [searching, setSearching] = useState(false)
  const [selectedBook, setSelectedBook] = useState(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const fetchWanted = async () => {
    setLoading(true)
    try {
      const res = await api.get('/wanted')
      setWanted(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchWanted() }, [])

  const handleSearch = async () => {
    if (!searchQuery.trim()) return
    setSearching(true)
    setSearchResults([])
    setSelectedBook(null)
    try {
      const res = await api.get(`/books`)
      const q = searchQuery.toLowerCase()
      const filtered = res.data.filter(b =>
        b.title.toLowerCase().includes(q) ||
        b.author.toLowerCase().includes(q)
      )
      setSearchResults(filtered)
    } catch (err) {
      console.error(err)
    } finally {
      setSearching(false)
    }
  }

  const handleAdd = async () => {
    if (!selectedBook) return
    setError('')
    try {
      await api.post('/wanted', { bookId: selectedBook.id })
      setSuccess(`"${selectedBook.title}" added to your wanted list.`)
      setShowModal(false)
      setSearchQuery('')
      setSearchResults([])
      setSelectedBook(null)
      fetchWanted()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add book.')
    }
  }

  const handleRemove = async (id) => {
    if (!window.confirm('Remove this book from your wanted list?')) return
    try {
      await api.delete(`/wanted/${id}`)
      setSuccess('Book removed from wanted list.')
      fetchWanted()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove.')
    }
  }

  return (
    <div className="page">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '2rem' }}>
        <div>
          <h1 className="page-title">Wanted Books</h1>
          <p className="page-subtitle">Books you are looking to receive in a swap</p>
        </div>
        <button className="btn btn-primary" onClick={() => {
          setError('')
          setSearchQuery('')
          setSearchResults([])
          setSelectedBook(null)
          setShowModal(true)
        }}>
          + Add Wanted Book
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {loading ? (
        <p style={{ color: 'var(--text-muted)' }}>Loading...</p>
      ) : wanted.length === 0 ? (
        <div className="empty">
          <h3>No wanted books yet</h3>
          <p>Search for books you want and the system will find matches for you</p>
        </div>
      ) : (
        <div className="grid">
          {wanted.map(w => (
            <div key={w.id} className="card">
              <h3 style={{ fontSize: '1.05rem', marginBottom: '0.25rem' }}>
                {w.book.title}
              </h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: '0.5rem' }}>
                by {w.book.author}
              </p>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '1rem' }}>
                {w.book.genre}
              </p>
              <button className="btn btn-danger btn-sm" onClick={() => handleRemove(w.id)}>
                Remove
              </button>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>Add to Wanted List</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.25rem' }}>
              Search by title or author to find the book you want.
            </p>

            {error && <div className="alert alert-error">{error}</div>}

            {/* Search input */}
            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
              <input
                style={{
                  flex: 1,
                  background: 'var(--surface2)',
                  border: '1px solid var(--border)',
                  borderRadius: '8px',
                  padding: '0.7rem 1rem',
                  color: 'var(--text)',
                  fontSize: '0.95rem',
                  outline: 'none'
                }}
                placeholder="Search by title or author..."
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && handleSearch()}
              />
              <button className="btn btn-outline" onClick={handleSearch} disabled={searching}>
                {searching ? '...' : 'Search'}
              </button>
            </div>

            {/* Search results */}
            {searchResults.length > 0 && (
              <div style={{
                border: '1px solid var(--border)',
                borderRadius: '8px',
                overflow: 'hidden',
                marginBottom: '1rem',
                maxHeight: '220px',
                overflowY: 'auto'
              }}>
                {searchResults.map(book => (
                  <div
                    key={book.id}
                    onClick={() => setSelectedBook(book)}
                    style={{
                      padding: '0.75rem 1rem',
                      cursor: 'pointer',
                      background: selectedBook?.id === book.id ? 'rgba(240,165,0,0.1)' : 'var(--surface2)',
                      borderBottom: '1px solid var(--border)',
                      borderLeft: selectedBook?.id === book.id ? '3px solid var(--amber)' : '3px solid transparent',
                      transition: 'all 0.15s'
                    }}
                  >
                    <p style={{ fontWeight: 500, fontSize: '0.95rem' }}>{book.title}</p>
                    <p style={{ color: 'var(--text-muted)', fontSize: '0.82rem' }}>
                      by {book.author} · {book.genre}
                    </p>
                  </div>
                ))}
              </div>
            )}

            {searchResults.length === 0 && searchQuery && !searching && (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1rem' }}>
                No books found. The book may not be in the catalog yet.
              </p>
            )}

            {selectedBook && (
              <div style={{
                background: 'rgba(240,165,0,0.08)',
                border: '1px solid var(--amber-dim)',
                borderRadius: '8px',
                padding: '0.75rem 1rem',
                marginBottom: '1rem'
              }}>
                <p style={{ fontSize: '0.78rem', color: 'var(--amber)', marginBottom: '0.2rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  Selected
                </p>
                <p style={{ fontWeight: 500 }}>{selectedBook.title}</p>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>by {selectedBook.author}</p>
              </div>
            )}

            <div className="modal-actions">
              <button type="button" className="btn btn-outline" onClick={() => setShowModal(false)}>
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={handleAdd}
                disabled={!selectedBook}
              >
                Add to Wanted
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}