import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { PagedResult } from '../../core/models/paged-result.model';
import {
  CreateTiendaPayload,
  Tienda,
  UpdateTiendaPayload
} from '../../core/models/tienda.model';
import { AlertService } from '../../core/services/alert.service';
import { TiendasService } from '../../core/services/tiendas.service';
import { PaginationComponent } from '../../shared/components/pagination.component';

@Component({
  selector: 'app-tiendas-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './tiendas-page.component.html',
  styleUrl: './tiendas-page.component.css'
})
export class TiendasPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly tiendasService = inject(TiendasService);
  private readonly alertService = inject(AlertService);

  protected readonly filtersForm = this.fb.group({
    nombre: [''],
    cadena: [''],
    region: [''],
    codigoTiendaB2B: [''],
    pageSize: [10, [Validators.required]]
  });

  protected readonly editorForm = this.fb.group({
    tiendaCadenaKey: ['', [Validators.required]],
    codigoTiendaB2BPrefijo: [''],
    codigoTiendaB2B: [''],
    nombreTienda: [''],
    nombreTiendaBimbo: [''],
    canal: [''],
    cadena: [''],
    formato: [''],
    tipoLocal: [''],
    limaProvincias: [''],
    region: [''],
    provincia: [''],
    ruta: [''],
    supervisor: [''],
    gestor: [''],
    vendedor: [''],
    ultimaFecha: [''],
    cantidadRegistros: [null as number | null],
    fuenteTienda: ['']
  });

  protected pagedResult: PagedResult<Tienda> = {
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

    this.tiendasService
      .list({
        nombre: this.cleanFilter(filters.nombre),
        cadena: this.cleanFilter(filters.cadena),
        region: this.cleanFilter(filters.region),
        codigoTiendaB2B: this.cleanFilter(filters.codigoTiendaB2B),
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
          this.handleError(error, 'No fue posible cargar las tiendas.');
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
      cadena: '',
      region: '',
      codigoTiendaB2B: '',
      pageSize: this.filtersForm.value.pageSize ?? 10
    });
    this.load(1);
  }

  protected startCreate(): void {
    this.isEditing = false;
    this.selectedId = null;
    this.editorForm.enable();
    this.editorForm.reset({
      tiendaCadenaKey: '',
      codigoTiendaB2BPrefijo: '',
      codigoTiendaB2B: '',
      nombreTienda: '',
      nombreTiendaBimbo: '',
      canal: '',
      cadena: '',
      formato: '',
      tipoLocal: '',
      limaProvincias: '',
      region: '',
      provincia: '',
      ruta: '',
      supervisor: '',
      gestor: '',
      vendedor: '',
      ultimaFecha: '',
      cantidadRegistros: null,
      fuenteTienda: ''
    });
  }

  protected startEdit(item: Tienda): void {
    this.isEditing = true;
    this.selectedId = item.tiendaCadenaKey;
    this.editorForm.reset({
      tiendaCadenaKey: item.tiendaCadenaKey,
      codigoTiendaB2BPrefijo: item.codigoTiendaB2BPrefijo ?? '',
      codigoTiendaB2B: item.codigoTiendaB2B ?? '',
      nombreTienda: item.nombreTienda ?? '',
      nombreTiendaBimbo: item.nombreTiendaBimbo ?? '',
      canal: item.canal ?? '',
      cadena: item.cadena ?? '',
      formato: item.formato ?? '',
      tipoLocal: item.tipoLocal ?? '',
      limaProvincias: item.limaProvincias ?? '',
      region: item.region ?? '',
      provincia: item.provincia ?? '',
      ruta: item.ruta ?? '',
      supervisor: item.supervisor ?? '',
      gestor: item.gestor ?? '',
      vendedor: item.vendedor ?? '',
      ultimaFecha: item.ultimaFecha ? item.ultimaFecha.slice(0, 10) : '',
      cantidadRegistros: item.cantidadRegistros,
      fuenteTienda: item.fuenteTienda ?? ''
    });
    this.editorForm.controls.tiendaCadenaKey.disable();
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

    const createPayload: CreateTiendaPayload = {
      tiendaCadenaKey: (formValue.tiendaCadenaKey ?? '').trim(),
      codigoTiendaB2BPrefijo: this.cleanText(formValue.codigoTiendaB2BPrefijo),
      codigoTiendaB2B: this.cleanText(formValue.codigoTiendaB2B),
      nombreTienda: this.cleanText(formValue.nombreTienda),
      nombreTiendaBimbo: this.cleanText(formValue.nombreTiendaBimbo),
      canal: this.cleanText(formValue.canal),
      cadena: this.cleanText(formValue.cadena),
      formato: this.cleanText(formValue.formato),
      tipoLocal: this.cleanText(formValue.tipoLocal),
      limaProvincias: this.cleanText(formValue.limaProvincias),
      region: this.cleanText(formValue.region),
      provincia: this.cleanText(formValue.provincia),
      ruta: this.cleanText(formValue.ruta),
      supervisor: this.cleanText(formValue.supervisor),
      gestor: this.cleanText(formValue.gestor),
      vendedor: this.cleanText(formValue.vendedor),
      ultimaFecha: this.cleanText(formValue.ultimaFecha),
      cantidadRegistros: formValue.cantidadRegistros,
      fuenteTienda: this.cleanText(formValue.fuenteTienda)
    };

    const request$ = this.isEditing && this.selectedId
      ? this.tiendasService.update(this.selectedId, this.toUpdatePayload(createPayload))
      : this.tiendasService.create(createPayload);

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.saving = false))
      )
      .subscribe({
        next: () => {
          this.alertService.success(this.isEditing ? 'Tienda actualizada correctamente.' : 'Tienda creada correctamente.');
          this.startCreate();
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible guardar la tienda.');
          this.cdr.detectChanges();
        }
      });
  }

  protected delete(item: Tienda): void {
    const confirmed = confirm(`Se eliminara la tienda ${item.tiendaCadenaKey}. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.alertService.clear();

    this.tiendasService
      .delete(item.tiendaCadenaKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.alertService.success('Tienda eliminada correctamente.');
          if (this.selectedId === item.tiendaCadenaKey) {
            this.startCreate();
          }
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible eliminar la tienda.');
          this.cdr.detectChanges();
        }
      });
  }

  private toUpdatePayload(payload: CreateTiendaPayload): UpdateTiendaPayload {
    const { tiendaCadenaKey: _discard, ...rest } = payload;
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
