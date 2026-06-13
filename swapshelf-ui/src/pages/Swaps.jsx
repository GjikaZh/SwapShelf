import { useState, useEffect } from 'react'
import api from '../api/axios'

const getCurrentUserId = () => {
  const token = localStorage.getItem('token')
  if (!token) return null
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const key = Object.keys(payload).find(k => k.includes('nameidentifier'))
    return key ? parseInt(payload[key]) : null
  } catch {
    return null
  }
}

const statusMap = {
  0: 'Pending', 1: 'Accepted', 2: 'Rejected',
  3: 'InTransit', 4: 'Completed', 5: 'Cancelled'
}

const statusClassMap = {
  0: 'pending', 1: 'accepted', 2: 'rejected',
  3: 'intransit', 4: 'completed', 5: 'cancelled'
}

const conditionMap = { 0: 'New', 1: 'Good', 2: 'Fair', 3: 'Poor' }

export default function Swaps() {
  const [swaps, setSwaps] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [showReviewModal, setShowReviewModal] = useState(false)
  const [reviewingSwap, setReviewingSwap] = useState(null)
  const [reviewForm, setReviewForm] = useState({ rating: 5, comment: '' })

  const userId = getCurrentUserId()

  const fetchSwaps = async () => {
    setLoading(true)
    try {
      const res = await api.get('/swaps')
      setSwaps(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchSwaps() }, [])

  const handleAction = async (swapId, action) => {
    setError('')
    setSuccess('')
    try {
      await api.put(`/swaps/${swapId}/${action}`)
      setSuccess(`Swap ${action} successful.`)
      fetchSwaps()
    } catch (err) {
      setError(err.response?.data?.message || `Failed to ${action} swap.`)
    }
  }

  const handleReview = async (e) => {
    e.preventDefault()
    setError('')
    try {
      const revieweeId = reviewingSwap.initiatorId === userId
        ? reviewingSwap.receiverId
        : reviewingSwap.initiatorId

      await api.post('/reviews', {
        swapRequestId: reviewingSwap.id,
        revieweeId,
        rating: parseInt(reviewForm.rating),
        comment: reviewForm.comment
      })
      setSuccess('Review submitted successfully.')
      setShowReviewModal(false)
      setReviewingSwap(null)
      setReviewForm({ rating: 5, comment: '' })
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to submit review.')
    }
  }

  const getStatusLabel = (status) => statusMap[status] ?? status
  const getStatusClass = (status) => statusClassMap[status] ?? 'pending'

  return (
    <div className="page">
      <h1 className="page-title">My Swaps</h1>
      <p className="page-subtitle">Track all your swap requests and their status</p>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {loading ? (
        <p style={{ color: 'var(--text-muted)' }}>Loading swaps...</p>
      ) : swaps.length === 0 ? (
        <div className="empty">
          <h3>No swaps yet</h3>
          <p>Go to Matches to initiate your first swap</p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {swaps.map(swap => {
            const isInitiator = swap.initiatorId === userId
            const otherName = isInitiator ? swap.receiverName : swap.initiatorName
            const statusLabel = getStatusLabel(swap.status)
            const statusClass = getStatusClass(swap.status)

            return (
              <div key={swap.id} className="card">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '1rem' }}>

                  <div style={{ flex: 1, minWidth: '220px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.75rem' }}>
                      <span className={`badge badge-${statusClass}`}>
                        {statusLabel}
                      </span>
                      <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                        with {otherName}
                      </span>
                    </div>

                    <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', flexWrap: 'wrap' }}>
                      <div>
                        <p style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '0.2rem' }}>
                          {isInitiator ? 'You offer' : 'They offer'}
                        </p>
                        <p style={{ fontSize: '0.95rem', fontWeight: 500 }}>
                          {swap.initiatorListing.book.title}
                        </p>
                        <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                          by {swap.initiatorListing.book.author}
                        </p>
                      </div>

                      <span style={{ color: 'var(--amber)', fontSize: '1.2rem' }}>⇄</span>

                      <div>
                        <p style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '0.2rem' }}>
                          {isInitiator ? 'You receive' : 'You offer'}
                        </p>
                        <p style={{ fontSize: '0.95rem', fontWeight: 500 }}>
                          {swap.receiverListing.book.title}
                        </p>
                        <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                          by {swap.receiverListing.book.author}
                        </p>
                      </div>
                    </div>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', alignItems: 'flex-end' }}>
                    <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                      {new Date(swap.createdAt).toLocaleDateString()}
                    </span>

                    <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', justifyContent: 'flex-end' }}>

                      {/* Receiver can accept or reject pending swaps */}
                      {!isInitiator && swap.status === 0 && (
                        <>
                          <button className="btn btn-success btn-sm"
                            onClick={() => handleAction(swap.id, 'accept')}>
                            Accept
                          </button>
                          <button className="btn btn-danger btn-sm"
                            onClick={() => handleAction(swap.id, 'reject')}>
                            Reject
                          </button>
                        </>
                      )}

                      {/* Either party can mark in transit when accepted */}
                      {swap.status === 1 && (
                        <button className="btn btn-outline btn-sm"
                          onClick={() => handleAction(swap.id, 'transit')}>
                          Mark In Transit
                        </button>
                      )}

                      {/* Either party can mark complete when in transit */}
                      {swap.status === 3 && (
                        <button className="btn btn-success btn-sm"
                          onClick={() => handleAction(swap.id, 'complete')}>
                          Mark Complete
                        </button>
                      )}

                      {/* Either party can cancel pending or accepted */}
                      {(swap.status === 0 || swap.status === 1) && (
                        <button className="btn btn-danger btn-sm"
                          onClick={() => handleAction(swap.id, 'cancel')}>
                          Cancel
                        </button>
                      )}

                      {/* Leave review when completed and not yet reviewed by this user */}
                      {swap.status === 4 && !swap.reviewerIds?.includes(userId) && (
                        <button className="btn btn-outline btn-sm"
                          onClick={() => {
                            setReviewingSwap(swap)
                            setShowReviewModal(true)
                          }}>
                          Leave Review
                        </button>
                      )}
                      {swap.status === 4 && swap.reviewerIds?.includes(userId) && (
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                          ✓ Reviewed
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}

      {showReviewModal && reviewingSwap && (
        <div className="modal-overlay" onClick={() => setShowReviewModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>Leave a Review</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.5rem' }}>
              Rate your experience with{' '}
              <strong style={{ color: 'var(--text)' }}>
                {reviewingSwap.initiatorId === userId
                  ? reviewingSwap.receiverName
                  : reviewingSwap.initiatorName}
              </strong>
            </p>
            {error && <div className="alert alert-error">{error}</div>}
            <form onSubmit={handleReview}>
              <div className="form-group">
                <label>Rating</label>
                <select
                  value={reviewForm.rating}
                  onChange={e => setReviewForm({ ...reviewForm, rating: e.target.value })}
                >
                  <option value={5}>★★★★★ — Excellent</option>
                  <option value={4}>★★★★☆ — Good</option>
                  <option value={3}>★★★☆☆ — Average</option>
                  <option value={2}>★★☆☆☆ — Poor</option>
                  <option value={1}>★☆☆☆☆ — Terrible</option>
                </select>
              </div>
              <div className="form-group">
                <label>Comment</label>
                <textarea
                  rows={3}
                  placeholder="Describe your experience..."
                  value={reviewForm.comment}
                  onChange={e => setReviewForm({ ...reviewForm, comment: e.target.value })}
                  style={{
                    background: 'var(--surface2)',
                    border: '1px solid var(--border)',
                    borderRadius: '8px',
                    padding: '0.7rem 1rem',
                    color: 'var(--text)',
                    resize: 'vertical',
                    width: '100%'
                  }}
                  required
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-outline"
                  onClick={() => setShowReviewModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Submit Review
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}