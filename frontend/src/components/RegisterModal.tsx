// src/components/RegisterModal.tsx
import React, { useState } from 'react';
import './LoginModal.css'; // Reutiliza el CSS del modal de login

interface RegisterModalProps {
  onClose: () => void;
  onRegister: (username: string, password: string, email: string) => Promise<boolean>;
  onSwitchToLogin: () => void; // [CORREGIDO] Agrega la nueva propiedad
}

const RegisterModal: React.FC<RegisterModalProps> = ({ onClose, onRegister, onSwitchToLogin }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    const success = await onRegister(username, password, email);
    setIsLoading(false);
    if (success) {
      onClose();
    }
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        <h2>Registrarse</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="reg-username">Usuario</label>
            <input
              type="text"
              id="reg-username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="reg-email">Correo Electrónico</label>
            <input
              type="email"
              id="reg-email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="reg-password">Contraseña</label>
            <input
              type="password"
              id="reg-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <div className="modal-actions">
            <button type="submit" disabled={isLoading}>
              {isLoading ? 'Registrando...' : 'Registrar'}
            </button>
            <button type="button" onClick={onClose} disabled={isLoading}>
              Cancelar
            </button>
          </div>
        </form>
        {/* [NUEVO] Botón para cambiar al formulario de login */}
        <div className="switch-modal">
          <p>¿Ya tienes una cuenta?</p>
          <button onClick={onSwitchToLogin} className="link-button">Iniciar Sesión</button>
        </div>
      </div>
    </div>
  );
};

export default RegisterModal;