import { useEffect, useRef, useState } from 'react'
import api from '../api/client'
import Modal from '../components/Modal'
import NumPad from '../components/NumPad'

function money(value, currency = 'EUR') {
  return `${currency === 'USD' ? '$' : currency === 'ANG' ? 'ƒ' : '€'}${Number(value).toFixed(2)}`
}

export default function PosPage() {
  const [products, setProducts] = useState([])
  const [rates, setRates] = useState([])
  const [cart, setCart] = useState([])
  const [pickerProduct, setPickerProduct] = useState(null)
  const [unknownBarcode, setUnknownBarcode] = useState(null)
  const [paying, setPaying] = useState(false)
  const [receipt, setReceipt] = useState(null)
  const [message, setMessage] = useState('')
  const scanRef = useRef(null)
  const modalOpen = pickerProduct || unknownBarcode || paying || receipt

  useEffect(() => {
    loadProducts()
    api.get('/exchangerate').then((res) => setRates(res.data)).catch(() => {})
  }, [])

  // The USB scanner types into an invisible input; keep it focused whenever no dialog is open.
  useEffect(() => {
    if (!modalOpen) {
      scanRef.current?.focus()
    }
  }, [modalOpen, cart])

  function loadProducts() {
    api.get('/product').then((res) => setProducts(res.data)).catch(() => {})
  }

  function handleScanKey(e) {
    if (e.key !== 'Enter') return
    const barcode = e.target.value.trim()
    e.target.value = ''
    if (!barcode) return
    api.get(`/product/by-barcode/${encodeURIComponent(barcode)}`)
      .then((res) => addProduct(res.data))
      .catch((err) => {
        if (err.response && err.response.status === 404) {
          setUnknownBarcode(barcode)
        } else {
          flash('Scan failed, try again')
        }
      })
  }

  function flash(text) {
    setMessage(text)
    setTimeout(() => setMessage(''), 2500)
  }

  function addProduct(product) {
    const units = product.sellUnits.filter((u) => u.isActive)
    if (units.length === 0) {
      flash(`${product.name} has no price yet`)
    } else if (units.length === 1) {
      addUnit(product, units[0])
    } else {
      setPickerProduct(product)
    }
  }

  function addUnit(product, unit) {
    setPickerProduct(null)
    setCart((prev) => {
      const existing = prev.find((l) => l.sellUnitId === unit.id)
      if (existing) {
        return prev.map((l) => (l.sellUnitId === unit.id ? { ...l, qty: l.qty + 1 } : l))
      }
      return [...prev, { sellUnitId: unit.id, name: `${product.name} (${unit.label})`, price: unit.price, qty: 1 }]
    })
  }

  function addAdhoc(name, price) {
    setUnknownBarcode(null)
    setCart((prev) => [...prev, { sellUnitId: null, name, price, qty: 1 }])
  }

  function changeQty(index, delta) {
    setCart((prev) =>
      prev
        .map((l, i) => (i === index ? { ...l, qty: l.qty + delta } : l))
        .filter((l) => l.qty > 0)
    )
  }

  const total = cart.reduce((sum, l) => sum + l.price * l.qty, 0)
  const quickProducts = products.filter((p) => !p.barcode && p.sellUnits.some((u) => u.isActive))

  function completeSale(paymentMethod, tenderCurrency, amountTendered) {
    const body = {
      lines: cart.map((l) => ({
        sellUnitId: l.sellUnitId,
        name: l.sellUnitId ? null : l.name,
        price: l.sellUnitId ? null : l.price,
        qty: l.qty,
      })),
      paymentMethod,
      tenderCurrency,
      amountTendered,
    }
    api.post('/sale', body)
      .then((res) => {
        setPaying(false)
        setCart([])
        setReceipt(res.data)
        loadProducts()
      })
      .catch((err) => {
        flash(err.response?.data?.error || 'Sale failed')
      })
  }

  return (
    <div className="pos">
      <input
        ref={scanRef}
        className="scan-input"
        onKeyDown={handleScanKey}
        onBlur={() => { if (!modalOpen) scanRef.current?.focus() }}
        aria-label="barcode scanner"
        autoFocus
      />

      <div className="pos-left">
        <div className="cart">
          {cart.length === 0 && <p className="cart-empty">Scan an item or tap a button →</p>}
          {cart.map((line, i) => (
            <div key={i} className="cart-line">
              <span className="cart-name">{line.name}</span>
              <div className="cart-qty">
                <button onClick={() => changeQty(i, -1)}>−</button>
                <span>{line.qty}</span>
                <button onClick={() => changeQty(i, 1)}>+</button>
              </div>
              <span className="cart-price">{money(line.price * line.qty)}</span>
            </div>
          ))}
        </div>
        <div className="pos-footer">
          {message && <p className="error-text">{message}</p>}
          <div className="pos-total">{money(total)}</div>
          <div className="pos-actions">
            <button className="btn-danger" disabled={cart.length === 0} onClick={() => setCart([])}>
              Clear
            </button>
            <button className="btn-pay" disabled={cart.length === 0} onClick={() => setPaying(true)}>
              PAY
            </button>
          </div>
        </div>
      </div>

      <div className="pos-right">
        <div className="quick-grid">
          {quickProducts.map((p) => (
            <button key={p.id} className="quick-btn" onClick={() => addProduct(p)}>
              {p.name}
            </button>
          ))}
        </div>
      </div>

      {pickerProduct && (
        <Modal title={pickerProduct.name} onClose={() => setPickerProduct(null)}>
          <div className="unit-picker">
            {pickerProduct.sellUnits.filter((u) => u.isActive).map((u) => (
              <button key={u.id} className={u.isCold ? 'unit-btn cold' : 'unit-btn'} onClick={() => addUnit(pickerProduct, u)}>
                <span>{u.label}</span>
                <strong>{money(u.price)}</strong>
              </button>
            ))}
          </div>
        </Modal>
      )}

      {unknownBarcode && (
        <UnknownItemModal
          barcode={unknownBarcode}
          onClose={() => setUnknownBarcode(null)}
          onCreated={(name, price) => { addAdhoc(name, price); loadProducts() }}
        />
      )}

      {paying && (
        <PaymentModal
          total={total}
          rates={rates}
          onClose={() => setPaying(false)}
          onComplete={completeSale}
        />
      )}

      {receipt && (
        <Modal title="Done!" onClose={() => setReceipt(null)}>
          <div className="change-screen">
            {receipt.changeGiven != null ? (
              <>
                <p>Change due</p>
                <div className="change-amount">{money(receipt.changeGiven, receipt.tenderCurrency)}</div>
              </>
            ) : (
              <div className="change-amount">{money(receipt.totalInTenderCurrency, receipt.tenderCurrency)}</div>
            )}
            <button className="btn-pay" onClick={() => setReceipt(null)}>New Sale</button>
          </div>
        </Modal>
      )}
    </div>
  )
}

// Unknown barcode: create the product with one sell unit on the fly, minimal questions.
function UnknownItemModal({ barcode, onClose, onCreated }) {
  const [name, setName] = useState('')
  const [price, setPrice] = useState('')

  function save() {
    const parsedPrice = parseFloat(price)
    if (!name.trim() || isNaN(parsedPrice) || parsedPrice < 0) return
    api.post('/product', {
      barcode,
      name: name.trim(),
      costPrice: 0,
      isActive: true,
      openingQty: 0,
      initialSellUnit: { label: 'Each', price: parsedPrice, unitsPerSale: 1, isCold: false, isActive: true, sortOrder: 1 },
    })
      .then(() => onCreated(name.trim(), parsedPrice))
      .catch(() => onCreated(name.trim(), parsedPrice)) // still sell it ad-hoc if the save fails
  }

  return (
    <Modal title="New item" onClose={onClose}>
      <div className="unknown-item">
        <p className="unknown-barcode">Barcode: {barcode}</p>
        <input
          className="big-input"
          placeholder="Item name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          autoFocus
        />
        <input className="big-input" placeholder="Price €" value={price} readOnly />
        <NumPad value={price} onChange={setPrice} allowDecimal />
        <button className="btn-pay" onClick={save} disabled={!name.trim() || !price}>
          Add to sale
        </button>
      </div>
    </Modal>
  )
}

function PaymentModal({ total, rates, onClose, onComplete }) {
  const [currency, setCurrency] = useState('EUR')
  const [method, setMethod] = useState('Cash')
  const [tendered, setTendered] = useState('')

  const currencies = ['EUR', ...rates.map((r) => r.currencyCode.trim())]
  const rate = currency === 'EUR' ? 1 : rates.find((r) => r.currencyCode.trim() === currency)?.rateToBase || 1
  const totalInCurrency = total / rate
  const tenderedValue = parseFloat(tendered) || 0
  const change = tenderedValue - totalInCurrency

  function confirm() {
    onComplete(method, currency, method === 'Cash' ? tenderedValue : null)
  }

  return (
    <Modal title="Payment" onClose={onClose}>
      <div className="payment">
        <div className="pay-row">
          {currencies.map((c) => (
            <button key={c} className={c === currency ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setCurrency(c)}>
              {c}
            </button>
          ))}
        </div>
        <div className="pay-total">{money(totalInCurrency, currency)}</div>
        <div className="pay-row">
          {['Cash', 'Card', 'Other'].map((m) => (
            <button key={m} className={m === method ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setMethod(m)}>
              {m}
            </button>
          ))}
        </div>
        {method === 'Cash' && (
          <>
            <input className="big-input" placeholder="Cash received" value={tendered} readOnly />
            <NumPad value={tendered} onChange={setTendered} allowDecimal />
            <div className={change >= 0 ? 'pay-change ok' : 'pay-change'}>
              Change: {money(Math.max(change, 0), currency)}
            </div>
          </>
        )}
        <button
          className="btn-pay"
          disabled={method === 'Cash' && change < 0}
          onClick={confirm}
        >
          COMPLETE SALE
        </button>
      </div>
    </Modal>
  )
}
