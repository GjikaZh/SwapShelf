import { useState, useEffect } from 'react'
import api from '../api/axios'

const conditionMap = { 0: 'New', 1: 'Good', 2: 'Fair', 3: 'Poor' }

export default function Matches() {
  const [matches, setMatches] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [swapping, setSwapping] = useState(null)
  const [selectedMyListing, setSelectedMyListing] = useState('')

  const fetchMatches = async () => {
    setLoading(true)
    try {
      const res = await api.get('/matches')
      setMatches(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchMatches() }, [])

  const handleSwap = async (e) => {
    e.preventDefault()
    setError('')
    try {
      await api.post('/swaps', {
        initiatorListingId: parseInt(selectedMyListing),
        receiverListingId: swapping.theirListing.id
      })
      setSuccess('Swap request sent successfully.')
      setSwapping(null)
      setSelectedMyListing('')
      fetchMatches()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to send swap request.')
    }
  }

  return (
    <div className="page">
      <h1 className="page-title">Your Matches</h1>
      <p className="page-subtitle">
        People who have a book you want and want a book you have
      </p>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {loading ? (
        <p style={{ color: 'var(--text-muted)' }}>Finding your matches...</p>
      ) : matches.length === 0 ? (
        <div className="empty">
          <h3>No matches yet</h3>
          <p>Add listings and wanted books to find mutual swap opportunities</p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          {matches.map((match, i) => (
            <div key={i} className="card">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '1rem' }}>

                {/* Their side */}
                <div style={{ flex: 1, minWidth: '200px' }}>
                  <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.5rem' }}>
                    They have
                  </p>
                  <h3 style={{ fontSize: '1.05rem', marginBottom: '0.2rem' }}>
                    {match.theirListing.book.title}
                  </h3>
                  <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: '0.5rem' }}>
                    by {match.theirListing.book.author}
                  </p>
                  <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                    <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                      📍 {match.theirListing.location}
                    </span>
                    <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                      · {conditionMap[match.theirListing.condition] || match.theirListing.condition}
                    </span>
                  </div>
                </div>

                {/* Arrow */}
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  color: 'var(--amber)',
                  fontSize: '1.5rem',
                  padding: '0 1rem'
                }}>
                  ⇄
                </div>

                {/* My side */}
                <div style={{ flex: 1, minWidth: '200px' }}>
                  <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.5rem' }}>
                    They want one of yours
                  </p>
                  {match.myMatchingListings.map(ml => (
                    <div key={ml.id} style={{ marginBottom: '0.4rem' }}>
                      <span style={{ fontSize: '0.9rem' }}>{ml.book.title}</span>
                      <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginLeft: '0.5rem' }}>
                        · {conditionMap[ml.condition] || ml.condition}
                      </span>
                    </div>
                  ))}
                </div>

                {/* User info + action */}
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: '0.75rem' }}>
                  <div style={{ textAlign: 'right' }}>
                    <p style={{ fontSize: '0.9rem', fontWeight: 500 }}>{match.theirUserName}</p>
                    <p style={{ fontSize: '0.82rem', color: 'var(--amber)' }}>
                      ★ {match.theirTrustScore.toFixed(1)}
                    </p>
                  </div>
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => { setSwapping(match); setSelectedMyListing('') }}
                  >
                    Request Swap
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Swap request modal */}
      {swapping && (
        <div className="modal-overlay" onClick={() => setSwapping(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>Send Swap Request</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.5rem' }}>
              You are requesting{' '}
              <strong style={{ color: 'var(--text)' }}>
                {swapping.theirListing.book.title}
              </strong>{' '}
              from {swapping.theirUserName}. Choose which of your books to offer in return.
            </p>
            {error && <div className="alert alert-error">{error}</div>}
            <form onSubmit={handleSwap}>
              <div className="form-group">
                <label>Your listing to offer</label>
                <select
                  value={selectedMyListing}
                  onChange={e => setSelectedMyListing(e.target.value)}
                  required
                >
                  <option value="">Select one of your listings...</option>
                  {swapping.myMatchingListings.map(ml => (
                    <option key={ml.id} value={ml.id}>
                      {ml.book.title} — {conditionMap[ml.condition] || ml.condition}
                    </option>
                  ))}
                </select>
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-outline" onClick={() => setSwapping(null)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Send Request
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}