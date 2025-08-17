// src/App.tsx
import { useState, useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import Header from './components/Header';
import Footer from './components/Footer';
import UserPage from './components/UserPage';
// [NUEVO] Importamos el nuevo componente del modal del carrito
import CartModal from './components/CartModal'; 
import LoginModal from './components/LoginModal';
import RegisterModal from './components/RegisterModal';
import { type Course } from './types/Course';
import { getCartItems, addCourseToCart, login, register, createCart, processPayment } from './api/api';
import './App.css';
import type { PaymentResult, PaymentRequest } from './types/Payment';

function App() {
  const [cartItems, setCartItems] = useState<Course[]>([]);
  const [userId, setUserId] = useState<string | null>(null);
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [userName, setUserName] = useState<string | null>(null);
  const [paymentMessage, setPaymentMessage] = useState<string | null>(null);
  // [NUEVO] Estado para controlar la visibilidad del modal del carrito
  const [isCartModalOpen, setIsCartModalOpen] = useState(false); 

  useEffect(() => {
    const loadCart = async (currentUserId: string) => {
      try {
        const items = await getCartItems(currentUserId);
        setCartItems(items);
      } catch (error) {
        console.error("No se pudo cargar el carrito del usuario:", error);
        if (error instanceof Error && error.message.includes('404')) {
          console.log("El carrito no existe, intentando crear uno nuevo...");
          try {
            await createCart(currentUserId);
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

  const clearCart = () => setCartItems([]);

  const handlePayment = async (paymentRequest: PaymentRequest): Promise<PaymentResult> => {
    if (!paymentRequest.userId) {
      alert("Debes iniciar sesión para procesar el pago.");
      return { message: "Error: No se ha iniciado sesión." };
    }
    try {
      const result = await processPayment(paymentRequest); 
      const msg = String(result?.message ?? "");
      console.log('Pago procesado:', msg);

      setPaymentMessage(msg);

      setTimeout(() => {
        setPaymentMessage(null);
      }, 3000);

      return result;
    } catch (error) {
      console.error('Error al procesar el pago:', error);
      setPaymentMessage("Error al procesar el pago. Por favor, inténtelo de nuevo.");
      return { message: "Error al procesar el pago. Por favor, inténtelo de nuevo." };
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
    setCartItems([]);
    console.log("Usuario ha cerrado sesión.");
  };

  const openLoginModal = () => setIsLoginModalOpen(true);
  const closeLoginModal = () => setIsLoginModalOpen(false);
  const openRegisterModal = () => {
    closeLoginModal();
    setIsRegisterModalOpen(true);
  };
  const closeRegisterModal = () => setIsRegisterModalOpen(false);
  // [NUEVO] Funciones para abrir y cerrar el modal del carrito
  const openCartModal = () => setIsCartModalOpen(true);
  const closeCartModal = () => setIsCartModalOpen(false);

  return (
    <div className="App">
      <Header
        cartItemCount={cartItems.length}
        isLoggedIn={!!userId}
        userName={userName}
        onLogin={openLoginModal}
        onLogout={handleLogout}
        // [NUEVO] Pasamos la función para abrir el modal del carrito
        onOpenCartModal={openCartModal} 
      />
      <div className="main-content">
        <Routes>
          <Route
            path="/"
            element={<HomePage onItemAddedToCart={handleItemAdded} userId={userId} onProcessPayment={handlePayment} />}
          />
          <Route
            path="/my-courses"
            element={
              <UserPage
                userId={userId}
              />
            }
          />
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
      {/* [NUEVO] Renderizamos el modal del carrito condicionalmente */}
      <CartModal
        isOpen={isCartModalOpen}
        onClose={closeCartModal}
        cartItems={cartItems}
        userId={userId}
        onProcessPayment={handlePayment}
        paymentMessage={paymentMessage}
        onClearCart={clearCart}
      />
    </div>
  );
}

export default App;