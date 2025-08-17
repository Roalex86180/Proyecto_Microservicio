// src/components/Header.tsx
import { Link } from 'react-router-dom';
import React from 'react';
import './Header.css';
import alainLogo from '../assets/Alain.jpeg';
// [MODIFICACIÓN 1] Ahora usamos FaShoppingCart y FaUserCircle
import { FaShoppingCart, FaUserCircle } from 'react-icons/fa';

interface HeaderProps {
  cartItemCount: number;
  isLoggedIn: boolean;
  userName?: string | null;
  onLogin: () => void;
  onLogout: () => void;
  // [NUEVO] Propiedad para abrir el modal del carrito
  onOpenCartModal: () => void; 
}

const Header: React.FC<HeaderProps> = ({ 
  cartItemCount, 
  isLoggedIn, 
  userName,
  onLogin, 
  onLogout,
  onOpenCartModal
}) => {
  return (
    <header className="header">
      <div className="logo-container">
        <img src={alainLogo} alt="Alain Cloud Academy Logo" className="logo" />
        <h1 className="title">Alain Cloud Academy</h1>
      </div>
      <div className="user-actions">
        {/* [NUEVO] Ícono de carrito para abrir el modal */}
        <div className="cart-icon-container" onClick={onOpenCartModal}>
          <FaShoppingCart color="white" size={24} />
          {cartItemCount > 0 && (
            <span className="cart-badge">{cartItemCount}</span>
          )}
        </div>
        
        <div className="auth-button">
          {/* [MODIFICACIÓN 2] Si está logueado, el ícono de usuario navega a "Mis Cursos" */}
          {isLoggedIn ? (
            <Link to="/my-courses" className="user-icon-link">
              <FaUserCircle color="white" size={24} />
            </Link>
          ) : (
            // [MODIFICACIÓN 3] Si no está logueado, solo se muestra el ícono.
            <FaUserCircle color="white" size={24} />
          )}

          {isLoggedIn ? (
            <div className="user-info">
              <span className="username">¡Hola, {userName}!</span>
              <button onClick={onLogout}>Cerrar Sesión</button>
            </div>
          ) : (
            <button onClick={onLogin}>Iniciar Sesión</button>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;