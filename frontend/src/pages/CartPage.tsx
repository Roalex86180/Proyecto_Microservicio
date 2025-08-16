// src/pages/CartPage.tsx
import React, { useState } from 'react';
import { type Course } from '../types/Course';
import './CartPage.css';
import type { PaymentResult } from '../types/Payment';

interface CartPageProps {
  cartItems: Course[];
  // [CAMBIO] Se añade el userId y se actualiza el tipo de retorno de la función
  userId: string | null;
  onProcessPayment: (userId: string) => Promise<PaymentResult>;
}

const CartPage: React.FC<CartPageProps> = ({ cartItems, userId, onProcessPayment }) => {
  const [loading, setLoading] = useState(false);
  const [paymentStatus, setPaymentStatus] = useState<string | null>(null);

  const totalPrice = cartItems.reduce((total, item) => total + item.Price, 0);
  
  const handleCheckout = async () => {
    // [NUEVO] Se valida si el usuario está autenticado
    if (!userId) {
      setPaymentStatus('Debes iniciar sesión para procesar el pago.');
      return;
    }
    
    setLoading(true);
    setPaymentStatus(null);
    try {
      // [CAMBIO] Se pasa el userId a la función de pago
      const result = await onProcessPayment(userId);
      setPaymentStatus(result.message);
    } catch (error) {
      setPaymentStatus('Error al procesar el pago. Por favor, inténtelo de nuevo.');
      console.error('Error al procesar el pago:', error);
    } finally {
      setLoading(false);
    }
  };

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
            {paymentStatus && (
              <p style={{ color: paymentStatus.includes('Error') ? 'red' : 'green', marginTop: '10px' }}>
                {paymentStatus}
              </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default CartPage;