import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
 
/** Merge Tailwind classes safely */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
 
/** Format date for display */
export function formatDate(date: string | Date, locale = 'en-US'): string {
  return new Date(date).toLocaleDateString(locale, {
    year: 'numeric', month: 'short', day: 'numeric',
  });
}
 
/** Format date with time */
export function formatDateTime(date: string | Date, locale = 'en-US'): string {
  return new Date(date).toLocaleString(locale, {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}
 
/** Truncate text */
export function truncate(text: string, maxLength: number): string {
  return text.length > maxLength ? `${text.slice(0, maxLength)}...` : text;
}
 
/** Sleep utility */
export const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));