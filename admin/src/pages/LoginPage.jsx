import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/client'
import { useAuth } from '../AuthContext'

export default function LoginPage() {
  const [users, setUsers] = useState([])
  const [userId, setUserId] = useState('')
  const [pin, setPin] = useState('')
  const [error, setError] = useState('')
  const { user, login } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (user) {
      navigate('/', { replace: true })
    }
  }, [user, navigate])

  useEffect(() => {
    api.get('/auth/users')
      .then((res) => {
        setUsers(res.data)
        const admin = res.data.find((u) => u.role === 'Admin')
        if (admin) {
          setUserId(String(admin.id))
        }
      })
      .catch((err) => {
        if (err.response) {
          setError('API error — is the database up and the connection string set?')
        } else {
          setError('Cannot reach the API — is it running on port 5000?')
        }
      })
  }, [])

  function submit(e) {
    e.preventDefault()
    setError('')
    api.post('/auth/login', { userId: Number(userId), pin })
      .then((res) => {
        // Reports and product/price management are Admin endpoints, so a
        // cashier login would only see errors here.
        if (res.data.role !== 'Admin') {
          setError('This back office needs an Admin account.')
          return
        }
        login(res.data)
        navigate('/', { replace: true })
      })
      .catch(() => {
        setError('Wrong PIN, try again.')
        setPin('')
      })
  }

  return (
    <div className="login-screen">
      <form className="login-card" onSubmit={submit}>
        <h1>Back Office</h1>
        <label>
          Who are you?
          <select value={userId} onChange={(e) => setUserId(e.target.value)}>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.displayName} ({u.role})
              </option>
            ))}
          </select>
        </label>
        <label>
          PIN
          <input
            type="password"
            inputMode="numeric"
            maxLength={4}
            value={pin}
            onChange={(e) => setPin(e.target.value.replace(/\D/g, ''))}
            autoFocus
          />
        </label>
        {error && <p className="error-text">{error}</p>}
        <button type="submit" className="btn-primary" disabled={pin.length !== 4 || !userId}>
          Log in
        </button>
      </form>
    </div>
  )
}
