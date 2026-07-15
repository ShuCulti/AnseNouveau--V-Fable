import { useEffect, useState } from 'react'
import api from '../api/client'
import ProductFormModal from '../components/ProductFormModal'
import SellUnitModal from '../components/SellUnitModal'

export default function ProductsPage() {
  const [products, setProducts] = useState([])
  const [departments, setDepartments] = useState([])
  const [filter, setFilter] = useState('')
  const [creating, setCreating] = useState(false)
  const [editingProduct, setEditingProduct] = useState(null)
  const [unitModal, setUnitModal] = useState(null) // { product, unit: null for new }
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

  const visible = products.filter(
    (p) =>
      p.name.toLowerCase().includes(filter.toLowerCase()) ||
      (p.barcode || '').includes(filter)
  )

  return (
    <div>
      <div className="toolbar">
        <input
          className="search"
          placeholder="Filter by name or barcode…"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
        />
        <button className="btn-primary" onClick={() => setCreating(true)}>
          + New product
        </button>
      </div>
      {message && <p className="ok-text">{message}</p>}

      <table className="data-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Department</th>
            <th>Barcode</th>
            <th className="num">Cost</th>
            <th className="num">Stock</th>
            <th>Sell units</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {visible.map((p) => (
            <tr key={p.id}>
              <td>{p.name}</td>
              <td>{departmentName(p.departmentId)}</td>
              <td>{p.barcode || <span className="muted">quick button</span>}</td>
              <td className="num">€{p.costPrice.toFixed(2)}</td>
              <td className={p.stockQty < 10 ? 'num low' : 'num'}>{p.stockQty}</td>
              <td>
                <div className="unit-chips">
                  {p.sellUnits.map((u) => (
                    <button
                      key={u.id}
                      className="chip"
                      title="Edit unit / change price"
                      onClick={() => setUnitModal({ product: p, unit: u })}
                    >
                      {u.label} €{u.price.toFixed(2)}
                      {u.isCold ? ' ❄' : ''}
                    </button>
                  ))}
                  <button className="chip add" onClick={() => setUnitModal({ product: p, unit: null })}>
                    + unit
                  </button>
                </div>
              </td>
              <td>
                <button className="btn-small" onClick={() => setEditingProduct(p)}>
                  Edit
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {creating && (
        <ProductFormModal
          departments={departments}
          onClose={() => setCreating(false)}
          onSaved={(name) => { setCreating(false); flash(`${name} created`); load() }}
        />
      )}

      {editingProduct && (
        <ProductFormModal
          departments={departments}
          product={editingProduct}
          onClose={() => setEditingProduct(null)}
          onSaved={(name) => { setEditingProduct(null); flash(`${name} saved`); load() }}
        />
      )}

      {unitModal && (
        <SellUnitModal
          product={unitModal.product}
          unit={unitModal.unit}
          onClose={() => setUnitModal(null)}
          onSaved={(text) => { setUnitModal(null); flash(text); load() }}
        />
      )}
    </div>
  )
}
