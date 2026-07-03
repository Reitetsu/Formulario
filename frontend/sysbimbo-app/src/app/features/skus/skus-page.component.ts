import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { PagedResult } from '../../core/models/paged-result.model';
import { CreateSkuPayload, Sku, UpdateSkuPayload } from '../../core/models/sku.model';
import { AlertService } from '../../core/services/alert.service';
import { SkusService } from '../../core/services/skus.service';
import { PaginationComponent } from '../../shared/components/pagination.component';

@Component({
  selector: 'app-skus-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './skus-page.component.html',
  styleUrl: './skus-page.component.css'
})
export class SkusPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly skusService = inject(SkusService);
  private readonly alertService = inject(AlertService);

  protected readonly filtersForm = this.fb.group({
    nombre: [''],
    marca: [''],
    categoria: [''],
    codigoSkuB2B: [''],
    codigoSkuBimbo: [''],
    pageSize: [10, [Validators.required]]
  });

  protected readonly editorForm = this.fb.group({
    skuKey: ['', [Validators.required]],
    codigoSkuB2B: [''],
    nombreSkuB2B: [''],
    codigoSkuBimbo: [''],
    nombreSkuBimbo: [''],
    unidadNegocio: [''],
    area: [''],
    categoria: [''],
    marca: [''],
    tipoProducto: [''],
    status: [''],
    gramaje: [''],
    ultimaFecha: [''],
    cantidadRegistros: [null as number | null],
    fuenteSku: ['']
  });

  protected pagedResult: PagedResult<Sku> = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  };

  protected loading = false;
  protected saving = false;
  protected isEditing = false;
  protected selectedId: string | null = null;

  constructor() {
    this.startCreate();
  }

  ngOnInit(): void {
    this.load();
  }

  protected load(pageNumber = 1): void {
    this.loading = true;
    this.alertService.clear();

    const filters = this.filtersForm.getRawValue();

    this.skusService
      .list({
        nombre: this.cleanFilter(filters.nombre),
        marca: this.cleanFilter(filters.marca),
        categoria: this.cleanFilter(filters.categoria),
        codigoSkuB2B: this.cleanFilter(filters.codigoSkuB2B),
        codigoSkuBimbo: this.cleanFilter(filters.codigoSkuBimbo),
        pageNumber,
        pageSize: Number(filters.pageSize) || 10
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.pagedResult = response;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.loading = false;
          this.handleError(error, 'No fue posible cargar los SKUs.');
          this.cdr.detectChanges();
        }
      });
  }

  protected search(): void {
    this.load(1);
  }

  protected clearFilters(): void {
    this.filtersForm.reset({
      nombre: '',
      marca: '',
      categoria: '',
      codigoSkuB2B: '',
      codigoSkuBimbo: '',
      pageSize: this.filtersForm.value.pageSize ?? 10
    });
    this.load(1);
  }

  protected startCreate(): void {
    this.isEditing = false;
    this.selectedId = null;
    this.editorForm.enable();
    this.editorForm.reset({
      skuKey: '',
      codigoSkuB2B: '',
      nombreSkuB2B: '',
      codigoSkuBimbo: '',
      nombreSkuBimbo: '',
      unidadNegocio: '',
      area: '',
      categoria: '',
      marca: '',
      tipoProducto: '',
      status: '',
      gramaje: '',
      ultimaFecha: '',
      cantidadRegistros: null,
      fuenteSku: ''
    });
  }

  protected startEdit(item: Sku): void {
    this.isEditing = true;
    this.selectedId = item.skuKey;
    this.editorForm.reset({
      skuKey: item.skuKey,
      codigoSkuB2B: item.codigoSkuB2B ?? '',
      nombreSkuB2B: item.nombreSkuB2B ?? '',
      codigoSkuBimbo: item.codigoSkuBimbo ?? '',
      nombreSkuBimbo: item.nombreSkuBimbo ?? '',
      unidadNegocio: item.unidadNegocio ?? '',
      area: item.area ?? '',
      categoria: item.categoria ?? '',
      marca: item.marca ?? '',
      tipoProducto: item.tipoProducto ?? '',
      status: item.status ?? '',
      gramaje: item.gramaje ?? '',
      ultimaFecha: item.ultimaFecha ? item.ultimaFecha.slice(0, 10) : '',
      cantidadRegistros: item.cantidadRegistros,
      fuenteSku: item.fuenteSku ?? ''
    });
    this.editorForm.controls.skuKey.disable();
  }

  protected cancelEdit(): void {
    this.startCreate();
  }

  protected submit(): void {
    if (this.editorForm.invalid) {
      this.editorForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.alertService.clear();

    const formValue = this.editorForm.getRawValue();

    const createPayload: CreateSkuPayload = {
      skuKey: (formValue.skuKey ?? '').trim(),
      codigoSkuB2B: this.cleanText(formValue.codigoSkuB2B),
      nombreSkuB2B: this.cleanText(formValue.nombreSkuB2B),
      codigoSkuBimbo: this.cleanText(formValue.codigoSkuBimbo),
      nombreSkuBimbo: this.cleanText(formValue.nombreSkuBimbo),
      unidadNegocio: this.cleanText(formValue.unidadNegocio),
      area: this.cleanText(formValue.area),
      categoria: this.cleanText(formValue.categoria),
      marca: this.cleanText(formValue.marca),
      tipoProducto: this.cleanText(formValue.tipoProducto),
      status: this.cleanText(formValue.status),
      gramaje: this.cleanText(formValue.gramaje),
      ultimaFecha: this.cleanText(formValue.ultimaFecha),
      cantidadRegistros: formValue.cantidadRegistros,
      fuenteSku: this.cleanText(formValue.fuenteSku)
    };

    const request$ = this.isEditing && this.selectedId
      ? this.skusService.update(this.selectedId, this.toUpdatePayload(createPayload))
      : this.skusService.create(createPayload);

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.saving = false))
      )
      .subscribe({
        next: () => {
          this.alertService.success(this.isEditing ? 'SKU actualizado correctamente.' : 'SKU creado correctamente.');
          this.startCreate();
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible guardar el SKU.');
          this.cdr.detectChanges();
        }
      });
  }

  protected delete(item: Sku): void {
    const confirmed = confirm(`Se eliminara el SKU ${item.skuKey}. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.alertService.clear();

    this.skusService
      .delete(item.skuKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.alertService.success('SKU eliminado correctamente.');
          if (this.selectedId === item.skuKey) {
            this.startCreate();
          }
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible eliminar el SKU.');
          this.cdr.detectChanges();
        }
      });
  }

  private toUpdatePayload(payload: CreateSkuPayload): UpdateSkuPayload {
    const { skuKey: _discard, ...rest } = payload;
    return rest;
  }

  private cleanText(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private cleanFilter(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  private handleError(error: unknown, fallbackMessage: string): void {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        this.alertService.error('No fue posible conectarse al backend en http://localhost:5105. Verifica que la API este ejecutandose.');
        return;
      }

      const message = error.error?.message ?? error.error?.detail ?? fallbackMessage;
      this.alertService.error(message);
      return;
    }

    this.alertService.error(fallbackMessage);
  }
}
