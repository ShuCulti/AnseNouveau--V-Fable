import { useEffect, useState } from 'react'
import api from '../api/client'

// Spreadsheet-style price grid: one row per product, one column per unit kind
// (cold/warm × pack size). Click a cell to edit; typing a price into an empty
// cell creates that sell unit.
export default function GridPage() {
  const [products, setProducts] = useState([])
  const [departments, setDepartments] = useState([])
  const [extraColumns, setExtraColumns] = useState([])
  const [newPackCold, setNewPackCold] = useState('false')
  const [newPackSize, setNewPackSize] = useState('')
  const [message, setMessage] = useState('')

  useEffect(() => {
    load()
    api.get('/department').then((res) => setDepartments(res.data)).catch(() => {})
  }, [])

  function load() {
    api.get('/product').then((res) => setProducts(res.data)).catch(() => {})
  }

  function flash(text) {
    setMessage(text)
    setTimeout(() => setMessage(''), 4000)
  }

  function departmentName(id) {
    return departments.find((d) => d.id === id)?.name || '—'
  }

  const columns = buildColumns(products, extraColumns)

  function findUnit(product, column) {
    return product.sellUnits.find(
      (u) => u.isActive && u.isCold === column.isCold && u.unitsPerSale === column.unitsPerSale
    )
  }

  async function savePrice(product, column, value) {
    const unit = findUnit(product, column)
    try {
      if (unit) {
        await api.put(`/sellunit/${unit.id}/price`, { newPrice: value })
      } else {
        await api.post('/sellunit', {
          productId: product.id,
          label: unitLabel(column),
          price: value,
          unitsPerSale: column.unitsPerSale,
          isCold: column.isCold,
          isActive: true,
          sortOrder: product.sellUnits.length + 1,
        })
      }
      flash(`${product.name} — ${unitLabel(column)} → €${value.toFixed(2)}`)
      load()
    } catch (err) {
      flash(err.response?.data?.error || 'Could not save the price')
    }
  }

  async function saveCost(product, value) {
    try {
      await api.put(`/product/${product.id}`, {
        departmentId: product.departmentId,
        barcode: product.barcode,
        name: product.name,
        costPrice: value,
        isActive: product.isActive,
      })
      flash(`${product.name} — cost → €${value.toFixed(2)}`)
      load()
    } catch (err) {
      flash(err.response?.data?.error || 'Could not save the cost price')
    }
  }

  function addPackColumn() {
    const size = parseFloat(newPackSize)
    if (isNaN(size) || size <= 1) return
    const isCold = newPackCold === 'true'
    setExtraColumns((prev) => [...prev, { isCold, unitsPerSale: size }])
    setNewPackSize('')
  }

  return (
    <div>
      <div className="toolbar">
        <span className="hint">Click a price to change it; type into an empty cell to add that unit.</span>
        <div className="add-column">
          <select value={newPackCold} onChange={(e) => setNewPackCold(e.target.value)}>
            <option value="false">Warm</option>
            <option value="true">Cold</option>
          </select>
          <input
            type="number"
            min="2"
            placeholder="pack size"
            value={newPackSize}
            onChange={(e) => setNewPackSize(e.target.value)}
          />
          <button className="btn-small" onClick={addPackColumn}>+ column</button>
        </div>
      </div>
      {message && <p className="ok-text">{message}</p>}

      <div className="grid-scroll">
        <table className="data-table grid-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Department</th>
              <th className="num">Stock</th>
              <th className="num">Cost €</th>
              {columns.map((c) => (
                <th key={c.key} className="num">{c.label}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {products.map((p) => (
              <tr key={p.id}>
                <td>{p.name}</td>
                <td>{departmentName(p.departmentId)}</td>
                <td className={p.stockQty < 10 ? 'num low' : 'num'}>{p.stockQty}</td>
                <EditableCell value={p.costPrice} onSave={(v) => saveCost(p, v)} />
                {columns.map((c) => {
                  const unit = findUnit(p, c)
                  return (
                    <EditableCell
                      key={c.key}
                      value={unit ? unit.price : null}
                      cold={c.isCold}
                      onSave={(v) => savePrice(p, c, v)}
                    />
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function buildColumns(products, extraColumns) {
  // Cold and warm singles are always shown; pack sizes come from the data.
  const map = new Map()
  function add(isCold, unitsPerSale) {
    const key = `${isCold ? 'C' : 'W'}:${unitsPerSale}`
    if (!map.has(key)) {
      map.set(key, { key, isCold, unitsPerSale })
    }
  }
  add(true, 1)
  add(false, 1)
  for (const p of products) {
    for (const u of p.sellUnits) {
      if (u.isActive) {
        add(u.isCold, u.unitsPerSale)
      }
    }
  }
  for (const c of extraColumns) {
    add(c.isCold, c.unitsPerSale)
  }
  return [...map.values()]
    .sort((a, b) => a.unitsPerSale - b.unitsPerSale || (a.isCold === b.isCold ? 0 : a.isCold ? -1 : 1))
    .map((c) => ({ ...c, label: columnLabel(c) }))
}

function columnLabel(column) {
  if (column.unitsPerSale === 1) {
    return column.isCold ? 'Cold' : 'Warm'
  }
  return `${column.isCold ? 'Cold' : 'Warm'} ×${column.unitsPerSale}`
}

function unitLabel(column) {
  if (column.unitsPerSale === 1) {
    return column.isCold ? 'Cold single' : 'Warm single'
  }
  return `${column.isCold ? 'Cold case' : 'Case'} (${column.unitsPerSale})`
}

function EditableCell({ value, cold, onSave }) {
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState('')

  function start() {
    setDraft(value != null ? String(value) : '')
    setEditing(true)
  }

  function commit() {
    setEditing(false)
    const parsed = parseFloat(draft)
    if (isNaN(parsed) || parsed < 0 || parsed === value) return
    onSave(parsed)
  }

  if (editing) {
    return (
      <td className="num cell-editing">
        <input
          type="number"
          step="0.01"
          min="0"
          value={draft}
          autoFocus
          onChange={(e) => setDraft(e.target.value)}
          onBlur={commit}
          onKeyDown={(e) => {
            if (e.key === 'Enter') e.target.blur()
            if (e.key === 'Escape') setEditing(false)
          }}
        />
      </td>
    )
  }
  return (
    <td className={`num cell-editable${cold ? ' cell-cold' : ''}`} onClick={start}>
      {value != null ? `€${value.toFixed(2)}` : <span className="cell-empty">—</span>}
    </td>
  )
}
