// src/types/Payment.ts

// [NUEVO] Este es el tipo de datos que se envía al backend para procesar el pago.
export interface PaymentRequest {
  userId: string;
  courseId?: string;
  productName?: string;
  price?: number;
}

// Este es el tipo de dato que se recibe como respuesta del backend.
export interface PaymentResult {
  message: string;
}