import { useState } from 'react'
import api from '../api/client'

function monthStart() {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`
}

function today() {
  return new Date().toISOString().slice(0, 10)
}

function downloadCsv(filename, headers, rows) {
  const escape = (v) => `"${String(v).replace(/"/g, '""')}"`
  const csv = [headers, ...rows].map((r) => r.map(escape).join(',')).join('\n')
  const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }))
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

export default function MonthEndPage() {
  const [from, setFrom] = useState(monthStart())
  const [to, setTo] = useState(today())
  const [sales, setSales] = useState(null)
  const [profit, setProfit] = useState(null)
  const [stock, setStock] = useState(null)
  const [products, setProducts] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function load() {
    setError('')
    setLoading(true)
    try {
      const [salesRes, profitRes, stockRes, productsRes] = await Promise.all([
        api.get('/report/sales', { params: { from, to } }),
        api.get('/report/profit', { params: { from, to } }),
        api.get('/report/stock', { params: { from, to } }),
        api.get('/product'),
      ])
      setSales(salesRes.data)
      setProfit(profitRes.data)
      setStock(stockRes.data)
      setProducts(productsRes.data)
    } catch (err) {
      setError(err.response?.data?.error || 'Could not load the reports.')
    }
    setLoading(false)
  }

  const summary = sales && {
    salesTotal: sales.filter((s) => s.status === 'Completed').reduce((sum, s) => sum + s.totalAmount, 0),
    refundTotal: sales.filter((s) => s.status === 'Refund').reduce((sum, s) => sum + s.totalAmount, 0),
    netTotal: sales.reduce((sum, s) => sum + s.totalAmount, 0),
    cashTotal: sales.filter((s) => s.paymentMethod === 'Cash').reduce((sum, s) => sum + s.totalAmount, 0),
    cardTotal: sales.filter((s) => s.paymentMethod === 'Card').reduce((sum, s) => sum + s.totalAmount, 0),
    saleCount: sales.filter((s) => s.status === 'Completed').length,
  }
  const profitTotals = profit && {
    revenue: profit.reduce((sum, r) => sum + r.revenue, 0),
    cost: profit.reduce((sum, r) => sum + r.cost, 0),
    profit: profit.reduce((sum, r) => sum + r.profit, 0),
  }
  // Value of what's on the shelves right now, at wholesale cost.
  const stockValue = products && products.reduce((sum, p) => sum + p.stockQty * p.costPrice, 0)

  return (
    <div>
      <div className="toolbar">
        <label className="inline">From <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
        <label className="inline">To <input type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
        <button className="btn-primary" onClick={load} disabled={loading}>
          {loading ? 'Loading…' : 'Run month end'}
        </button>
      </div>
      {error && <p className="error-text">{error}</p>}

      {summary && (
        <>
          <div className="tiles">
            <div className="tile"><span>Sales</span><strong>€{summary.salesTotal.toFixed(2)}</strong></div>
            <div className="tile"><span>Refunds</span><strong>€{summary.refundTotal.toFixed(2)}</strong></div>
            <div className="tile"><span>Net</span><strong>€{summary.netTotal.toFixed(2)}</strong></div>
            <div className="tile"><span>Cash</span><strong>€{summary.cashTotal.toFixed(2)}</strong></div>
            <div className="tile"><span>Card</span><strong>€{summary.cardTotal.toFixed(2)}</strong></div>
            <div className="tile"><span># Sales</span><strong>{summary.saleCount}</strong></div>
            {profitTotals && <div className="tile highlight"><span>Profit</span><strong>€{profitTotals.profit.toFixed(2)}</strong></div>}
            {stockValue != null && <div className="tile"><span>Stock value (cost)</span><strong>€{stockValue.toFixed(2)}</strong></div>}
          </div>

          <section>
            <div className="section-head">
              <h2>Profit per product</h2>
              <button
                className="btn-small"
                onClick={() => downloadCsv(`profit_${from}_${to}.csv`,
                  ['Product', 'Units sold', 'Revenue', 'Cost', 'Profit'],
                  profit.map((r) => [r.productName, r.unitsSold, r.revenue.toFixed(2), r.cost.toFixed(2), r.profit.toFixed(2)]))}
              >
                Download CSV
              </button>
            </div>
            <table className="data-table">
              <thead>
                <tr><th>Product</th><th className="num">Units sold</th><th className="num">Revenue</th><th className="num">Cost</th><th className="num">Profit</th></tr>
              </thead>
              <tbody>
                {profit.map((r) => (
                  <tr key={r.productId}>
                    <td>{r.productName}</td>
                    <td className="num">{r.unitsSold}</td>
                    <td className="num">€{r.revenue.toFixed(2)}</td>
                    <td className="num">€{r.cost.toFixed(2)}</td>
                    <td className={r.profit < 0 ? 'num low' : 'num'}>€{r.profit.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr>
                  <td>Total</td>
                  <td></td>
                  <td className="num">€{profitTotals.revenue.toFixed(2)}</td>
                  <td className="num">€{profitTotals.cost.toFixed(2)}</td>
                  <td className="num">€{profitTotals.profit.toFixed(2)}</td>
                </tr>
              </tfoot>
            </table>
          </section>

          <section>
            <div className="section-head">
              <h2>Stock movement</h2>
              <button
                className="btn-small"
                onClick={() => downloadCsv(`stock_${from}_${to}.csv`,
                  ['Product', 'Opening', 'Sold', 'Delivered', 'Adjusted', 'Closing'],
                  stock.map((r) => [r.productName, r.openingQty, r.soldQty, r.deliveredQty, r.adjustedQty, r.closingQty]))}
              >
                Download CSV
              </button>
            </div>
            <table className="data-table">
              <thead>
                <tr><th>Product</th><th className="num">Opening</th><th className="num">Sold</th><th className="num">Delivered</th><th className="num">Adjusted</th><th className="num">Closing</th></tr>
              </thead>
              <tbody>
                {stock.map((r) => (
                  <tr key={r.productId}>
                    <td>{r.productName}</td>
                    <td className="num">{r.openingQty}</td>
                    <td className="num">{r.soldQty}</td>
                    <td className="num">{r.deliveredQty}</td>
                    <td className={r.adjustedQty < 0 ? 'num low' : 'num'}>{r.adjustedQty}</td>
                    <td className="num">{r.closingQty}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </>
      )}
    </div>
  )
}
