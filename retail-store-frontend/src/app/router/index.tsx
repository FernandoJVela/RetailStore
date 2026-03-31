import { createBrowserRouter } from 'react-router-dom';
import { MainLayout } from '@app/layouts/MainLayout';
import { AuthGuard } from '@shared/components/navigation/AuthGuard';
import { RouteErrorPage } from '@shared/components/feedback/ErrorBoundary';
 
// ─── Users / Auth ───────────────────────────────────────────
import { LoginPage } from '@features/users/ui/pages/LoginPage';
import { RegisterPage } from '@features/users/ui/pages/RegisterPage';
import { UsersListPage } from '@features/users/ui/pages/UsersListPage';
 
// ─── Customers ───────────────────────────────────────────────
import { CustomersListPage } from '@features/customers/ui/pages/CustomersListPage';
 
// ─── Products ───────────────────────────────────────────────
import { ProductsListPage } from '@features/products/ui/pages/ProductsListPage';
 
// ─── Placeholder pages (to be built per module later) ───────
function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-24">
      <h1 className="text-2xl font-bold text-[var(--text-primary)]">{title}</h1>
      <p className="mt-2 text-[var(--text-secondary)]">This module will be built next.</p>
    </div>
  );
}
 
export const router = createBrowserRouter([
  // ─── Public routes (no auth) ──────────────────────────────
  { path: '/login', element: <LoginPage />, errorElement: <RouteErrorPage /> },
  { path: '/register', element: <RegisterPage />, errorElement: <RouteErrorPage /> },
 
  // ─── Protected routes (auth required) ─────────────────────
  {
    element: (
      <AuthGuard>
        <MainLayout />
      </AuthGuard>
    ),
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <PlaceholderPage title="Dashboard" /> },
      { path: 'products', element: <ProductsListPage /> },
      { path: 'orders', element: <PlaceholderPage title="Orders" /> },
      { path: 'customers', element: <CustomersListPage /> },
      { path: 'inventory', element: <PlaceholderPage title="Inventory" /> },
      { path: 'providers', element: <PlaceholderPage title="Providers" /> },
      { path: 'shipping', element: <PlaceholderPage title="Shipping" /> },
      { path: 'payments', element: <PlaceholderPage title="Payments" /> },
      { path: 'notifications', element: <PlaceholderPage title="Notifications" /> },
      { path: 'reports', element: <PlaceholderPage title="Reports" /> },
      { path: 'audit', element: <PlaceholderPage title="Audit Trail" /> },
      { path: 'users', element: <UsersListPage /> },
    ],
  },
 
  // ─── Catch-all 404 ────────────────────────────────────────
  { path: '*', element: <RouteErrorPage /> },
]);