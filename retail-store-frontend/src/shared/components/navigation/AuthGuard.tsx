import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@shared/store/auth-store';
import type { ReactNode } from 'react';
 
interface AuthGuardProps {
  children: ReactNode;
  requiredPermission?: string;
}
 
export function AuthGuard({ children, requiredPermission }: AuthGuardProps) {
  const { isAuthenticated, hasPermission } = useAuthStore();
  const location = useLocation();
 
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
 
  if (requiredPermission && !hasPermission(requiredPermission)) {
    return <Navigate to="/" replace />;
  }
 
  return <>{children}</>;
}