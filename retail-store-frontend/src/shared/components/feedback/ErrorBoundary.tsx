import { Component, type ReactNode } from 'react';
import { AlertTriangle, RotateCcw, Home } from 'lucide-react';
 
interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}
 
interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}
 
/**
 * Global Error Boundary — catches unhandled React rendering errors.
 * Wrap around <RouterProvider> or individual route sections.
 *
 * Location: src/shared/components/feedback/ErrorBoundary.tsx
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }
 
  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }
 
  componentDidCatch(error: Error, info: React.ErrorInfo) {
    console.error('[ErrorBoundary] Caught error:', error, info.componentStack);
    // TODO: Send to error tracking service (Sentry, etc.)
  }
 
  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };
 
  render() {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback;
      return <ErrorFallback error={this.state.error} onReset={this.handleReset} />;
    }
    return this.props.children;
  }
}
 
/**
 * Styled error fallback — matches the RetailStore design system.
 * Also used as React Router's errorElement.
 */
export function ErrorFallback({
  error,
  onReset,
}: {
  error?: Error | null;
  onReset?: () => void;
}) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--bg-primary)] px-6">
      <div className="w-full max-w-lg text-center">
        {/* Icon */}
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-2xl bg-red-100 dark:bg-red-500/15">
          <AlertTriangle className="h-10 w-10 text-red-600 dark:text-red-400" />
        </div>
 
        {/* Title */}
        <h1 className="text-2xl font-bold text-[var(--text-primary)]">
          Something went wrong
        </h1>
        <p className="mt-2 text-[var(--text-secondary)]">
          An unexpected error occurred. Our team has been notified.
        </p>
 
        {/* Error details (dev only) */}
        {error && import.meta.env.DEV && (
          <div className="mt-6 rounded-lg border border-red-200 bg-red-50 p-4 text-left dark:border-red-800 dark:bg-red-900/20">
            <p className="text-sm font-mono font-medium text-red-800 dark:text-red-400">
              {error.name}: {error.message}
            </p>
            {error.stack && (
              <pre className="mt-2 max-h-40 overflow-auto text-xs text-red-600 dark:text-red-500">
                {error.stack.split('\n').slice(1, 6).join('\n')}
              </pre>
            )}
          </div>
        )}
 
        {/* Actions */}
        <div className="mt-8 flex items-center justify-center gap-3">
          {onReset && (
            <button
              onClick={onReset}
              className="inline-flex items-center gap-2 rounded-lg bg-primary-600 px-5 py-2.5 text-sm font-medium text-white hover:bg-primary-700 transition-colors"
            >
              <RotateCcw className="h-4 w-4" />
              Try Again
            </button>
          )}
          <a
            href="/"
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] px-5 py-2.5 text-sm font-medium text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)] transition-colors"
          >
            <Home className="h-4 w-4" />
            Go Home
          </a>
        </div>
      </div>
    </div>
  );
}
 
/**
 * Route-level error page — used as React Router's errorElement.
 * Shows a 404-style page for unmatched routes or route-level errors.
 */
export function RouteErrorPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--bg-primary)] px-6">
      <div className="w-full max-w-lg text-center">
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-2xl bg-amber-100 dark:bg-amber-500/15">
          <AlertTriangle className="h-10 w-10 text-amber-600 dark:text-amber-400" />
        </div>
        <h1 className="text-5xl font-bold text-[var(--text-primary)]">404</h1>
        <p className="mt-3 text-lg text-[var(--text-secondary)]">
          This page doesn't exist or you don't have access.
        </p>
        <a
          href="/"
          className="mt-8 inline-flex items-center gap-2 rounded-lg bg-primary-600 px-6 py-3 text-sm font-medium text-white hover:bg-primary-700 transition-colors"
        >
          <Home className="h-4 w-4" />
          Back to Dashboard
        </a>
      </div>
    </div>
  );
}