// src/api/api.ts
import axios from 'axios';
import { type Course } from '../types/Course';

// Clave de suscripción principal que aplica a todos los servicios
const API_SUBSCRIPTION_KEY = 'dc252096cbad43c192a0c399fa333f21'; 

// URL para el servicio de catálogo de cursos
const API_BASE_URL = 'https://miapimanagement1.azure-api.net/coursecatalog'; 
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Ocp-Apim-Subscription-Key': API_SUBSCRIPTION_KEY,
    'Content-Type': 'application/json',
  },
});

export const getCourses = async (): Promise<Course[]> => {
  try {
    const response = await api.get('/courses');
    return response.data;
  } catch (error) {
    console.error('Error al obtener los cursos:', error);
    throw error;
  }
};

// URL para el servicio de carrito
const API_MANAGEMENT_URL = 'https://miapimanagement1.azure-api.net/cart';

export const addCourseToCart = async (userId: string, courseId: string) => {
  const requestOptions: RequestInit = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Ocp-Apim-Subscription-Key': API_SUBSCRIPTION_KEY,
    },
    body: JSON.stringify({ userId, productId: courseId, quantity: 1 }), 
  };

  try {
    const response = await fetch(`${API_MANAGEMENT_URL}/add`, requestOptions);
    if (!response.ok) {
      const error = new Error(`Error: ${response.status} ${response.statusText}`);
      (error as any).status = response.status;
      throw error;
    }
    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Error al añadir el curso al carrito:', error);
    throw error;
  }
};

export const getCartItems = async (userId: string) => {
  const requestOptions: RequestInit = {
    method: 'GET',
    headers: {
      'Ocp-Apim-Subscription-Key': API_SUBSCRIPTION_KEY,
    },
  };

  try {
    const response = await fetch(`${API_MANAGEMENT_URL}/${userId}`, requestOptions);
    if (!response.ok) {
        const error = new Error(`Error: ${response.status} ${response.statusText}`);
        (error as any).status = response.status;
        throw error;
    }
    const result = await response.json();
    console.log("Datos del carrito recibidos:", result);
    return result;
  } catch (error) {
    console.error('Error al obtener el carrito:', error);
    throw error;
  }
};

// URL para el servicio de autenticación
const AUTHENTICATION_API_URL = 'https://miapimanagement1.azure-api.net/authentication';

export const login = async (username: string, password: string) => {
    const requestOptions: RequestInit = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Ocp-Apim-Subscription-Key': API_SUBSCRIPTION_KEY, // ✅ Corregido: usar header en lugar de query parameter
        },
        body: JSON.stringify({ username, password }),
    };

    try {
        // ✅ Corregido: URL sin query parameter
        const response = await fetch(`${AUTHENTICATION_API_URL}/login`, requestOptions);
        if (!response.ok) {
            const error = new Error(`Error: ${response.status} ${response.statusText}`);
            (error as any).status = response.status;
            throw error;
        }
        const result = await response.json();
        return result;
    } catch (error) {
        console.error('Error en el login:', error);
        throw error;
    }
};

export const register = async (username: string, password: string, email: string) => {
    const requestOptions: RequestInit = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Ocp-Apim-Subscription-Key': API_SUBSCRIPTION_KEY, // ✅ Corregido: usar header en lugar de query parameter
        },
        body: JSON.stringify({ username, password, email }),
    };

    try {
        // ✅ Corregido: URL sin query parameter
        const response = await fetch(`${AUTHENTICATION_API_URL}/register`, requestOptions);
        if (!response.ok) {
            const error = new Error(`Error: ${response.status} ${response.statusText}`);
            (error as any).status = response.status;
            throw error;
        }
        const result = await response.json();
        return result;
    } catch (error) {
        console.error('Error en el registro:', error);
        throw error;
    }
};