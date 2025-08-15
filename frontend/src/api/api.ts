// src/api/api.ts
import axios from 'axios';
import { type Course } from '../types/Course';

// Se cambió la URL base para que incluya la ruta de la API
const API_BASE_URL = 'https://miapimanagement1.azure-api.net/coursecatalog'; 
const SUBSCRIPTION_KEY = '5a59f4f476264197908619c54d991a2e'; 

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Ocp-Apim-Subscription-Key': SUBSCRIPTION_KEY,
    'Content-Type': 'application/json',
  },
});

export const getCourses = async (): Promise<Course[]> => {
  try {
    // La ruta de la operación es solo '/courses'
    const response = await api.get('/courses');
    return response.data;
  } catch (error) {
    console.error('Error al obtener los cursos:', error);
    throw error;
  }
};

const API_MANAGEMENT_URL = 'https://miapimanagement1.azure-api.net/cart';
const API_SUBSCRIPTION_KEY = '5a59f4f476264197908619c54d991a2e';

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
      // [MODIFICADO] Lanza un error personalizado con el status y el texto de la respuesta
      const error = new Error(`Error: ${response.status} ${response.statusText}`);
      (error as any).status = response.status;
      throw error;
    }

    const result = await response.json();
    return result;

  } catch (error) {
    console.error('Error al añadir el curso al carrito:', error);
    throw error; // Propaga el error para que HomePage pueda manejarlo
  }
};