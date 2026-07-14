import { useEffect, useRef, useState } from 'react'
import api from '../api/client'
import Modal from '../components/Modal'
import NumPad from '../components/NumPad'

// Massive fonts, minimal chrome: search or scan, tap a price, type the new one.
export default function PricesPage() {
  const [term, setTerm] = useState('')
  const [products, setProducts] = useState([])
  const [editingUnit, setEditingUnit] = useState(null)
  const [newPrice, setNewPrice] = useState('')
  const [message, setMessage] = useState('')
  const searchRef = useRef(null)

  useEffect(() => {
    const handle = setTimeout(() => {
      if (!term.trim()) {
        setProducts([])
        return
      }
      // A scanner "types" the barcode; numeric terms are tried as barcode first.
      if (/^\d{6,}$/.test(term.trim())) {
        api.get(`/product/by-barcode/${encodeURIComponent(term.trim())}`)
          .then((res) => setProducts([res.data]))
          .catch(() => searchByName())
      } else {
        searchByName()
      }
      function searchByName() {
        api.get('/product/search', { params: { term: term.trim() } })
          .then((res) => setProducts(res.data))
          .catch(() => {})
      }
    }, 300)
    return () => clearTimeout(handle)
  }, [term])

  function savePrice() {
    const price = parseFloat(newPrice)
    if (isNaN(price) || price < 0) return
    api.put(`/sellunit/${editingUnit.id}/price`, { newPrice: price })
      .then(() => {
        setMessage(`${editingUnit.label} → €${price.toFixed(2)} saved`)
        setEditingUnit(null)
        setNewPrice('')
        setProducts((prev) =>
          prev.map((p) => ({
            ...p,
            sellUnits: p.sellUnits.map((u) => (u.id === editingUnit.id ? { ...u, price } : u)),
          }))
        )
        setTimeout(() => setMessage(''), 3000)
      })
      .catch((err) => setMessage(err.response?.data?.error || 'Could not save price'))
  }

  return (
    <div className="prices">
      <input
        ref={searchRef}
        className="big-input"
        placeholder="Scan or type a product name…"
        value={term}
        onChange={(e) => setTerm(e.target.value)}
        autoFocus
      />
      {message && <p className="ok-text">{message}</p>}
      <div className="price-list">
        {products.map((p) => (
          <div key={p.id} className="price-product">
            <h2>{p.name}</h2>
            <div className="price-units">
              {p.sellUnits.map((u) => (
                <button key={u.id} className="price-btn" onClick={() => { setEditingUnit(u); setNewPrice('') }}>
                  <span>{u.label}</span>
                  <strong>€{u.price.toFixed(2)}</strong>
                </button>
              ))}
            </div>
          </div>
        ))}
      </div>

      {editingUnit && (
        <Modal title={editingUnit.label} onClose={() => setEditingUnit(null)}>
          <div className="price-edit">
            <p className="price-old">Now: €{editingUnit.price.toFixed(2)}</p>
            <div className="price-new">{newPrice || '0'}</div>
            <NumPad value={newPrice} onChange={setNewPrice} allowDecimal />
            <button className="btn-pay" onClick={savePrice} disabled={!newPrice}>
              Save price
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}
