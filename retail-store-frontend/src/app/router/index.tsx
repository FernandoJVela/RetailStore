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
 
// ─── Inventory ──────────────────────────────────────────────
import { InventoryListPage } from '@features/inventory/ui/pages/InventoryListPage';
 
// ─── Providers ──────────────────────────────────────────────
import { ProvidersListPage } from '@features/providers/ui/pages/ProvidersListPage';
 
// ─── Payments ───────────────────────────────────────────────
import { PaymentsListPage } from '@features/payments/ui/pages/PaymentsListPage';
 
// ─── Shipping ───────────────────────────────────────────────
import { ShippingListPage } from '@features/shipping/ui/pages/ShippingListPage';
 
// ─── Orders ───────────────────────────────────────────────
import { OrdersListPage } from '@features/orders/ui/pages/OrdersListPage';
 
// ─── Reports ────────────────────────────────────────────────
import { ReportsPage } from '@features/reports/ui/pages/ReportsPage';
 
// ─── Notifications ──────────────────────────────────────────
import { NotificationsPage } from '@features/notifications/ui/pages/NotificationsPage';
 
// ─── Audit ──────────────────────────────────────────────────
import { AuditPage } from '@features/audit/ui/pages/AuditPage';
 
// ─── Audit ──────────────────────────────────────────────────
import { DashboardPage } from '@features/dashboard/ui/pages/DashboardPage';
 
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
      { index: true, element: <DashboardPage /> },
      { path: 'products', element: <ProductsListPage /> },
      { path: 'orders', element: <OrdersListPage /> },
      { path: 'customers', element: <CustomersListPage /> },
      { path: 'inventory', element: <InventoryListPage /> },
      { path: 'providers', element: <ProvidersListPage /> },
      { path: 'shipping', element: <ShippingListPage /> },
      { path: 'payments', element: <PaymentsListPage /> },
      { path: 'notifications', element: <NotificationsPage /> },
      { path: 'reports', element: <ReportsPage /> },
      { path: 'audit', element: <AuditPage /> },
      { path: 'users', element: <UsersListPage /> },
    ],
  },
 
  // ─── Catch-all 404 ────────────────────────────────────────
  { path: '*', element: <RouteErrorPage /> },
]);