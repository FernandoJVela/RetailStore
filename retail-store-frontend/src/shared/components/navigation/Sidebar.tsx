import { NavLink, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@shared/store/auth-store';
import { useThemeStore } from '@shared/theme/theme-store';
import { useSidebarStore } from '@shared/store/sidebar-store';
import { cn } from '@shared/lib/utils';
import {
  LayoutDashboard, Package, ShoppingCart, Users, Warehouse,
  Truck, CreditCard, Bell, BarChart3, Shield, Building2,
  LogOut, Sun, Moon, Languages, ChevronLeft, Store, X,
} from 'lucide-react';
 
const NAV_ITEMS = [
  { key: 'dashboard', path: '/', icon: LayoutDashboard },
  { key: 'products', path: '/products', icon: Package },
  { key: 'orders', path: '/orders', icon: ShoppingCart },
  { key: 'customers', path: '/customers', icon: Users },
  { key: 'inventory', path: '/inventory', icon: Warehouse },
  { key: 'providers', path: '/providers', icon: Building2 },
  { key: 'shipping', path: '/shipping', icon: Truck },
  { key: 'payments', path: '/payments', icon: CreditCard },
  { key: 'notifications', path: '/notifications', icon: Bell },
  { key: 'reports', path: '/reports', icon: BarChart3 },
  { key: 'audit', path: '/audit', icon: Shield },
  { key: 'users', path: '/users', icon: Users },
];
 
export function Sidebar() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const logout = useAuthStore((s) => s.logout);
  const { theme, toggle: toggleTheme } = useThemeStore();
  const { collapsed, mobileOpen, toggle, closeMobile } = useSidebarStore();
 
  const toggleLanguage = () => {
    const next = i18n.language === 'en' ? 'es' : 'en';
    i18n.changeLanguage(next);
    localStorage.setItem('language', next);
  };
 
  const handleLogout = () => {
    logout();
    navigate('/login');
  };
 
  const handleNavClick = () => {
    // Close mobile sidebar on navigation
    closeMobile();
  };
 
  const sidebarContent = (
    <>
      {/* Logo */}
      <div className="flex h-16 items-center justify-between border-b border-white/10 px-4">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary-600">
            <Store className="h-5 w-5 text-white" />
          </div>
          {!collapsed && (
            <span className="text-lg font-bold tracking-tight text-white">
              {t('common.appName')}
            </span>
          )}
        </div>
        {/* Mobile close button */}
        <button
          onClick={closeMobile}
          className="rounded-lg p-1.5 text-white/60 hover:bg-white/10 lg:hidden"
        >
          <X className="h-5 w-5" />
        </button>
      </div>
 
      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto px-3 py-4 space-y-1">
        {NAV_ITEMS.map(({ key, path, icon: Icon }) => (
          <NavLink
            key={key}
            to={path}
            onClick={handleNavClick}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-primary-600/20 text-[var(--sidebar-active)]'
                  : 'text-[var(--sidebar-text)] hover:bg-white/5 hover:text-white'
              )
            }
          >
            <Icon className="h-5 w-5 shrink-0" />
            {!collapsed && <span>{t(`nav.${key}`)}</span>}
          </NavLink>
        ))}
      </nav>
 
      {/* Bottom controls */}
      <div className="border-t border-white/10 px-3 py-3 space-y-1">
        <button
          onClick={toggleTheme}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-[var(--sidebar-text)] hover:bg-white/5 hover:text-white transition-colors"
        >
          {theme === 'dark' ? <Sun className="h-5 w-5 shrink-0" /> : <Moon className="h-5 w-5 shrink-0" />}
          {!collapsed && <span>{theme === 'dark' ? t('common.lightMode') : t('common.darkMode')}</span>}
        </button>
 
        <button
          onClick={toggleLanguage}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-[var(--sidebar-text)] hover:bg-white/5 hover:text-white transition-colors"
        >
          <Languages className="h-5 w-5 shrink-0" />
          {!collapsed && <span>{i18n.language === 'en' ? 'Español' : 'English'}</span>}
        </button>
 
        {/* Collapse toggle — desktop only */}
        <button
          onClick={toggle}
          className="hidden lg:flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-[var(--sidebar-text)] hover:bg-white/5 hover:text-white transition-colors"
        >
          <ChevronLeft className={cn('h-5 w-5 shrink-0 transition-transform', collapsed && 'rotate-180')} />
          {!collapsed && <span>Collapse</span>}
        </button>
 
        <button
          onClick={handleLogout}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-red-400 hover:bg-red-500/10 hover:text-red-300 transition-colors"
        >
          <LogOut className="h-5 w-5 shrink-0" />
          {!collapsed && <span>{t('nav.logout')}</span>}
        </button>
      </div>
    </>
  );
 
  return (
    <>
      {/* ─── Mobile overlay ──────────────────────────────────── */}
      {mobileOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm lg:hidden"
          onClick={closeMobile}
        />
      )}
 
      {/* ─── Mobile sidebar (slides in from left) ────────────── */}
      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 flex w-[260px] flex-col bg-[var(--sidebar-bg)] transition-transform duration-300 lg:hidden',
          mobileOpen ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        {sidebarContent}
      </aside>
 
      {/* ─── Desktop sidebar (always visible, collapsible) ───── */}
      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-30 hidden lg:flex flex-col bg-[var(--sidebar-bg)] transition-all duration-300',
          collapsed ? 'w-[68px]' : 'w-[260px]'
        )}
      >
        {sidebarContent}
      </aside>
    </>
  );
}