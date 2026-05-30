/** API DTOs — match backend JSON responses exactly. */
 
export interface NotificationDto {
  id: string;
  channel: string;
  category: string;
  subject: string | null;
  status: string;
  priority: string;
  createdAt: string;
  readAt: string | null;
}
 
export interface NotificationDetailDto {
  id: string;
  templateId: string | null;
  recipientId: string | null;
  recipientType: string;
  channel: string;
  category: string;
  subject: string | null;
  body: string;
  status: string;
  priority: string;
  sentAt: string | null;
  deliveredAt: string | null;
  readAt: string | null;
  failedAt: string | null;
  failureReason: string | null;
  retryCount: number;
  referenceType: string | null;
  referenceId: string | null;
  createdAt: string;
}
 
export interface PreferenceDto {
  id: string;
  category: string;
  channel: string;
  isEnabled: boolean;
}
 
export interface UnreadCountDto {
  count: number;
}
 
export interface UpdatePreferenceRequestDto {
  recipientId: string;
  recipientType: string;
  category: string;
  channel: string;
  isEnabled: boolean;
}