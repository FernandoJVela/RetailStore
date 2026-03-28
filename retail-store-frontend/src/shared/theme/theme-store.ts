import { create } from 'zustand';
 
type Theme = 'light' | 'dark';
 
interface ThemeState {
  theme: Theme;
  toggle: () => void;
  setTheme: (theme: Theme) => void;
}
 
const getInitialTheme = (): Theme => {
  const saved = localStorage.getItem('theme') as Theme | null;
  if (saved) return saved;
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};
 
export const useThemeStore = create<ThemeState>((set) => ({
  theme: getInitialTheme(),
  toggle: () =>
    set((state) => {
      const next = state.theme === 'light' ? 'dark' : 'light';
      localStorage.setItem('theme', next);
      document.documentElement.classList.toggle('dark', next === 'dark');
      return { theme: next };
    }),
  setTheme: (theme) => {
    localStorage.setItem('theme', theme);
    document.documentElement.classList.toggle('dark', theme === 'dark');
    set({ theme });
  },
}));
 
// Apply theme on load
const initial = getInitialTheme();
document.documentElement.classList.toggle('dark', initial === 'dark');