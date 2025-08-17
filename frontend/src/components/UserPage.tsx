// src/pages/UserPage.tsx
import React, { useState, useEffect } from 'react';
import { type Course } from '../types/Course';
import './UserPage.css';
// [MODIFICACIÓN 1] Eliminamos las importaciones no utilizadas
import { getPurchasedCourses } from '../api/api'; 

interface UserPageProps {
  userId: string | null;
  // Las props de pago ya no son necesarias aquí.
}

const UserPage: React.FC<UserPageProps> = ({ userId }) => {
  const [purchasedCourses, setPurchasedCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPurchasedCourses = async () => {
      if (!userId) {
        setError("Debes iniciar sesión para ver tus cursos.");
        setLoading(false);
        return;
      }
      try {
        setLoading(true);
        // [MODIFICACIÓN 2] Llama a la función de la API importada
        const courses = await getPurchasedCourses(userId);
        setPurchasedCourses(courses);
        setError(null);
      } catch (err) {
        console.error('Error al obtener cursos comprados:', err);
        setError("No se pudieron cargar tus cursos. Por favor, inténtalo de nuevo.");
      } finally {
        setLoading(false);
      }
    };

    fetchPurchasedCourses();
  }, [userId]);

  return (
    <div className="user-page-container">
      <h2>Mis Cursos Comprados</h2>
      {loading && <p>Cargando tus cursos...</p>}
      {error && <p className="error-message">{error}</p>}
      {!loading && !error && (
        purchasedCourses.length === 0 ? (
          <p>Aún no has comprado ningún curso.</p>
        ) : (
          <div className="purchased-courses-list">
            {purchasedCourses.map(course => (
              <div key={course.Id} className="course-card">
                <h3>{course.Name}</h3>
                <p>{course.Description}</p>
                <p>Precio: ${course.Price}</p>
              </div>
            ))}
          </div>
        )
      )}
    </div>
  );
};

export default UserPage;