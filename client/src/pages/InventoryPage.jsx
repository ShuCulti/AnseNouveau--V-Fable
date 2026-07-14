import { useEffect, useState } from 'react'
import api from '../api/client'
import Modal from '../components/Modal'
import NumPad from '../components/NumPad'

const LOW_STOCK_THRESHOLD = 10
const ADJUSTMENT_REASONS = ['Damaged', 'Expired', 'Own use', 'Correction', 'Other']

export default function InventoryPage() {
  const [products, setProducts] = useState([])
  const [filter, setFilter] = useState('')
  const [entry, setEntry] = useState(null) // { product, mode: 'delivery' | 'adjustment' }
  const [message, setMessage] = useState('')

  useEffect(() => {
    load()
  }, [])

  function load() {
    api.get('/product').then((res) => setProducts(res.data)).catch(() => {})
  }

  function flash(text) {
    setMessage(text)
    setTimeout(() => setMessage(''), 3000)
  }

  const visible = products.filter((p) => p.name.toLowerCase().includes(filter.toLowerCase()))

  return (
    <div className="inventory">
      <input
        className="big-input"
        placeholder="Filter products…"
        value={filter}
        onChange={(e) => setFilter(e.target.value)}
      />
      {message && <p className="ok-text">{message}</p>}
      <div className="inv-list">
        {visible.map((p) => (
          <div key={p.id} className={p.stockQty < LOW_STOCK_THRESHOLD ? 'inv-row low' : 'inv-row'}>
            <span className="inv-name">{p.name}</span>
            <span className="inv-qty">{p.stockQty}</span>
            <button onClick={() => setEntry({ product: p, mode: 'delivery' })}>+ Delivery</button>
            <button onClick={() => setEntry({ product: p, mode: 'adjustment' })}>± Adjust</button>
          </div>
        ))}
      </div>

      {entry && (
        <StockEntryModal
          entry={entry}
          onClose={() => setEntry(null)}
          onSaved={(text) => { setEntry(null); flash(text); load() }}
        />
      )}
    </div>
  )
}

function StockEntryModal({ entry, onClose, onSaved }) {
  const [qty, setQty] = useState('')
  const [negative, setNegative] = useState(false)
  const [reason, setReason] = useState(ADJUSTMENT_REASONS[0])
  const isDelivery = entry.mode === 'delivery'

  function save() {
    const value = parseFloat(qty)
    if (isNaN(value) || value <= 0) return
    if (isDelivery) {
      api.post('/stock/delivery', { productId: entry.product.id, qty: value, reason: null })
        .then(() => onSaved(`${entry.product.name}: +${value} received`))
        .catch(() => onSaved('Could not save delivery'))
    } else {
      const delta = negative ? -value : value
      api.post('/stock/adjustment', { productId: entry.product.id, qtyDelta: delta, reason })
        .then(() => onSaved(`${entry.product.name}: ${delta > 0 ? '+' : ''}${delta} adjusted`))
        .catch(() => onSaved('Could not save adjustment'))
    }
  }

  return (
    <Modal title={`${isDelivery ? 'Delivery' : 'Adjustment'} — ${entry.product.name}`} onClose={onClose}>
      <div className="stock-entry">
        {!isDelivery && (
          <div className="pay-row">
            <button className={negative ? 'toggle-btn' : 'toggle-btn active'} onClick={() => setNegative(false)}>
              + Add
            </button>
            <button className={negative ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setNegative(true)}>
              − Remove
            </button>
          </div>
        )}
        <input className="big-input" placeholder="Quantity" value={qty} readOnly />
        <NumPad value={qty} onChange={setQty} allowDecimal />
        {!isDelivery && (
          <div className="pay-row reasons">
            {ADJUSTMENT_REASONS.map((r) => (
              <button key={r} className={r === reason ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setReason(r)}>
                {r}
              </button>
            ))}
          </div>
        )}
        <button className="btn-pay" onClick={save} disabled={!qty}>
          Save
        </button>
      </div>
    </Modal>
  )
}
