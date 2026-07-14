// Big-button numeric pad used for PINs, prices and quantities.
export default function NumPad({ value, onChange, allowDecimal = false }) {
  function press(key) {
    if (key === 'C') {
      onChange('')
    } else if (key === '⌫') {
      onChange(value.slice(0, -1))
    } else if (key === '.') {
      if (allowDecimal && !value.includes('.')) {
        onChange(value === '' ? '0.' : value + '.')
      }
    } else {
      onChange(value + key)
    }
  }

  const keys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', allowDecimal ? '.' : 'C', '0', '⌫']
  return (
    <div className="numpad">
      {keys.map((key) => (
        <button key={key} type="button" className="numpad-key" onClick={() => press(key)}>
          {key}
        </button>
      ))}
      {allowDecimal && (
        <button type="button" className="numpad-key numpad-clear" onClick={() => press('C')}>
          C
        </button>
      )}
    </div>
  )
}
