// src/App.tsx
import { useState, useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import Header from './components/Header';
import Footer from './components/Footer';
import CartPage from './pages/CartPage'; 
import LoginModal from './components/LoginModal';
import RegisterModal from './components/RegisterModal'; 
import { type Course } from './types/Course'; 
import { getCartItems, addCourseToCart, login, register } from './api/api'; 
import './App.css';

function App() {
  const [cartItems, setCartItems] = useState<Course[]>([]);
  const [userId, setUserId] = useState<string | null>(null);
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [userName, setUserName] = useState<string | null>(null); // [NUEVO] Estado para el nombre de usuario

  useEffect(() => {
    const loadCart = async (currentUserId: string) => {
      try {
        const items = await getCartItems(currentUserId);
        setCartItems(items);
      } catch (error) {
        console.error("No se pudo cargar el carrito del usuario:", error);
      }
    };
    
    if (userId) {
      loadCart(userId);
    } else {
      setCartItems([]);
    }
  }, [userId]);

  const handleItemAdded = async (course: Course) => {
    if (!userId) {
      alert("Debes iniciar sesión para añadir cursos al carrito.");
      return;
    }

    try {
      await addCourseToCart(userId, course.Id);
      setCartItems(prevItems => [...prevItems, course]);
    } catch (error) {
      console.error('Error al añadir el curso al carrito:', error);
    }
  };
  
  const handleLogin = async (username: string, password: string) => {
    try {
      const result = await login(username, password);
      console.log('Resultado del login:', result); // Para depuración
      
      if (result && result.token) { // ✅ Corregido: verificar token en lugar de userId
        // Como el login solo devuelve token, usar username como identificador temporal
        setUserId(username); // Temporal: usar username como userId
        setUserName(username); 
        alert("¡Has iniciado sesión con éxito!");
        closeLoginModal(); // Cierra el modal de login en caso de éxito
        return true;
      }
      return false;
    } catch (error) {
      console.error("Error en el login:", error);
      alert("Error en el login. Por favor, revisa tus credenciales.");
      return false;
    }
  };

  const handleRegister = async (username: string, password: string, email: string) => {
    try {
        const result = await register(username, password, email);
        console.log('Resultado del registro:', result); // Para depuración
        
        if (result && result.userId) { // ✅ Esto debería funcionar según la respuesta de tu API
            setUserId(result.userId);
            setUserName(username);
            alert("¡Registro exitoso! Ya has iniciado sesión.");
            closeRegisterModal(); // Cierra el modal de registro en caso de éxito
            return true;
        }
        return false;
    } catch (error) {
        console.error("Error en el registro:", error);
        alert("Error en el registro. Por favor, revisa los datos o intenta con otro usuario.");
        return false;
    }
  };
  
  const handleLogout = () => {
    setUserId(null);
    setUserName(null); 
    console.log("Usuario ha cerrado sesión.");
  };

  const openLoginModal = () => setIsLoginModalOpen(true);
  const closeLoginModal = () => setIsLoginModalOpen(false);

  const openRegisterModal = () => {
    closeLoginModal();
    setIsRegisterModalOpen(true);
  };
  const closeRegisterModal = () => setIsRegisterModalOpen(false);


  return (
    <div className="App">
      <Header 
        cartItemCount={cartItems.length}
        isLoggedIn={!!userId}
        userName={userName}
        onLogin={openLoginModal}
        onLogout={handleLogout}
      />
      
      <div className="main-content">
        <Routes>
          <Route path="/" element={<HomePage onItemAddedToCart={handleItemAdded} userId={userId} />} />
          <Route path="/cart" element={<CartPage cartItems={cartItems} />} />
        </Routes>
      </div>
      <Footer />

      {isLoginModalOpen && (
        <LoginModal 
          onClose={closeLoginModal} 
          onLogin={handleLogin}
          onSwitchToRegister={openRegisterModal}
        />
      )}

      {isRegisterModalOpen && (
        <RegisterModal 
          onClose={closeRegisterModal} 
          onRegister={handleRegister}
          onSwitchToLogin={openLoginModal}
        />
      )}
    </div>
  );
}

export default App;