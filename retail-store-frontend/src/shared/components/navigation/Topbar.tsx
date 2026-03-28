import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@shared/store/auth-store';
import { useSidebarStore } from '@shared/store/sidebar-store';
import { ChevronRight, LogOut, Menu } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
 
export function Topbar() {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();
  const openMobile = useSidebarStore((s) => s.openMobile);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
 
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);
 
  const segments = location.pathname.split('/').filter(Boolean);
  const breadcrumbs = segments.map((seg, i) => ({
    label: t(`nav.${seg}`, seg.charAt(0).toUpperCase() + seg.slice(1)),
    path: '/' + segments.slice(0, i + 1).join('/'),
    isLast: i === segments.length - 1,
  }));
 
  return (
    <header className="mb-6 flex items-center justify-between">
      <div className="flex items-center gap-3">
        {/* Mobile hamburger */}
        <button
          onClick={openMobile}
          className="rounded-lg p-2 text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)] lg:hidden"
        >
          <Menu className="h-5 w-5" />
        </button>
 
        {/* Breadcrumbs — hidden on small screens */}
        <nav className="hidden sm:flex items-center gap-1.5 text-sm">
          <span className="text-[var(--text-muted)]">{t('nav.dashboard')}</span>
          {breadcrumbs.map((bc) => (
            <span key={bc.path} className="flex items-center gap-1.5">
              <ChevronRight className="h-3.5 w-3.5 text-[var(--text-muted)]" />
              {bc.isLast ? (
                <span className="font-medium text-[var(--text-primary)]">{bc.label}</span>
              ) : (
                <button
                  onClick={() => navigate(bc.path)}
                  className="text-[var(--text-muted)] hover:text-[var(--text-primary)]"
                >
                  {bc.label}
                </button>
              )}
            </span>
          ))}
        </nav>
 
        {/* Mobile: show current page title */}
        <span className="text-sm font-medium text-[var(--text-primary)] sm:hidden">
          {breadcrumbs.length > 0 ? breadcrumbs[breadcrumbs.length - 1]?.label : t('nav.dashboard')}
        </span>
      </div>
 
      {/* User menu */}
      <div className="relative" ref={menuRef}>
        <button
          onClick={() => setMenuOpen(!menuOpen)}
          className="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm hover:bg-[var(--bg-tertiary)] transition-colors"
        >
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-600 text-xs font-bold text-white">
            {user?.username?.charAt(0).toUpperCase() ?? 'U'}
          </div>
          <div className="hidden sm:block text-left">
            <p className="font-medium text-[var(--text-primary)]">{user?.username ?? 'User'}</p>
            <p className="text-xs text-[var(--text-muted)]">{user?.email ?? ''}</p>
          </div>
        </button>
 
        {menuOpen && (
          <div className="absolute right-0 top-full z-10 mt-1 w-48 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] py-1 shadow-lg">
            {/* Mobile: show user info */}
            <div className="border-b border-[var(--border-color)] px-4 py-2 sm:hidden">
              <p className="font-medium text-[var(--text-primary)] text-sm">{user?.username}</p>
              <p className="text-xs text-[var(--text-muted)]">{user?.email}</p>
            </div>
            <button
              onClick={() => { logout(); navigate('/login'); setMenuOpen(false); }}
              className="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20"
            >
              <LogOut className="h-4 w-4" />
              {t('nav.logout')}
            </button>
          </div>
        )}
      </div>
    </header>
  );
}