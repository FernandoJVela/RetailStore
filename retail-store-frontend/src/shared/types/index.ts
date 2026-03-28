/** Paginated response wrapper (for future use) */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
 
/** Base query params for list endpoints */
export interface ListQueryParams {
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
 
/** Result wrapper for mutations */
export interface MutationResult {
  success: boolean;
  message: string;
}
 
/** Toast notification type */
export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  description?: string;
}