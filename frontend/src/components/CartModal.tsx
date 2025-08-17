// src/components/CartModal.tsx
import React from 'react';
import './CartModal.css';
import { type Course } from '../types/Course';
import type { PaymentRequest, PaymentResult } from '../types/Payment';

interface CartModalProps {
  isOpen: boolean;
  onClose: () => void;
  cartItems: Course[];
  userId: string | null;
  onProcessPayment: (paymentRequest: PaymentRequest) => Promise<PaymentResult>;
  paymentMessage: string | null;
  onClearCart: () => void;
}

const CartModal: React.FC<CartModalProps> = ({ 
  isOpen, 
  onClose, 
  cartItems, 
  userId, 
  onProcessPayment,
  paymentMessage,
  onClearCart
}) => {
  if (!isOpen) {
    return null;
  }

  const totalPrice = cartItems.reduce((total, item) => total + item.Price, 0);

  const handleCheckout = async () => {
    if (!userId) {
      alert('Debes iniciar sesión para procesar el pago.');
      return;
    }
    const paymentRequest: PaymentRequest = {
      userId: userId,
    };
    await onProcessPayment(paymentRequest);
  };
  
  // Si el pago fue exitoso, vacía el carrito después de mostrar el mensaje
  React.useEffect(() => {
    if (paymentMessage && paymentMessage.includes('realizado con éxito')) {
      const timer = setTimeout(() => {
        onClearCart();
      }, 2500);

      return () => clearTimeout(timer);
    }
  }, [paymentMessage, onClearCart]);


  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <button className="modal-close-button" onClick={onClose}>&times;</button>
        <h2>Tu Carrito</h2>
        {cartItems.length === 0 ? (
          <p>Tu carrito está vacío.</p>
        ) : (
          <>
            <div className="cart-items-list">
              {cartItems.map(item => (
                <div key={item.Id} className="cart-item-card">
                  <h3>{item.Name}</h3>
                  <p>Precio: ${item.Price}</p>
                </div>
              ))}
            </div>
            <div className="cart-summary">
              <h3>Total: ${totalPrice.toFixed(2)}</h3>
              <button 
                className="checkout-button" 
                onClick={handleCheckout}
              >
                Proceder al pago
              </button>
            </div>
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
          </>
        )}
      </div>
    </div>
  );
};

export default CartModal;