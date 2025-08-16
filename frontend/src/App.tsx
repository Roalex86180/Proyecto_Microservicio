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
// [CAMBIO] Importamos la nueva función 'createCart'
import { getCartItems, addCourseToCart, login, register, createCart } from './api/api'; 
import './App.css';

function App() {
  const [cartItems, setCartItems] = useState<Course[]>([]);
  const [userId, setUserId] = useState<string | null>(null);
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [userName, setUserName] = useState<string | null>(null);

  useEffect(() => {
    const loadCart = async (currentUserId: string) => {
      try {
        // [CAMBIO] Intenta cargar el carrito
        const items = await getCartItems(currentUserId);
        setCartItems(items);
      } catch (error) {
        // [CAMBIO] Si falla, verifica si es un error 404
        console.error("No se pudo cargar el carrito del usuario:", error);
        
        // Verifica si el error es de tipo '404 Resource Not Found'
        if (error instanceof Error && error.message.includes('404')) {
          console.log("El carrito no existe, intentando crear uno nuevo...");
          try {
            // Llama a la nueva función para crear el carrito.
            // Asegúrate de que esta función envíe un POST a la API.
            await createCart(currentUserId);
            // Si el carrito se crea con éxito, actualiza el estado a un carrito vacío
            setCartItems([]);
            console.log("Nuevo carrito creado con éxito.");
          } catch (createError) {
            console.error("Error al crear el carrito:", createError);
          }
        }
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
      console.log('Resultado del login:', result);
      
      if (result && result.token) {
        setUserId(username);
        setUserName(username); 
        alert("¡Has iniciado sesión con éxito!");
        closeLoginModal();
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
        console.log('Resultado del registro:', result);
        
        if (result && result.userId) {
          setUserId(result.userId);
          setUserName(username);
          alert("¡Registro exitoso! Ya has iniciado sesión.");
          closeRegisterModal();
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