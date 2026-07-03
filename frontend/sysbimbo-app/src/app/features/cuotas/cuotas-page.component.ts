import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { CreateCuotaPayload, Cuota, UpdateCuotaPayload } from '../../core/models/cuota.model';
import { PagedResult } from '../../core/models/paged-result.model';
import { AlertService } from '../../core/services/alert.service';
import { CuotasService } from '../../core/services/cuotas.service';
import { PaginationComponent } from '../../shared/components/pagination.component';

@Component({
  selector: 'app-cuotas-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './cuotas-page.component.html',
  styleUrl: './cuotas-page.component.css'
})
export class CuotasPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cuotasService = inject(CuotasService);
  private readonly alertService = inject(AlertService);

  protected readonly filtersForm = this.fb.group({
    campania: [''],
    tiendaCadenaKey: [''],
    fecha: [''],
    pageSize: [10, [Validators.required]]
  });

  protected readonly editorForm = this.fb.group({
    cuotaId: [{ value: 0, disabled: true }],
    campania: ['', [Validators.required]],
    tiendaCadenaKey: ['', [Validators.required]],
    fecha: ['', [Validators.required]],
    cuota: [null as number | null, [Validators.required]]
  });

  protected pagedResult: PagedResult<Cuota> = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  };

  protected loading = false;
  protected saving = false;
  protected isEditing = false;
  protected selectedId: number | null = null;

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

    this.cuotasService
      .list({
        campania: this.cleanFilter(filters.campania),
        tiendaCadenaKey: this.cleanFilter(filters.tiendaCadenaKey),
        fecha: this.cleanFilter(filters.fecha),
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
          this.handleError(error, 'No fue posible cargar las cuotas.');
          this.cdr.detectChanges();
        }
      });
  }

  protected search(): void {
    this.load(1);
  }

  protected clearFilters(): void {
    this.filtersForm.reset({
      campania: '',
      tiendaCadenaKey: '',
      fecha: '',
      pageSize: this.filtersForm.value.pageSize ?? 10
    });
    this.load(1);
  }

  protected startCreate(): void {
    this.isEditing = false;
    this.selectedId = null;
    this.editorForm.reset({
      cuotaId: 0,
      campania: '',
      tiendaCadenaKey: '',
      fecha: '',
      cuota: null
    });
  }

  protected startEdit(item: Cuota): void {
    this.isEditing = true;
    this.selectedId = item.cuotaId;
    this.editorForm.reset({
      cuotaId: item.cuotaId,
      campania: item.campania ?? '',
      tiendaCadenaKey: item.tiendaCadenaKey ?? '',
      fecha: item.fecha ? item.fecha.slice(0, 10) : '',
      cuota: item.cuota
    });
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
    const payload: CreateCuotaPayload = {
      campania: (formValue.campania ?? '').trim(),
      tiendaCadenaKey: (formValue.tiendaCadenaKey ?? '').trim(),
      fecha: (formValue.fecha ?? '').trim(),
      cuota: Number(formValue.cuota)
    };

    const request$ = this.isEditing && this.selectedId !== null
      ? this.cuotasService.update(this.selectedId, this.toUpdatePayload(payload))
      : this.cuotasService.create(payload);

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.saving = false))
      )
      .subscribe({
        next: () => {
          this.alertService.success(this.isEditing ? 'Cuota actualizada correctamente.' : 'Cuota creada correctamente.');
          this.startCreate();
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible guardar la cuota.');
          this.cdr.detectChanges();
        }
      });
  }

  protected delete(item: Cuota): void {
    const confirmed = confirm(`Se eliminara la cuota ${item.cuotaId}. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.alertService.clear();

    this.cuotasService
      .delete(item.cuotaId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.alertService.success('Cuota eliminada correctamente.');
          if (this.selectedId === item.cuotaId) {
            this.startCreate();
          }
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible eliminar la cuota.');
          this.cdr.detectChanges();
        }
      });
  }

  private toUpdatePayload(payload: CreateCuotaPayload): UpdateCuotaPayload {
    return payload;
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
