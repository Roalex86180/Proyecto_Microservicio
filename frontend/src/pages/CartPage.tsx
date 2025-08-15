// src/pages/CartPage.tsx
import React from 'react';
import { type Course } from '../types/Course';
import './CartPage.css';

interface CartPageProps {
  cartItems: Course[];
}

const CartPage: React.FC<CartPageProps> = ({ cartItems }) => {
  const totalPrice = cartItems.reduce((total, item) => total + item.Price, 0);

  return (
    <div className="cart-page-container">
      <h2>Tu Carrito</h2>
      {cartItems.length === 0 ? (
        <p>Tu carrito está vacío.</p>
      ) : (
        <div className="cart-items-list">
          {cartItems.map(item => (
            <div key={item.Id} className="cart-item-card">
              <h3>{item.Name}</h3>
              <p>Precio: ${item.Price}</p>
            </div>
          ))}
          <div className="cart-summary">
            <h3>Total: ${totalPrice.toFixed(2)}</h3>
            <button className="checkout-button">Proceder al pago</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default CartPage;