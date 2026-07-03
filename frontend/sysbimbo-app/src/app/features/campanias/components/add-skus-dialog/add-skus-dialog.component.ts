import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime } from 'rxjs';
import { AddCampaniaSkusPayload } from '../../../../core/models/campania-operacion.model';
import { PagedResult } from '../../../../core/models/paged-result.model';
import { SkuCatalogo } from '../../../../core/models/sku-catalogo.model';
import { SkusService } from '../../../../core/services/skus.service';
import { PaginationComponent } from '../../../../shared/components/pagination.component';

@Component({
  selector: 'app-add-skus-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './add-skus-dialog.component.html',
  styleUrl: './add-skus-dialog.component.css'
})
export class AddSkusDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly skusService = inject(SkusService);

  @Input() codigosExistentes: string[] = [];
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<AddCampaniaSkusPayload>();

  protected readonly filtersForm = this.fb.group({
    nombre: [''],
    marca: [''],
    categoria: [''],
    codigoSkuBimbo: [''],
    pageSize: [10, [Validators.required]]
  });

  protected pagedResult: PagedResult<SkuCatalogo> = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  };

  protected loading = false;
  protected readonly selectedCodes = new Set<string>();

  ngOnInit(): void {
    this.filtersForm.valueChanges
      .pipe(debounceTime(250), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load(1));

    this.load();
  }

  protected load(pageNumber = 1): void {
    this.loading = true;
    const filters = this.filtersForm.getRawValue();

    this.skusService
      .listCatalogo({
        nombre: this.cleanFilter(filters.nombre),
        marca: this.cleanFilter(filters.marca),
        categoria: this.cleanFilter(filters.categoria),
        codigoSkuBimbo: this.cleanFilter(filters.codigoSkuBimbo),
        pageNumber,
        pageSize: Number(filters.pageSize) || 10
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.pagedResult = response;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  protected search(): void {
    this.load(1);
  }

  protected toggleSku(item: SkuCatalogo): void {
    if (this.isDisabled(item.codigoSkuBimbo)) {
      return;
    }

    if (this.selectedCodes.has(item.codigoSkuBimbo)) {
      this.selectedCodes.delete(item.codigoSkuBimbo);
      return;
    }

    this.selectedCodes.add(item.codigoSkuBimbo);
  }

  protected isSelected(code: string): boolean {
    return this.selectedCodes.has(code);
  }

  protected isDisabled(code: string): boolean {
    return this.codigosExistentes.includes(code);
  }

  protected toggleCurrentPage(selectAll: boolean): void {
    this.pagedResult.items.forEach((item) => {
      if (this.isDisabled(item.codigoSkuBimbo)) {
        return;
      }

      if (selectAll) {
        this.selectedCodes.add(item.codigoSkuBimbo);
        return;
      }

      this.selectedCodes.delete(item.codigoSkuBimbo);
    });
  }

  protected selectedPreview(): string[] {
    return Array.from(this.selectedCodes).sort().slice(0, 8);
  }

  protected canSubmit(): boolean {
    return this.selectedCodes.size > 0;
  }

  protected clearSelection(): void {
    this.selectedCodes.clear();
  }

  protected submit(): void {
    if (!this.canSubmit()) {
      return;
    }

    this.saved.emit({
      codigosSkuBimbo: Array.from(this.selectedCodes).sort()
    });
  }

  protected close(): void {
    this.closed.emit();
  }

  private cleanFilter(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }
}
