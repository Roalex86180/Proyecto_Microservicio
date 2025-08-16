import { Link } from 'react-router-dom';
import React from 'react';
import './Header.css';
import alainLogo from '../assets/Alain.jpeg';
import { FaShoppingCart, FaUserCircle } from 'react-icons/fa';

// ✅ Agregar userName a la interfaz
interface HeaderProps {
  cartItemCount: number;
  isLoggedIn: boolean;
  userName?: string | null; // ← Agregar esta propiedad
  onLogin: () => void;
  onLogout: () => void;
}

const Header: React.FC<HeaderProps> = ({ 
  cartItemCount, 
  isLoggedIn, 
  userName, // ← Recibir userName
  onLogin, 
  onLogout 
}) => {
  return (
    <header className="header">
      <div className="logo-container">
        <img src={alainLogo} alt="Alain Cloud Academy Logo" className="logo" />
        <h1 className="title">Alain Cloud Academy</h1>
      </div>
      <div className="user-actions">
        <Link to="/cart" className="cart-icon">
          <FaShoppingCart color="white" size={24} />
          {cartItemCount > 0 && (
            <span className="cart-badge">{cartItemCount}</span>
          )}
        </Link>
        <div className="auth-button">
          <FaUserCircle color="white" size={24} />
          {isLoggedIn ? (
            <div className="user-info">
              {/* ✅ Mostrar el nombre de usuario */}
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