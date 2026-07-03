import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DetalleProgramacion } from '../../core/models/detalle-programacion.model';
import { PagedResult } from '../../core/models/paged-result.model';
import {
  CreateProgramacionPayload,
  Programacion,
  UpdateProgramacionPayload
} from '../../core/models/programacion.model';
import { AlertService } from '../../core/services/alert.service';
import { ProgramacionesService } from '../../core/services/programaciones.service';
import { PaginationComponent } from '../../shared/components/pagination.component';

@Component({
  selector: 'app-programaciones-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './programaciones-page.component.html',
  styleUrl: './programaciones-page.component.css'
})
export class ProgramacionesPageComponent implements OnInit {
  private readonly weekdayFormatter = new Intl.DateTimeFormat('es-PE', { weekday: 'long' });
  private readonly shortDateFormatter = new Intl.DateTimeFormat('es-PE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly programacionesService = inject(ProgramacionesService);
  private readonly alertService = inject(AlertService);

  protected readonly filtersForm = this.fb.group({
    nombreCampania: [''],
    nombreTiendaBimbo: [''],
    fecha: [''],
    cuota: [null as number | null],
    estado: [''],
    pageSize: [10, [Validators.required]]
  });

  protected readonly detailFiltersForm = this.fb.group({
    codigoSkuBimbo: [''],
    nombreSkuBimbo: [''],
    fechaCreacion: ['']
  });

  protected readonly editorForm = this.fb.group({
    programacionId: [{ value: 0, disabled: true }],
    campaniaId: [null as number | null],
    nombreCampania: [{ value: '', disabled: true }],
    tiendaCadenaKey: ['', [Validators.required]],
    nombreTiendaBimbo: [{ value: '', disabled: true }],
    fecha: ['', [Validators.required]],
    cuota: [null as number | null],
    estado: [''],
    fuenteProgramacion: [''],
    fechaCreacion: [''],
    fechaActualizacion: ['']
  });

  protected pagedResult: PagedResult<Programacion> = {
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
  protected selectedProgramacionForDetail: Programacion | null = null;
  protected detalleItems: DetalleProgramacion[] = [];
  protected filteredDetalleItems: DetalleProgramacion[] = [];
  protected detalleLoading = false;
  protected detailMode = false;

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

    this.programacionesService
      .list({
        nombreCampania: this.cleanFilter(filters.nombreCampania),
        nombreTiendaBimbo: this.cleanFilter(filters.nombreTiendaBimbo),
        fecha: this.cleanFilter(filters.fecha),
        cuota: filters.cuota ?? undefined,
        estado: this.cleanFilter(filters.estado),
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
          this.handleError(error, 'No fue posible cargar las programaciones.');
          this.cdr.detectChanges();
        }
      });
  }

  protected search(): void {
    if (this.detailMode) {
      this.applyDetailFilters();
      return;
    }

    this.load(1);
  }

  protected clearFilters(): void {
    if (this.detailMode) {
      this.detailFiltersForm.reset({
        codigoSkuBimbo: '',
        nombreSkuBimbo: '',
        fechaCreacion: ''
      });
      this.applyDetailFilters();
      return;
    }

    this.filtersForm.reset({
      nombreCampania: '',
      nombreTiendaBimbo: '',
      fecha: '',
      cuota: null,
      estado: '',
      pageSize: this.filtersForm.value.pageSize ?? 10
    });
    this.load(1);
  }

  protected startCreate(): void {
    this.isEditing = false;
    this.selectedId = null;
    this.editorForm.reset({
      programacionId: 0,
      campaniaId: null,
      nombreCampania: '',
      tiendaCadenaKey: '',
      nombreTiendaBimbo: '',
      fecha: '',
      cuota: null,
      estado: '',
      fuenteProgramacion: '',
      fechaCreacion: '',
      fechaActualizacion: ''
    });
  }

  protected startEdit(item: Programacion): void {
    this.isEditing = true;
    this.selectedId = item.programacionId;
    this.selectedProgramacionForDetail = item;
    this.editorForm.reset({
      programacionId: item.programacionId,
      campaniaId: item.campaniaId,
      nombreCampania: item.nombreCampania ?? '',
      tiendaCadenaKey: item.tiendaCadenaKey ?? '',
      nombreTiendaBimbo: item.nombreTiendaBimbo ?? '',
      fecha: item.fecha ? item.fecha.slice(0, 10) : '',
      cuota: item.cuota,
      estado: item.estado ?? '',
      fuenteProgramacion: item.fuenteProgramacion ?? '',
      fechaCreacion: item.fechaCreacion ? item.fechaCreacion.slice(0, 10) : '',
      fechaActualizacion: item.fechaActualizacion ? item.fechaActualizacion.slice(0, 10) : ''
    });
  }

  protected cancelEdit(): void {
    this.startCreate();
  }

  protected selectProgramacion(item: Programacion): void {
    this.selectedProgramacionForDetail = item;
  }

  protected showDetail(): void {
    if (!this.selectedProgramacionForDetail) {
      return;
    }

    if (!this.detailMode) {
      this.detailFiltersForm.reset({
        codigoSkuBimbo: '',
        nombreSkuBimbo: '',
        fechaCreacion: ''
      });
    }

    this.detailMode = true;
    this.detalleLoading = true;
    this.alertService.clear();

    this.programacionesService
      .getDetail(this.selectedProgramacionForDetail.programacionId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.detalleItems = items;
          this.applyDetailFilters();
          this.detalleLoading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.detalleLoading = false;
          this.detailMode = false;
          this.handleError(error, 'No fue posible cargar el detalle de la programacion.');
          this.cdr.detectChanges();
        }
      });
  }

  protected backToProgramaciones(): void {
    this.detailMode = false;
    this.detalleItems = [];
    this.filteredDetalleItems = [];
    this.detalleLoading = false;
    this.detailFiltersForm.reset({
      codigoSkuBimbo: '',
      nombreSkuBimbo: '',
      fechaCreacion: ''
    });
  }

  protected submit(): void {
    if (this.editorForm.invalid) {
      this.editorForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.alertService.clear();

    const formValue = this.editorForm.getRawValue();
    const payload: CreateProgramacionPayload = {
      campaniaId: formValue.campaniaId,
      tiendaCadenaKey: (formValue.tiendaCadenaKey ?? '').trim(),
      fecha: (formValue.fecha ?? '').trim(),
      cuota: formValue.cuota,
      estado: this.cleanText(formValue.estado),
      fuenteProgramacion: this.cleanText(formValue.fuenteProgramacion),
      fechaCreacion: this.cleanText(formValue.fechaCreacion),
      fechaActualizacion: this.cleanText(formValue.fechaActualizacion)
    };

    const request$ = this.isEditing && this.selectedId !== null
      ? this.programacionesService.update(this.selectedId, this.toUpdatePayload(payload))
      : this.programacionesService.create(payload);

    request$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.saving = false;
          this.alertService.success(this.isEditing ? 'Programacion actualizada correctamente.' : 'Programacion creada correctamente.');
          this.startCreate();
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.saving = false;
          this.handleError(error, 'No fue posible guardar la programacion.');
          this.cdr.detectChanges();
        }
      });
  }

  protected delete(item: Programacion): void {
    const confirmed = confirm(`Se eliminara la programacion ${item.programacionId}. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.alertService.clear();

    this.programacionesService
      .delete(item.programacionId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.alertService.success('Programacion eliminada correctamente.');
          if (this.selectedId === item.programacionId) {
            this.startCreate();
          }
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible eliminar la programacion.');
          this.cdr.detectChanges();
        }
      });
  }

  private toUpdatePayload(payload: CreateProgramacionPayload): UpdateProgramacionPayload {
    return payload;
  }

  private cleanText(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private cleanFilter(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  protected formatWeekday(value: string | null): string {
    if (!value) {
      return '-';
    }

    const date = new Date(value);
    const weekday = this.weekdayFormatter.format(date);
    return weekday.charAt(0).toUpperCase() + weekday.slice(1);
  }

  protected formatShortDate(value: string | null): string {
    if (!value) {
      return '-';
    }

    return this.shortDateFormatter.format(new Date(value));
  }

  protected reloadCurrentView(): void {
    if (this.detailMode) {
      this.showDetail();
      return;
    }

    this.load(this.pagedResult.pageNumber);
  }

  protected isSelected(item: Programacion): boolean {
    return this.selectedProgramacionForDetail?.programacionId === item.programacionId;
  }

  private applyDetailFilters(): void {
    const filters = this.detailFiltersForm.getRawValue();
    const codigoSkuBimbo = this.cleanFilter(filters.codigoSkuBimbo)?.toLowerCase();
    const nombreSkuBimbo = this.cleanFilter(filters.nombreSkuBimbo)?.toLowerCase();
    const fechaCreacion = this.cleanFilter(filters.fechaCreacion);

    this.filteredDetalleItems = this.detalleItems.filter((item) => {
      const matchesCodigo = !codigoSkuBimbo || item.codigoSkuBimbo.toLowerCase().includes(codigoSkuBimbo);
      const matchesNombre = !nombreSkuBimbo || (item.nombreSkuBimbo ?? '').toLowerCase().includes(nombreSkuBimbo);
      const matchesFecha = !fechaCreacion || (item.fechaCreacion ? item.fechaCreacion.slice(0, 10) === fechaCreacion : false);

      return matchesCodigo && matchesNombre && matchesFecha;
    });
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
