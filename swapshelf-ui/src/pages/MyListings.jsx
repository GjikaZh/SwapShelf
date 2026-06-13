import { useState, useEffect } from 'react'
import api from '../api/axios'

const conditionMap = { 0: 'New', 1: 'Good', 2: 'Fair', 3: 'Poor' }
const statusMap = { 0: 'Available', 1: 'Locked', 2: 'Swapped' }
const statusClassMap = { 0: 'available', 1: 'locked', 2: 'swapped' }

export default function MyListings() {
  const [listings, setListings] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [editingListing, setEditingListing] = useState(null)
  const [showBookModal, setShowBookModal] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  // Book search state inside the listing form
  const [bookSearch, setBookSearch] = useState('')
  const [bookResults, setBookResults] = useState([])
  const [bookSearching, setBookSearching] = useState(false)
  const [selectedBook, setSelectedBook] = useState(null)

  const [listingForm, setListingForm] = useState({ condition: 1, location: '' })

  const [bookForm, setBookForm] = useState({ title: '', author: '', genre: '', isbn: '' })

  const fetchData = async () => {
    setLoading(true)
    try {
      const res = await api.get('/listings/mine')
      setListings(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchData() }, [])

  const handleBookSearch = async () => {
    if (!bookSearch.trim()) return
    setBookSearching(true)
    setBookResults([])
    setSelectedBook(null)
    try {
      const res = await api.get('/books')
      const q = bookSearch.toLowerCase()
      const filtered = res.data.filter(b =>
        b.title.toLowerCase().includes(q) ||
        b.author.toLowerCase().includes(q)
      )
      setBookResults(filtered)
    } catch (err) {
      console.error(err)
    } finally {
      setBookSearching(false)
    }
  }

  const handleCreateBook = async (e) => {
    e.preventDefault()
    setError('')
    try {
      const res = await api.post('/books', {
        title: bookForm.title,
        author: bookForm.author,
        genre: bookForm.genre,
        isbn: bookForm.isbn || null
      })
      setSelectedBook(res.data)
      setBookResults([])
      setBookSearch(`${res.data.title} — ${res.data.author}`)
      setShowBookModal(false)
      setBookForm({ title: '', author: '', genre: '', isbn: '' })
      setSuccess('Book added to catalog.')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create book.')
    }
  }

  const handleCreateListing = async (e) => {
    e.preventDefault()
    setError('')
    if (!selectedBook) {
      setError('Please select a book.')
      return
    }
    try {
      if (editingListing) {
        await api.put(`/listings/${editingListing.id}`, {
          bookId: selectedBook.id,
          condition: parseInt(listingForm.condition),
          location: listingForm.location
        })
        setSuccess('Listing updated.')
      } else {
        await api.post('/listings', {
          bookId: selectedBook.id,
          condition: parseInt(listingForm.condition),
          location: listingForm.location
        })
        setSuccess('Listing created.')
      }
      closeModal()
      fetchData()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save listing.')
    }
  }

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this listing?')) return
    try {
      await api.delete(`/listings/${id}`)
      setSuccess('Listing deleted.')
      fetchData()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete.')
    }
  }

  const openEdit = (listing) => {
    setEditingListing(listing)
    setSelectedBook(listing.book)
    setBookSearch(`${listing.book.title} — ${listing.book.author}`)
    setBookResults([])
    setListingForm({ condition: listing.condition, location: listing.location })
    setError('')
    setShowModal(true)
  }

  const closeModal = () => {
    setShowModal(false)
    setEditingListing(null)
    setSelectedBook(null)
    setBookSearch('')
    setBookResults([])
    setListingForm({ condition: 1, location: '' })
    setError('')
  }

  return (
    <div className="page">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '2rem' }}>
        <div>
          <h1 className="page-title">My Listings</h1>
          <p className="page-subtitle">Books you are offering for swap</p>
        </div>
        <button className="btn btn-primary" onClick={() => {
          setEditingListing(null)
          setSelectedBook(null)
          setBookSearch('')
          setBookResults([])
          setListingForm({ condition: 1, location: '' })
          setError('')
          setShowModal(true)
        }}>
          + New Listing
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {loading ? (
        <p style={{ color: 'var(--text-muted)' }}>Loading...</p>
      ) : listings.length === 0 ? (
        <div className="empty">
          <h3>No listings yet</h3>
          <p>Add your first book to start swapping</p>
        </div>
      ) : (
        <div className="grid">
          {listings.map(listing => (
            <div key={listing.id} className="card">
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.75rem' }}>
                <span className={`badge badge-${statusClassMap[listing.status] || 'available'}`}>
                  {statusMap[listing.status] || listing.status}
                </span>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                  {conditionMap[listing.condition] || listing.condition}
                </span>
              </div>
              <h3 style={{ fontSize: '1.05rem', marginBottom: '0.25rem' }}>
                {listing.book.title}
              </h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: '0.75rem' }}>
                by {listing.book.author}
              </p>
              <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '1rem' }}>
                {listing.location}
              </p>
              {listing.status === 2 ? (
                <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontStyle: 'italic' }}>
                  This book has been swapped and is now part of your history.
                </p>
              ) : (
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button
                    className="btn btn-outline btn-sm"
                    onClick={() => openEdit(listing)}
                    disabled={listing.status === 1}
                  >
                    Edit
                  </button>
                  <button
                    className="btn btn-danger btn-sm"
                    onClick={() => handleDelete(listing.id)}
                    disabled={listing.status === 1}
                  >
                    Delete
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Create/Edit Listing Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>{editingListing ? 'Edit Listing' : 'New Listing'}</h2>
            {error && <div className="alert alert-error">{error}</div>}
            <form onSubmit={handleCreateListing}>

              {/* Book search */}
              <div className="form-group">
                <label>Book</label>
                <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.5rem' }}>
                  <input
                    style={{ flex: 1 }}
                    placeholder="Search by title or author..."
                    value={bookSearch}
                    onChange={e => { setBookSearch(e.target.value); setSelectedBook(null) }}
                    onKeyDown={e => e.key === 'Enter' && (e.preventDefault(), handleBookSearch())}
                  />
                  <button type="button" className="btn btn-outline btn-sm" onClick={handleBookSearch} disabled={bookSearching}>
                    {bookSearching ? '...' : 'Search'}
                  </button>
                  <button type="button" className="btn btn-outline btn-sm" onClick={() => setShowBookModal(true)}>
                    + New
                  </button>
                </div>

                {bookResults.length > 0 && !selectedBook && (
                  <div style={{
                    border: '1px solid var(--border)',
                    borderRadius: '8px',
                    overflow: 'hidden',
                    maxHeight: '180px',
                    overflowY: 'auto'
                  }}>
                    {bookResults.map(book => (
                      <div
                        key={book.id}
                        onClick={() => { setSelectedBook(book); setBookSearch(`${book.title} — ${book.author}`); setBookResults([]) }}
                        style={{
                          padding: '0.6rem 1rem',
                          cursor: 'pointer',
                          background: 'var(--surface2)',
                          borderBottom: '1px solid var(--border)',
                          fontSize: '0.9rem'
                        }}
                      >
                        <span style={{ fontWeight: 500 }}>{book.title}</span>
                        <span style={{ color: 'var(--text-muted)', marginLeft: '0.4rem' }}>by {book.author}</span>
                      </div>
                    ))}
                  </div>
                )}

                {selectedBook && (
                  <div style={{
                    background: 'rgba(240,165,0,0.08)',
                    border: '1px solid var(--amber-dim)',
                    borderRadius: '8px',
                    padding: '0.6rem 1rem',
                    fontSize: '0.88rem'
                  }}>
                    <span style={{ color: 'var(--amber)', fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Selected · </span>
                    <strong>{selectedBook.title}</strong>
                    <span style={{ color: 'var(--text-muted)' }}> by {selectedBook.author}</span>
                  </div>
                )}
              </div>

              <div className="form-group">
                <label>Condition</label>
                <select
                  value={listingForm.condition}
                  onChange={e => setListingForm({ ...listingForm, condition: parseInt(e.target.value) })}
                >
                  <option value={0}>New</option>
                  <option value={1}>Good</option>
                  <option value={2}>Fair</option>
                  <option value={3}>Poor</option>
                </select>
              </div>

              <div className="form-group">
                <label>Location</label>
                <input
                  placeholder="e.g. Skopje, MK"
                  value={listingForm.location}
                  onChange={e => setListingForm({ ...listingForm, location: e.target.value })}
                  required
                />
              </div>

              <div className="modal-actions">
                <button type="button" className="btn btn-outline" onClick={closeModal}>Cancel</button>
                <button type="submit" className="btn btn-primary">
                  {editingListing ? 'Save Changes' : 'Create Listing'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add New Book to Catalog Modal */}
      {showBookModal && (
        <div className="modal-overlay" onClick={() => setShowBookModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>Add Book to Catalog</h2>
            {error && <div className="alert alert-error">{error}</div>}
            <form onSubmit={handleCreateBook}>
              <div className="form-group">
                <label>Title</label>
                <input
                  placeholder="Book title"
                  value={bookForm.title}
                  onChange={e => setBookForm({ ...bookForm, title: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Author</label>
                <input
                  placeholder="Author name"
                  value={bookForm.author}
                  onChange={e => setBookForm({ ...bookForm, author: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Genre</label>
                <select
                  value={bookForm.genre}
                  onChange={e => setBookForm({ ...bookForm, genre: e.target.value })}
                  required
                >
                  <option value="">Select genre...</option>
                  <option value="Fiction">Fiction</option>
                  <option value="Non-Fiction">Non-Fiction</option>
                  <option value="Science">Science</option>
                  <option value="History">History</option>
                  <option value="Fantasy">Fantasy</option>
                  <option value="Biography">Biography</option>
                  <option value="Mystery">Mystery</option>
                  <option value="Romance">Romance</option>
                </select>
              </div>
              <div className="form-group">
                <label>ISBN (optional)</label>
                <input
                  placeholder="978-..."
                  value={bookForm.isbn}
                  onChange={e => setBookForm({ ...bookForm, isbn: e.target.value })}
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-outline" onClick={() => setShowBookModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">Add Book</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
