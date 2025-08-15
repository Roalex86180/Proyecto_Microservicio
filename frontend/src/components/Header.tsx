import { Link } from 'react-router-dom';
import React from 'react';
import './Header.css';
import alainLogo from '../assets/Alain.jpeg';
import { FaShoppingCart, FaUserCircle } from 'react-icons/fa';

interface HeaderProps {
  cartItemCount: number;
}

const Header: React.FC<HeaderProps> = ({ cartItemCount }) => {
  return (
    <header className="header">
      <div className="logo-container">
        <img src={alainLogo} alt="Alain Cloud Academy Logo" className="logo" />
        <h1 className="title">Alain Cloud Academy</h1>
      </div>
      <div className="user-actions">
        {/* [MODIFICADO] Usamos el componente Link para la navegación */}
        <Link to="/cart" className="cart-icon">
          <FaShoppingCart color="white" size={24} />
          {cartItemCount > 0 && (
            <span className="cart-badge">{cartItemCount}</span>
          )}
        </Link>
        <span className="auth-button">
          <FaUserCircle color="white" size={24} />
        </span>
      </div>
    </header>
  );
};

export default Header;