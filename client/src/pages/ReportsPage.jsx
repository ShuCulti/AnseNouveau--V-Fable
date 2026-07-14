import { useState } from 'react'
import api from '../api/client'
import NumPad from '../components/NumPad'

function today() {
  return new Date().toISOString().slice(0, 10)
}

export default function ReportsPage() {
  const [tab, setTab] = useState('z')
  return (
    <div className="reports">
      <div className="pay-row tabs">
        <button className={tab === 'z' ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setTab('z')}>Today (Z)</button>
        <button className={tab === 'sales' ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setTab('sales')}>Sales</button>
        <button className={tab === 'stock' ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setTab('stock')}>Stock</button>
        <button className={tab === 'profit' ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setTab('profit')}>Profit</button>
        <button className={tab === 'cash' ? 'toggle-btn active' : 'toggle-btn'} onClick={() => setTab('cash')}>Cash count</button>
      </div>
      {tab === 'z' && <ZReportTab />}
      {tab === 'sales' && <SalesTab />}
      {tab === 'stock' && <StockTab />}
      {tab === 'profit' && <ProfitTab />}
      {tab === 'cash' && <CashCountTab />}
    </div>
  )
}

function ZReportTab() {
  const [date, setDate] = useState(today())
  const [report, setReport] = useState(null)

  function load() {
    api.get('/report/z', { params: { date } }).then((res) => setReport(res.data)).catch(() => {})
  }

  return (
    <div>
      <div className="report-controls">
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        <button className="toggle-btn active" onClick={load}>Show</button>
      </div>
      {report && (
        <div className="zreport">
          <div className="z-line big"><span>Sales</span><span>€{report.salesTotal.toFixed(2)}</span></div>
          <div className="z-line"><span>Refunds</span><span>€{report.refundTotal.toFixed(2)}</span></div>
          <div className="z-line big"><span>Net</span><span>€{report.netTotal.toFixed(2)}</span></div>
          <div className="z-line"><span>Cash</span><span>€{report.cashTotal.toFixed(2)}</span></div>
          <div className="z-line"><span>Card</span><span>€{report.cardTotal.toFixed(2)}</span></div>
          <div className="z-line"><span>Other</span><span>€{report.otherTotal.toFixed(2)}</span></div>
          <div className="z-line"><span>Sale count</span><span>{report.saleCount}</span></div>
          <h3>Per department</h3>
          {report.departments.map((d, i) => (
            <div key={i} className="z-line"><span>{d.departmentName}</span><span>€{d.total.toFixed(2)}</span></div>
          ))}
        </div>
      )}
    </div>
  )
}

function SalesTab() {
  const [from, setFrom] = useState(today())
  const [to, setTo] = useState(today())
  const [sales, setSales] = useState([])

  function load() {
    api.get('/report/sales', { params: { from, to } }).then((res) => setSales(res.data)).catch(() => {})
  }

  return (
    <div>
      <div className="report-controls">
        <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        <button className="toggle-btn active" onClick={load}>Show</button>
      </div>
      <table className="report-table">
        <thead>
          <tr><th>#</th><th>Time (UTC)</th><th>Status</th><th>Pay</th><th>Total</th></tr>
        </thead>
        <tbody>
          {sales.map((s) => (
            <tr key={s.id}>
              <td>{s.id}</td>
              <td>{s.saleTimeUtc.replace('T', ' ').slice(0, 16)}</td>
              <td>{s.status}</td>
              <td>{s.paymentMethod}</td>
              <td>€{s.totalAmount.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function StockTab() {
  const [from, setFrom] = useState(today())
  const [to, setTo] = useState(today())
  const [rows, setRows] = useState([])

  function load() {
    api.get('/report/stock', { params: { from, to } }).then((res) => setRows(res.data)).catch(() => {})
  }

  return (
    <div>
      <div className="report-controls">
        <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        <button className="toggle-btn active" onClick={load}>Show</button>
      </div>
      <table className="report-table">
        <thead>
          <tr><th>Product</th><th>Opening</th><th>Sold</th><th>Delivered</th><th>Adjusted</th><th>Closing</th></tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.productId}>
              <td>{r.productName}</td>
              <td>{r.openingQty}</td>
              <td>{r.soldQty}</td>
              <td>{r.deliveredQty}</td>
              <td>{r.adjustedQty}</td>
              <td>{r.closingQty}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ProfitTab() {
  const [from, setFrom] = useState(today())
  const [to, setTo] = useState(today())
  const [rows, setRows] = useState([])

  function load() {
    api.get('/report/profit', { params: { from, to } }).then((res) => setRows(res.data)).catch(() => {})
  }

  const totalProfit = rows.reduce((sum, r) => sum + r.profit, 0)

  return (
    <div>
      <div className="report-controls">
        <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        <button className="toggle-btn active" onClick={load}>Show</button>
      </div>
      <table className="report-table">
        <thead>
          <tr><th>Product</th><th>Units</th><th>Revenue</th><th>Cost</th><th>Profit</th></tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.productId}>
              <td>{r.productName}</td>
              <td>{r.unitsSold}</td>
              <td>€{r.revenue.toFixed(2)}</td>
              <td>€{r.cost.toFixed(2)}</td>
              <td>€{r.profit.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
        {rows.length > 0 && (
          <tfoot>
            <tr><td colSpan="4">Total</td><td>€{totalProfit.toFixed(2)}</td></tr>
          </tfoot>
        )}
      </table>
    </div>
  )
}

// End-of-day: enter float + counted cash, see the difference, save.
function CashCountTab() {
  const [businessDate, setBusinessDate] = useState(today())
  const [floatAmount, setFloatAmount] = useState('')
  const [counted, setCounted] = useState('')
  const [expected, setExpected] = useState(null)
  const [saved, setSaved] = useState(null)
  const [error, setError] = useState('')

  function loadExpected() {
    api.get('/cashcount/expected', { params: { businessDate, floatAmount: parseFloat(floatAmount) || 0 } })
      .then((res) => setExpected(res.data.expectedCash))
      .catch(() => setError('Could not calculate expected cash'))
  }

  function save() {
    setError('')
    api.post('/cashcount', {
      businessDate,
      floatAmount: parseFloat(floatAmount) || 0,
      countedCash: parseFloat(counted) || 0,
      notes: null,
    })
      .then((res) => setSaved(res.data))
      .catch((err) => setError(err.response?.data?.error || 'Could not save cash count'))
  }

  if (saved) {
    return (
      <div className="cash-result">
        <div className="z-line big"><span>Expected</span><span>€{saved.expectedCash.toFixed(2)}</span></div>
        <div className="z-line big"><span>Counted</span><span>€{saved.countedCash.toFixed(2)}</span></div>
        <div className={saved.difference === 0 ? 'z-line big ok' : 'z-line big bad'}>
          <span>Difference</span><span>€{saved.difference.toFixed(2)}</span>
        </div>
        <button className="toggle-btn active" onClick={() => setSaved(null)}>Back</button>
      </div>
    )
  }

  return (
    <div className="cash-count">
      <div className="report-controls">
        <input type="date" value={businessDate} onChange={(e) => setBusinessDate(e.target.value)} />
      </div>
      <label className="cash-label">Float (start cash)</label>
      <input className="big-input" value={floatAmount} readOnly />
      <NumPad value={floatAmount} onChange={setFloatAmount} allowDecimal />
      <button className="toggle-btn active" onClick={loadExpected}>Show expected</button>
      {expected != null && <div className="z-line big"><span>Expected</span><span>€{expected.toFixed(2)}</span></div>}
      <label className="cash-label">Counted cash</label>
      <input className="big-input" value={counted} readOnly />
      <NumPad value={counted} onChange={setCounted} allowDecimal />
      {error && <p className="error-text">{error}</p>}
      <button className="btn-pay" onClick={save} disabled={!counted}>
        Save cash count
      </button>
    </div>
  )
}
