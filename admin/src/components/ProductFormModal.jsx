import { useState } from 'react'
import api from '../api/client'

const emptyUnit = { label: '', price: '', unitsPerSale: '1', isCold: false }

// One form for create and edit. On create it supports several sell units:
// the first goes along in the product POST, the rest are posted afterwards.
export default function ProductFormModal({ departments, product, onClose, onSaved }) {
  const isEdit = !!product
  const [name, setName] = useState(product?.name || '')
  const [departmentId, setDepartmentId] = useState(product?.departmentId ? String(product.departmentId) : '')
  const [barcode, setBarcode] = useState(product?.barcode || '')
  const [costPrice, setCostPrice] = useState(product ? String(product.costPrice) : '')
  const [isActive, setIsActive] = useState(product ? product.isActive : true)
  const [openingQty, setOpeningQty] = useState('')
  const [units, setUnits] = useState([{ ...emptyUnit }])
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  function setUnit(index, field, value) {
    setUnits((prev) => prev.map((u, i) => (i === index ? { ...u, [field]: value } : u)))
  }

  function validUnits() {
    return units.filter((u) => u.label.trim() && u.price !== '' && !isNaN(parseFloat(u.price)))
  }

  async function save(e) {
    e.preventDefault()
    setError('')
    if (!name.trim()) {
      setError('Name is required.')
      return
    }
    setSaving(true)
    try {
      const body = {
        departmentId: departmentId ? Number(departmentId) : null,
        barcode: barcode.trim() || null,
        name: name.trim(),
        costPrice: parseFloat(costPrice) || 0,
        isActive,
      }
      if (isEdit) {
        await api.put(`/product/${product.id}`, body)
      } else {
        const toCreate = validUnits()
        const [first, ...rest] = toCreate
        const res = await api.post('/product', {
          ...body,
          openingQty: parseFloat(openingQty) || 0,
          initialSellUnit: first
            ? {
                label: first.label.trim(),
                price: parseFloat(first.price),
                unitsPerSale: parseFloat(first.unitsPerSale) || 1,
                isCold: first.isCold,
                isActive: true,
                sortOrder: 1,
              }
            : null,
        })
        // POST returns 201 with a Location header pointing at the new product.
        const newId = Number(res.headers.location.split('/').pop())
        for (let i = 0; i < rest.length; i++) {
          await api.post('/sellunit', {
            productId: newId,
            label: rest[i].label.trim(),
            price: parseFloat(rest[i].price),
            unitsPerSale: parseFloat(rest[i].unitsPerSale) || 1,
            isCold: rest[i].isCold,
            isActive: true,
            sortOrder: i + 2,
          })
        }
      }
      onSaved(name.trim())
    } catch (err) {
      setError(err.response?.data?.error || 'Could not save the product.')
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={save}>
        <div className="modal-header">
          <h2>{isEdit ? `Edit ${product.name}` : 'New product'}</h2>
          <button type="button" className="modal-close" onClick={onClose}>✕</button>
        </div>

        <div className="form-grid">
          <label>
            Name *
            <input value={name} onChange={(e) => setName(e.target.value)} autoFocus />
          </label>
          <label>
            Department
            <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
              <option value="">— none —</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>{d.name}</option>
              ))}
            </select>
          </label>
          <label>
            Barcode (empty = quick button on the register)
            <input value={barcode} onChange={(e) => setBarcode(e.target.value)} />
          </label>
          <label>
            Cost price € (what you pay wholesale, per base unit)
            <input type="number" step="0.01" min="0" value={costPrice} onChange={(e) => setCostPrice(e.target.value)} />
          </label>
          {!isEdit && (
            <label>
              Opening stock (base units)
              <input type="number" step="0.01" min="0" value={openingQty} onChange={(e) => setOpeningQty(e.target.value)} />
            </label>
          )}
          {isEdit && (
            <label className="check">
              <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
              Active (uncheck to hide from the register)
            </label>
          )}
        </div>

        {!isEdit && (
          <>
            <h3>Sell units (how it's sold: single, cold single, six-pack, case…)</h3>
            <table className="unit-table">
              <thead>
                <tr>
                  <th>Label</th>
                  <th>Price €</th>
                  <th>Base units per sale</th>
                  <th>Cold</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {units.map((u, i) => (
                  <tr key={i}>
                    <td><input value={u.label} placeholder="e.g. Cold single" onChange={(e) => setUnit(i, 'label', e.target.value)} /></td>
                    <td><input type="number" step="0.01" min="0" value={u.price} onChange={(e) => setUnit(i, 'price', e.target.value)} /></td>
                    <td><input type="number" step="0.01" min="0.01" value={u.unitsPerSale} onChange={(e) => setUnit(i, 'unitsPerSale', e.target.value)} /></td>
                    <td className="center">
                      <input type="checkbox" checked={u.isCold} onChange={(e) => setUnit(i, 'isCold', e.target.checked)} />
                    </td>
                    <td>
                      {units.length > 1 && (
                        <button type="button" className="btn-small" onClick={() => setUnits(units.filter((_, j) => j !== i))}>
                          Remove
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <button type="button" className="btn-small" onClick={() => setUnits([...units, { ...emptyUnit }])}>
              + another sell unit
            </button>
          </>
        )}

        {error && <p className="error-text">{error}</p>}
        <div className="modal-actions">
          <button type="button" className="btn-plain" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving ? 'Saving…' : isEdit ? 'Save changes' : 'Create product'}
          </button>
        </div>
      </form>
    </div>
  )
}
