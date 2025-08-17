// src/App.tsx
import { useState, useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import Header from './components/Header';
import Footer from './components/Footer';
import UserPage from './components/UserPage';
import Sidebar from './components/Sidebar';
import CartModal from './components/CartModal'; 
import LoginModal from './components/LoginModal';
import RegisterModal from './components/RegisterModal';
import { type Course } from './types/Course';
import { getCartItems, addCourseToCart, login, register, createCart, processPayment } from './api/api';
import './App.css';
import type { PaymentResult, PaymentRequest } from './types/Payment';

interface VideoReview {
  Name: string;
  Url: string;
}

type SidebarItem = 'Microsoft Azure' | 'Testimonios' | 'Amazon Web Services' | 'Google Cloud' | 'Estructura del Proyecto';

function App() {
  const [cartItems, setCartItems] = useState<Course[]>([]);
  const [userId, setUserId] = useState<string | null>(null);
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [userName, setUserName] = useState<string | null>(null);
  const [paymentMessage, setPaymentMessage] = useState<string | null>(null);
  const [isCartModalOpen, setIsCartModalOpen] = useState(false); 
  
  const [isSidebarVisible, setIsSidebarVisible] = useState(true);
  const [selectedSidebarItem, setSelectedSidebarItem] = useState<SidebarItem>('Microsoft Azure');
  const [videos, setVideos] = useState<VideoReview[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const toggleSidebar = () => {
    setIsSidebarVisible(prev => !prev);
  };

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

  useEffect(() => {
    if (selectedSidebarItem === 'Testimonios') {
      const fetchVideos = async () => {
        setLoading(true);
        setError(null);
        try {
          const response = await fetch('https://[tu-function-app].azurewebsites.net/api/ListVideoReviews');
          if (!response.ok) {
            throw new Error(`Error ${response.status}: ${response.statusText}`);
          }
          const data = await response.json();
          setVideos(data);
        } catch (err: any) {
          setError(err.message);
        } finally {
          setLoading(false);
        }
      };
      fetchVideos();
    } else {
      setVideos([]);
    }
  }, [selectedSidebarItem]);

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
  const openCartModal = () => setIsCartModalOpen(true);
  const closeCartModal = () => setIsCartModalOpen(false);
  
  const handleItemClick = (itemName: string) => {
    setSelectedSidebarItem(itemName as SidebarItem);
  };
  
  return (
    <div className="App">
      {/* El header ahora es un elemento aparte */}
      <Header
        cartItemCount={cartItems.length}
        isLoggedIn={!!userId}
        userName={userName}
        onLogin={openLoginModal}
        onLogout={handleLogout}
        onOpenCartModal={openCartModal} 
      />
      {/* [NUEVO] Nuevo contenedor para la barra lateral y el contenido principal */}
      <div className="page-layout-container">
        <Sidebar isSidebarVisible={isSidebarVisible} toggleSidebar={toggleSidebar} onItemClick={handleItemClick} />
        <main className="main-content">
          {selectedSidebarItem === 'Testimonios' ? (
            <div>
              <h1>Testimonios</h1>
              {loading && <p>Cargando videos...</p>}
              {error && <p style={{ color: 'red' }}>Error: {error}</p>}
              {!loading && !error && videos.length > 0 && (
                <div className="video-list">
                  {videos.map((video, index) => (
                    <div key={index}>
                      <h3>{video.Name}</h3>
                      <video src={video.Url} controls style={{ maxWidth: '100%', height: 'auto' }} />
                    </div>
                  ))}
                </div>
              )}
              {!loading && !error && videos.length === 0 && <p>No se encontraron videos de testimonios.</p>}
            </div>
          ) : (
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
          )}
        </main>
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