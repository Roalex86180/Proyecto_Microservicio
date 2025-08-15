// src/types/Cart.ts

import { type Course } from './Course';

export interface CartItem {
  course: Course; // Un item del carrito contiene un curso completo
  quantity: number;
}

export interface Cart {
  id: string; // Si el carrito tiene un ID
  items: CartItem[];
  totalPrice: number;
}