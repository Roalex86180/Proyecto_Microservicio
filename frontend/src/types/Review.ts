// src/types/Review.ts

export interface Review {
  id: string;
  courseId: string;
  userId: string;
  rating: number; // Por ejemplo, un número de 1 a 5
  comment: string;
  date: string; // La fecha podría ser un string o un objeto Date
}