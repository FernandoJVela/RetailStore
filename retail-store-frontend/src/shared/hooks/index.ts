import { useState, useEffect, useRef, type RefObject } from 'react';
 
/** Debounce a value (for search inputs) */
export function useDebounce<T>(value: T, delay = 300): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);
  return debounced;
}
 
/** Detect clicks outside a ref (for dropdowns) */
export function useClickOutside<T extends HTMLElement>(
  callback: () => void
): RefObject<T | null> {
  const ref = useRef<T>(null);
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) callback();
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [callback]);
  return ref;
}
 
/** Document title */
export function useDocumentTitle(title: string) {
  useEffect(() => {
    const prev = document.title;
    document.title = `${title} | RetailStore`;
    return () => { document.title = prev; };
  }, [title]);
}