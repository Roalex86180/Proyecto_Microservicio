// src/App.tsx
import { useState } from 'react';
import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import Header from './components/Header';
import Footer from './components/Footer';
import CartPage from './pages/CartPage'; // [NUEVO] Importa la nueva página
import { type Course } from './types/Course'; // [NUEVO] Importa el tipo Course
import './App.css';

function App() {
  // [MODIFICADO] Ahora el estado del carrito es un array de cursos
  const [cartItems, setCartItems] = useState<Course[]>([]);

  // [MODIFICADO] Esta función ahora recibe el curso completo y lo añade al estado
  const handleItemAdded = (course: Course) => {
    // Para evitar duplicados en el estado, comprobamos si el curso ya está en el carrito
    const isAlreadyInCart = cartItems.some(item => item.Id === course.Id);
    if (!isAlreadyInCart) {
      setCartItems(prevItems => [...prevItems, course]);
    }
  };

  return (
    <div className="App">
      {/* [MODIFICADO] Pasa el número de ítems (cartItems.length) al Header */}
      <Header cartItemCount={cartItems.length} />
      
      <div className="main-content">
        <Routes>
          {/* [MODIFICADO] Pasa la función de manejo al HomePage */}
          <Route path="/" element={<HomePage onItemAddedToCart={handleItemAdded} />} />
          
          {/* [NUEVO] Ruta para la página del carrito, le pasamos los ítems del carrito */}
          <Route path="/cart" element={<CartPage cartItems={cartItems} />} />
        </Routes>
      </div>
      <Footer />
    </div>
  );
}

export default App;