// src/components/LoginModal.tsx
import React, { useState } from 'react';
import './LoginModal.css';

interface LoginModalProps {
  onClose: () => void;
  onLogin: (username: string, password: string) => Promise<boolean>;
  onSwitchToRegister: () => void; // [NUEVO] Propiedad para cambiar al modal de registro
}

// [CORREGIDO] Añade onSwitchToRegister aquí para que el componente lo reciba
const LoginModal: React.FC<LoginModalProps> = ({ onClose, onLogin, onSwitchToRegister }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    const success = await onLogin(username, password);
    setIsLoading(false);
    if (success) {
      onClose();
    }
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        <h2>Iniciar Sesión</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Usuario</label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Contraseña</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <div className="modal-actions">
            <button type="submit" disabled={isLoading}>
              {isLoading ? 'Iniciando...' : 'Entrar'}
            </button>
            <button type="button" onClick={onClose} disabled={isLoading}>
              Cancelar
            </button>
          </div>
        </form>
        {/* [NUEVO] Botón para cambiar al formulario de registro */}
        <div className="switch-modal">
          <p>¿No tienes una cuenta?</p>
          <button onClick={onSwitchToRegister} className="link-button">Regístrate</button>
        </div>
      </div>
    </div>
  );
};

export default LoginModal;