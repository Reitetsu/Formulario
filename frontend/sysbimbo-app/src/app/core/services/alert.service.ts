import { Injectable, signal } from '@angular/core';

export interface AlertMessage {
  type: 'success' | 'error';
  text: string;
}

@Injectable({ providedIn: 'root' })
export class AlertService {
  private readonly messageState = signal<AlertMessage | null>(null);
  private pendingTimer: ReturnType<typeof setTimeout> | null = null;
  readonly message = this.messageState.asReadonly();

  success(text: string): void {
    this.scheduleMessage({ type: 'success', text });
  }

  error(text: string): void {
    this.scheduleMessage({ type: 'error', text });
  }

  clear(): void {
    this.scheduleMessage(null);
  }

  private scheduleMessage(message: AlertMessage | null): void {
    if (this.pendingTimer) {
      clearTimeout(this.pendingTimer);
    }

    this.pendingTimer = setTimeout(() => {
      this.messageState.set(message);
      this.pendingTimer = null;
    }, 0);
  }
}
