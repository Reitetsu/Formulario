import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-3 mt-3">
      <small class="text-secondary">Total de registros: {{ totalCount }}</small>

      <div class="d-flex align-items-center gap-2">
        <button
          type="button"
          class="btn btn-outline-secondary btn-sm"
          [disabled]="pageNumber <= 1"
          (click)="goTo(pageNumber - 1)"
        >
          Anterior
        </button>

        <span class="small text-secondary">Pagina {{ pageNumber }} de {{ totalPages || 1 }}</span>

        <button
          type="button"
          class="btn btn-outline-secondary btn-sm"
          [disabled]="pageNumber >= totalPages"
          (click)="goTo(pageNumber + 1)"
        >
          Siguiente
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent {
  @Input({ required: true }) pageNumber = 1;
  @Input({ required: true }) totalPages = 1;
  @Input({ required: true }) totalCount = 0;
  @Output() readonly pageChange = new EventEmitter<number>();

  protected goTo(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }
}
