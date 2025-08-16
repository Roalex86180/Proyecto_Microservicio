// src/components/CourseActionModal.tsx
import React, { useState } from 'react';
import { type Course } from '../types/Course';
import type { PaymentResult } from '../types/Payment';
import './CourseActionModal.css';

interface CourseActionModalProps {
  course: Course;
  onClose: () => void;
  onAddToCart: () => Promise<void>;
  userId: string | null;
  onProcessPayment: (userId: string, courseId?: string) => Promise<PaymentResult>;
}

const CourseActionModal: React.FC<CourseActionModalProps> = ({
  course,
  onClose,
  onAddToCart,
  userId,
  onProcessPayment
}) => {
  const [loading, setLoading] = useState(false);
  const [actionStatus, setActionStatus] = useState<string | null>(null);

  const handleAddToCart = async () => {
    if (!userId) {
      alert("Debes iniciar sesión para añadir cursos al carrito.");
      return;
    }
    setLoading(true);
    try {
      await onAddToCart();
      setActionStatus("✅ Su curso fue añadido.");
      setTimeout(() => {
        onClose();
      }, 2000);
    } catch (error) {
      setActionStatus("Error al añadir el curso.");
    } finally {
      setLoading(false);
    }
  };

  const handlePayNow = async () => {
    if (!userId) {
      alert("Debes iniciar sesión para procesar el pago.");
      return;
    }

    setLoading(true);
    setActionStatus(null);
    try {
      console.log(`Procediendo a pagar ${course.Name}...`);
      const result = await onProcessPayment(userId, course.Id);

      if (result.message.includes('successfully')) {
        setActionStatus('✅ Su pago fue exitoso.');
      } else {
        setActionStatus(result.message);
      }

      if (result.message.includes('successfully')) {
        setTimeout(() => {
          onClose();
        }, 3000);
      }
    } catch (error) {
      setActionStatus('Error al procesar el pago. Por favor, inténtelo de nuevo.');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>
          &times;
        </button>
        <h2 className="modal-title">{course.Name}</h2>
        <p className="modal-description">{course.Description}</p>
        <p className="modal-price">Precio: ${course.Price}</p>

        {actionStatus ? (
          <div style={{ marginTop: '20px', color: actionStatus.includes('Error') ? 'red' : 'green' }}>
            {actionStatus}
          </div>
        ) : (
          <div className="modal-buttons">
            <button className="add-to-cart-button" onClick={handleAddToCart} disabled={loading}>
              Añadir al Carro
            </button>
            <button className="pay-now-button" onClick={handlePayNow} disabled={loading}>
              {loading ? 'Procesando...' : 'Pagar Ahora'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default CourseActionModal;
