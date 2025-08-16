// src/pages/CartPage.tsx
import React, { useState, useEffect } from 'react';
import { type Course } from '../types/Course';
import './CartPage.css';
import type { PaymentResult } from '../types/Payment';

interface CartPageProps {
  cartItems: Course[];
  userId: string | null;
  onProcessPayment: (userId: string) => Promise<PaymentResult>;
  paymentMessage: string | null;
  // [NUEVO] Prop opcional para vaciar el carrito desde App
  onClearCart?: () => void;
}

const CartPage: React.FC<CartPageProps> = ({
  cartItems,
  userId,
  onProcessPayment,
  paymentMessage,
  onClearCart
}) => {
  const [loading, setLoading] = useState(false);

  const totalPrice = cartItems.reduce((total, item) => total + item.Price, 0);

  const handleCheckout = async () => {
    if (!userId) {
      alert('Debes iniciar sesión para procesar el pago.');
      return;
    }
    setLoading(true);
    await onProcessPayment(userId);
    setLoading(false);
  };

  // [NUEVO] Si el pago fue exitoso, vacía carrito después de mostrar el mensaje
  useEffect(() => {
    if (paymentMessage && paymentMessage.includes('successfully')) {
      const timer = setTimeout(() => {
        if (onClearCart) {
          onClearCart(); // Vacía carrito desde el padre
        }
      }, 2500); // espera 2.5s para que el mensaje se muestre

      return () => clearTimeout(timer);
    }
  }, [paymentMessage, onClearCart]);

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
            <button
              className="checkout-button"
              onClick={handleCheckout}
              disabled={loading}
            >
              {loading ? 'Procesando...' : 'Proceder al pago'}
            </button>
            {paymentMessage && (
              <p
                style={{
                  color: paymentMessage.includes('Error') ? 'red' : 'green',
                  marginTop: '10px',
                }}
              >
                {paymentMessage}
              </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default CartPage;
