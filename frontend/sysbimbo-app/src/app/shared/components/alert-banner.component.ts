import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AlertService } from '../../core/services/alert.service';

@Component({
  selector: 'app-alert-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (alertService.message(); as message) {
      <div
        class="alert d-flex align-items-center justify-content-between mb-4"
        [class.alert-success]="message.type === 'success'"
        [class.alert-danger]="message.type === 'error'"
        role="alert"
      >
        <span>{{ message.text }}</span>
        <button type="button" class="btn-close" aria-label="Close" (click)="alertService.clear()"></button>
      </div>
    }
  `
})
export class AlertBannerComponent {
  protected readonly alertService = inject(AlertService);
}
