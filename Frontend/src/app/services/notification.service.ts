import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
  action?: {
    label: string;
    handler: () => void;
  };
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor() {}

  // Show success notification
  showSuccess(message: string, duration: number = 3000): void {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'success',
      duration
    });
  }

  // Show error notification
  showError(message: string, duration: number = 5000): void {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'error',
      duration
    });
  }

  // Show warning notification
  showWarning(message: string, duration: number = 4000): void {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'warning',
      duration
    });
  }

  // Show info notification
  showInfo(message: string, duration: number = 3000): void {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'info',
      duration
    });
  }

  // Show notification with action
  showWithAction(
    message: string, 
    type: 'success' | 'error' | 'warning' | 'info',
    actionLabel: string,
    actionHandler: () => void,
    duration: number = 5000
  ): void {
    this.addNotification({
      id: this.generateId(),
      message,
      type,
      duration,
      action: {
        label: actionLabel,
        handler: actionHandler
      }
    });
  }

  // Remove notification
  removeNotification(id: string): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.filter(n => n.id !== id);
    this.notificationsSubject.next(updatedNotifications);
  }

  // Clear all notifications
  clearAll(): void {
    this.notificationsSubject.next([]);
  }

  // Private methods
  private addNotification(notification: Notification): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = [...currentNotifications, notification];
    this.notificationsSubject.next(updatedNotifications);

    // Auto-remove notification after duration
    if (notification.duration && notification.duration > 0) {
      setTimeout(() => {
        this.removeNotification(notification.id);
      }, notification.duration);
    }
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  // Reminder-specific notifications
  showReminderSuccess(action: 'added' | 'removed' | 'updated', time?: string): void {
    let message = '';
    switch (action) {
      case 'added':
        message = time ? `Reminder set for ${time}` : 'Reminder added successfully';
        break;
      case 'removed':
        message = 'Reminder removed';
        break;
      case 'updated':
        message = time ? `Reminder updated to ${time}` : 'Reminder updated successfully';
        break;
    }
    this.showSuccess(message);
  }

  showReminderError(action: 'add' | 'remove' | 'update', error?: any): void {
    let message = '';
    switch (action) {
      case 'add':
        message = 'Failed to add reminder';
        break;
      case 'remove':
        message = 'Failed to remove reminder';
        break;
      case 'update':
        message = 'Failed to update reminder';
        break;
    }

    // Add specific error details if available
    if (error) {
      if (error.status === 401) {
        message += ' - Please log in again';
      } else if (error.status === 400) {
        message += ' - Invalid request';
      } else if (error.status === 0) {
        message += ' - Network error';
      }
    }

    this.showError(message);
  }

  // Authentication-related notifications
  showAuthError(): void {
    this.showError('Please log in to continue', 0); // No auto-dismiss
  }

  showNetworkError(): void {
    this.showError('Network error - Please check your connection');
  }

  // Note-related notifications
  showNoteSuccess(action: 'created' | 'updated' | 'deleted' | 'archived' | 'restored'): void {
    const messages = {
      created: 'Note created',
      updated: 'Note updated',
      deleted: 'Note deleted',
      archived: 'Note archived',
      restored: 'Note restored'
    };
    this.showSuccess(messages[action]);
  }

  showNoteError(action: 'create' | 'update' | 'delete' | 'archive' | 'restore'): void {
    const messages = {
      create: 'Failed to create note',
      update: 'Failed to update note',
      delete: 'Failed to delete note',
      archive: 'Failed to archive note',
      restore: 'Failed to restore note'
    };
    this.showError(messages[action]);
  }
}
