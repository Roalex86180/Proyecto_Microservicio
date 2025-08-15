// src/components/CourseActionModal.tsx
import React from 'react';
import { type Course } from '../types/Course';
import './CourseActionModal.css';

interface CourseActionModalProps {
  course: Course;
  onClose: () => void;
  onAddToCart: () => Promise<void>; // La función no recibe parámetros y es asíncrona
}

const CourseActionModal: React.FC<CourseActionModalProps> = ({ course, onClose, onAddToCart }) => {
  const handleAddToCart = async () => {
    await onAddToCart(); // Llamamos a la función asíncrona
    onClose();
  };

  const handlePayNow = () => {
    // Lógica para el pago
    console.log(`Procediendo a pagar ${course.Name}...`);
    onClose();
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
        <div className="modal-buttons">
          <button className="add-to-cart-button" onClick={handleAddToCart}>
            Añadir al Carro
          </button>
          <button className="pay-now-button" onClick={handlePayNow}>
            Pagar Ahora
          </button>
        </div>
      </div>
    </div>
  );
};

export default CourseActionModal;