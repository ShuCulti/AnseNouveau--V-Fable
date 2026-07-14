import { useEffect, useRef, useState } from 'react'
import api from '../api/client'
import NumPad from '../components/NumPad'

// Blind count: scan a product, type what you counted; expected qty is never shown.
export default function StockCountPage() {
  const [count, setCount] = useState(null)
  const [scanned, setScanned] = useState(null)
  const [qty, setQty] = useState('')
  const [linesSaved, setLinesSaved] = useState(0)
  const [differences, setDifferences] = useState(null)
  const [message, setMessage] = useState('')
  const scanRef = useRef(null)

  useEffect(() => {
    api.get('/stockcount/open')
      .then((res) => setCount(res.data))
      .catch(() => setCount(null))
  }, [])

  useEffect(() => {
    if (count && !scanned && !differences) {
      scanRef.current?.focus()
    }
  }, [count, scanned, differences])

  function openCount() {
    api.post('/stockcount', { notes: null })
      .then(() => api.get('/stockcount/open'))
      .then((res) => { setCount(res.data); setLinesSaved(0) })
      .catch((err) => setMessage(err.response?.data?.error || 'Could not open a count'))
  }

  function handleScanKey(e) {
    if (e.key !== 'Enter') return
    const barcode = e.target.value.trim()
    e.target.value = ''
    if (!barcode) return
    api.get(`/product/by-barcode/${encodeURIComponent(barcode)}`)
      .then((res) => { setScanned(res.data); setQty('') })
      .catch(() => setMessage('Unknown barcode'))
  }

  function saveLine() {
    const value = parseFloat(qty)
    if (isNaN(value) || value < 0) return
    api.post(`/stockcount/${count.id}/lines`, { productId: scanned.id, countedQty: value })
      .then(() => {
        setScanned(null)
        setQty('')
        setLinesSaved((n) => n + 1)
      })
      .catch((err) => setMessage(err.response?.data?.error || 'Could not save line'))
  }

  function closeCount() {
    api.post(`/stockcount/${count.id}/close`)
      .then(() => api.get(`/stockcount/${count.id}/differences`))
      .then((res) => setDifferences(res.data))
      .catch((err) => setMessage(err.response?.data?.error || 'Could not close the count'))
  }

  if (differences) {
    return (
      <div className="stockcount">
        <h2>Count result</h2>
        <table className="report-table">
          <thead>
            <tr><th>Product</th><th>Expected</th><th>Counted</th><th>Difference</th></tr>
          </thead>
          <tbody>
            {differences.map((d) => (
              <tr key={d.productId} className={d.difference !== 0 ? 'bad' : ''}>
                <td>{d.productName}</td>
                <td>{d.expectedQty}</td>
                <td>{d.countedQty}</td>
                <td>{d.difference}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <button className="toggle-btn active" onClick={() => { setDifferences(null); setCount(null) }}>
          Done
        </button>
      </div>
    )
  }

  if (!count) {
    return (
      <div className="stockcount">
        <p>No stock count is open.</p>
        {message && <p className="error-text">{message}</p>}
        <button className="btn-pay" onClick={openCount}>Start stock count</button>
      </div>
    )
  }

  return (
    <div className="stockcount">
      <input
        ref={scanRef}
        className="scan-input"
        onKeyDown={handleScanKey}
        onBlur={() => { if (!scanned) scanRef.current?.focus() }}
        aria-label="barcode scanner"
        autoFocus
      />
      <p>Count #{count.id} — lines saved: {linesSaved}</p>
      {message && <p className="error-text">{message}</p>}
      {!scanned ? (
        <p className="cart-empty">Scan a product…</p>
      ) : (
        <div className="count-entry">
          <h2>{scanned.name}</h2>
          <input className="big-input" placeholder="Counted qty" value={qty} readOnly />
          <NumPad value={qty} onChange={setQty} allowDecimal />
          <div className="pay-row">
            <button className="toggle-btn" onClick={() => setScanned(null)}>Cancel</button>
            <button className="btn-pay" onClick={saveLine} disabled={qty === ''}>Save</button>
          </div>
        </div>
      )}
      <button className="btn-danger close-count" onClick={closeCount}>
        Close count
      </button>
    </div>
  )
}
