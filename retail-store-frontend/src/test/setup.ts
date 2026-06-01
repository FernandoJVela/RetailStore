import '@testing-library/jest-dom/vitest';
import '@shared/i18n'; // initialises i18next (sets up 'en' as default; uses jsdom localStorage)

// Suppress React 19 act() warnings in tests — they fire when state updates
// occur outside of act(), which is expected when testing async hooks.
const originalError = console.error.bind(console.error);
beforeAll(() => {
  console.error = (...args: unknown[]) => {
    if (typeof args[0] === 'string' && args[0].includes('act(')) return;
    originalError(...args);
  };
});
afterAll(() => {
  console.error = originalError;
});
