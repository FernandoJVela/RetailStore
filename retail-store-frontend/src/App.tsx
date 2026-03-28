import { RouterProvider } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { router } from '@app/router';
import { ErrorBoundary } from '@shared/components/feedback/ErrorBoundary';
import { ToastContainer } from '@shared/components/feedback/ToastContainer';
import '@shared/i18n';
 
export default function App() {
  return (
    <ErrorBoundary>
      <AppProviders>
        <RouterProvider router={router} />
        <ToastContainer />
      </AppProviders>
    </ErrorBoundary>
  );
}