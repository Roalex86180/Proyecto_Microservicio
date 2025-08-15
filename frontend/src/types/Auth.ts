// src/types/Auth.ts

// Interfaz para los datos que se envían para registrar un nuevo usuario
export interface RegisterPayload {
  username: string;
  email: string;
  password: string;
}

// Interfaz para los datos que se envían para iniciar sesión
export interface LoginPayload {
  email: string;
  password: string;
}

// Interfaz para la respuesta que se espera del servidor después del login
export interface AuthResponse {
  token: string;
  userId: string;
  username: string;
}

// Interfaz para el payload que se usaría para validar un token (si el backend lo requiere)
export interface ValidatePayload {
    token: string;
}

// Interfaz para la respuesta de la validación del token
export interface ValidateResponse {
    isValid: boolean;
    userId?: string; // Opcional, puede devolver el ID del usuario
}

// Interfaz para el payload que se usaría para eliminar un usuario (requiere autenticación)
export interface DeleteUserPayload {
    userId: string;
}