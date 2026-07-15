import { useState } from 'react'
import api from '../api/client'

// Add a unit to an existing product, or edit one. A changed price goes through
// the dedicated price endpoint so PriceHistory is written.
export default function SellUnitModal({ product, unit, onClose, onSaved }) {
  const isEdit = !!unit
  const [label, setLabel] = useState(unit?.label || '')
  const [price, setPrice] = useState(unit ? String(unit.price) : '')
  const [unitsPerSale, setUnitsPerSale] = useState(unit ? String(unit.unitsPerSale) : '1')
  const [isCold, setIsCold] = useState(unit?.isCold || false)
  const [isActive, setIsActive] = useState(unit ? unit.isActive : true)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  async function save(e) {
    e.preventDefault()
    setError('')
    const parsedPrice = parseFloat(price)
    if (!label.trim() || isNaN(parsedPrice) || parsedPrice < 0) {
      setError('Label and a valid price are required.')
      return
    }
    setSaving(true)
    try {
      if (isEdit) {
        await api.put(`/sellunit/${unit.id}`, {
          productId: product.id,
          label: label.trim(),
          price: unit.price,
          unitsPerSale: parseFloat(unitsPerSale) || 1,
          isCold,
          isActive,
          sortOrder: unit.sortOrder,
        })
        if (parsedPrice !== unit.price) {
          await api.put(`/sellunit/${unit.id}/price`, { newPrice: parsedPrice })
        }
        onSaved(`${product.name} (${label.trim()}) saved`)
      } else {
        await api.post('/sellunit', {
          productId: product.id,
          label: label.trim(),
          price: parsedPrice,
          unitsPerSale: parseFloat(unitsPerSale) || 1,
          isCold,
          isActive,
          sortOrder: product.sellUnits.length + 1,
        })
        onSaved(`${product.name} (${label.trim()}) added`)
      }
    } catch (err) {
      setError(err.response?.data?.error || 'Could not save the sell unit.')
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={save}>
        <div className="modal-header">
          <h2>{isEdit ? `Edit unit — ${product.name}` : `New unit — ${product.name}`}</h2>
          <button type="button" className="modal-close" onClick={onClose}>✕</button>
        </div>
        <div className="form-grid">
          <label>
            Label *
            <input value={label} placeholder="e.g. Six-pack" onChange={(e) => setLabel(e.target.value)} autoFocus />
          </label>
          <label>
            Price € *
            <input type="number" step="0.01" min="0" value={price} onChange={(e) => setPrice(e.target.value)} />
          </label>
          <label>
            Base units per sale (case of 24 = 24)
            <input type="number" step="0.01" min="0.01" value={unitsPerSale} onChange={(e) => setUnitsPerSale(e.target.value)} />
          </label>
          <label className="check">
            <input type="checkbox" checked={isCold} onChange={(e) => setIsCold(e.target.checked)} />
            Cold
          </label>
          <label className="check">
            <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
            Active
          </label>
        </div>
        {error && <p className="error-text">{error}</p>}
        <div className="modal-actions">
          <button type="button" className="btn-plain" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
        </div>
      </form>
    </div>
  )
}
