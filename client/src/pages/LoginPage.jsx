import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/client'
import { useAuth } from '../AuthContext'
import NumPad from '../components/NumPad'

export default function LoginPage() {
  const [users, setUsers] = useState([])
  const [selectedUser, setSelectedUser] = useState(null)
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
      .then((res) => setUsers(res.data))
      .catch((err) => {
        if (err.response) {
          setError('API error — is the database up and the connection string set?')
        } else {
          setError('Cannot reach the API — is it running on port 5000?')
        }
      })
  }, [])

  useEffect(() => {
    if (pin.length === 4 && selectedUser) {
      api.post('/auth/login', { userId: selectedUser.id, pin })
        .then((res) => {
          login(res.data)
          navigate('/', { replace: true })
        })
        .catch(() => {
          setError('Wrong PIN, try again')
          setPin('')
        })
    }
  }, [pin, selectedUser]) // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div className="login-screen">
      <h1 className="login-title">Anse Nouveau</h1>
      {!selectedUser ? (
        <div className="login-users">
          <p className="login-hint">Who are you?</p>
          {users.map((u) => (
            <button key={u.id} className="login-user-btn" onClick={() => { setSelectedUser(u); setError('') }}>
              {u.displayName}
            </button>
          ))}
          {error && <p className="error-text">{error}</p>}
        </div>
      ) : (
        <div className="login-pin">
          <button className="login-back" onClick={() => { setSelectedUser(null); setPin('') }}>
            ← {selectedUser.displayName}
          </button>
          <div className="pin-dots">
            {[0, 1, 2, 3].map((i) => (
              <span key={i} className={i < pin.length ? 'pin-dot filled' : 'pin-dot'} />
            ))}
          </div>
          {error && <p className="error-text">{error}</p>}
          <NumPad value={pin} onChange={(v) => { if (v.length <= 4) { setPin(v); setError('') } }} />
        </div>
      )}
    </div>
  )
}
