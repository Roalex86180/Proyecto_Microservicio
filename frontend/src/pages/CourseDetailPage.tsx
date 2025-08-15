// src/pages/CourseDetailPage.tsx
import React from 'react';
import { useParams } from 'react-router-dom';

const CourseDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <div>
      <h1>Detalle del Curso {id}</h1>
      {/* Aquí irán la lógica y los componentes para mostrar los detalles del curso */}
    </div>
  );
};

export default CourseDetailPage;