import { create } from 'zustand';
 
interface SidebarState {
  collapsed: boolean;
  mobileOpen: boolean;
  toggle: () => void;
  setCollapsed: (collapsed: boolean) => void;
  openMobile: () => void;
  closeMobile: () => void;
}
 
export const useSidebarStore = create<SidebarState>((set) => ({
  collapsed: false,
  mobileOpen: false,
  toggle: () => set((s) => ({ collapsed: !s.collapsed })),
  setCollapsed: (collapsed) => set({ collapsed }),
  openMobile: () => set({ mobileOpen: true }),
  closeMobile: () => set({ mobileOpen: false }),
}));