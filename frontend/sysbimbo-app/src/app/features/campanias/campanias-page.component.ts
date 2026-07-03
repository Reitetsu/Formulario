import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { CampaniaOperacionResultado } from '../../core/models/campania-operacion.model';
import {
  AddCampaniaFechasPayload,
  AddCampaniaSkusPayload,
  AddCampaniaTiendasPayload
} from '../../core/models/campania-operacion.model';
import { CampaniaFecha } from '../../core/models/campania-fecha.model';
import { CampaniaProgramacionDetalle } from '../../core/models/campania-programacion-detalle.model';
import { CampaniaProgramacion } from '../../core/models/campania-programacion.model';
import { CampaniaResumen } from '../../core/models/campania-resumen.model';
import { CampaniaSku } from '../../core/models/campania-sku.model';
import { CampaniaTienda } from '../../core/models/campania-tienda.model';
import { Campania, CreateCampaniaPayload, UpdateCampaniaPayload } from '../../core/models/campania.model';
import { PagedResult } from '../../core/models/paged-result.model';
import { AlertService } from '../../core/services/alert.service';
import { CampaniasService } from '../../core/services/campanias.service';
import { AddSkusDialogComponent } from './components/add-skus-dialog/add-skus-dialog.component';
import { AddTiendasDialogComponent } from './components/add-tiendas-dialog/add-tiendas-dialog.component';
import { ManageFechasDialogComponent } from './components/manage-fechas-dialog/manage-fechas-dialog.component';
import { PaginationComponent } from '../../shared/components/pagination.component';

type CampaniaTab = 'tiendas' | 'skus';
type CampaniaActionMode = 'tiendas' | 'fechas' | 'skus' | null;

@Component({
  selector: 'app-campanias-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PaginationComponent,
    AddTiendasDialogComponent,
    ManageFechasDialogComponent,
    AddSkusDialogComponent
  ],
  templateUrl: './campanias-page.component.html',
  styleUrl: './campanias-page.component.css'
})
export class CampaniasPageComponent implements OnInit {
  private readonly shortDateFormatter = new Intl.DateTimeFormat('es-PE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly campaniasService = inject(CampaniasService);
  private readonly alertService = inject(AlertService);

  protected readonly filtersForm = this.fb.group({
    nombreCampania: [''],
    descripcion: [''],
    estado: [''],
    pageSize: [10, [Validators.required]]
  });

  protected readonly workspaceFiltersForm = this.fb.group({
    tienda: [''],
    cadena: [''],
    fecha: [''],
    sku: [''],
    marca: [''],
    categoria: ['']
  });

  protected readonly tiendasBatchForm = this.fb.group({
    tiendaCadenaKeys: ['', [Validators.required]],
    fechas: [''],
    replicarSkusExistentes: [true, [Validators.required]]
  });

  protected readonly fechasBatchForm = this.fb.group({
    fechas: ['', [Validators.required]],
    tiendaCadenaKeys: [''],
    aplicarATodasLasTiendas: [true, [Validators.required]],
    replicarSkusExistentes: [true, [Validators.required]]
  });

  protected readonly skusBatchForm = this.fb.group({
    codigosSkuBimbo: ['', [Validators.required]]
  });

  protected readonly editorForm = this.fb.group({
    campaniaId: [{ value: 0, disabled: true }],
    nombreCampania: ['', [Validators.required]],
    descripcion: [''],
    fechaInicio: [''],
    fechaFin: [''],
    estado: ['']
  });

  protected pagedResult: PagedResult<Campania> = {
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
  protected tabActiva: CampaniaTab = 'tiendas';
  protected campaniaSeleccionada: Campania | null = null;
  protected campaniaDetalle: Campania | null = null;
  protected resumen: CampaniaResumen | null = null;
  protected tiendas: CampaniaTienda[] = [];
  protected fechas: CampaniaFecha[] = [];
  protected skus: CampaniaSku[] = [];
  protected programaciones: CampaniaProgramacion[] = [];
  protected detallesProgramacion: CampaniaProgramacionDetalle[] = [];
  protected selectedProgramacionId: number | null = null;
  protected cargandoWorkspace = false;
  protected cargandoDetalles = false;
  protected accionActiva: CampaniaActionMode = null;
  protected procesandoAccion = false;

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

    this.campaniasService
      .list({
        nombreCampania: this.cleanFilter(filters.nombreCampania),
        descripcion: this.cleanFilter(filters.descripcion),
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
          this.handleError(error, 'No fue posible cargar las campanias.');
          this.cdr.detectChanges();
        }
      });
  }

  protected search(): void {
    this.load(1);
  }

  protected clearFilters(): void {
    this.filtersForm.reset({
      nombreCampania: '',
      descripcion: '',
      estado: '',
      pageSize: this.filtersForm.value.pageSize ?? 10
    });
    this.load(1);
  }

  protected startCreate(): void {
    this.isEditing = false;
    this.selectedId = null;
    this.editorForm.reset({
      campaniaId: 0,
      nombreCampania: '',
      descripcion: '',
      fechaInicio: '',
      fechaFin: '',
      estado: ''
    });
  }

  protected startEdit(item: Campania): void {
    this.isEditing = true;
    this.selectedId = item.campaniaId;
    this.editorForm.reset({
      campaniaId: item.campaniaId,
      nombreCampania: item.nombreCampania ?? '',
      descripcion: item.descripcion ?? '',
      fechaInicio: item.fechaInicio ? item.fechaInicio.slice(0, 10) : '',
      fechaFin: item.fechaFin ? item.fechaFin.slice(0, 10) : '',
      estado: item.estado ?? ''
    });
  }

  protected cancelEdit(): void {
    this.startCreate();
  }

  protected selectCampania(item: Campania): void {
    this.campaniaSeleccionada = item;
  }

  protected openCampaniaDetalle(): void {
    if (!this.campaniaSeleccionada) {
      return;
    }

    this.campaniaDetalle = this.campaniaSeleccionada;
    this.resetWorkspaceState();
    this.loadWorkspace(this.campaniaSeleccionada.campaniaId);
  }

  protected volverAlListado(): void {
    this.campaniaDetalle = null;
    this.resetWorkspaceState();
    this.cdr.detectChanges();
  }

  protected changeTab(tab: CampaniaTab): void {
    if (this.tabActiva === tab) {
      return;
    }

    this.tabActiva = tab;
    if (!this.campaniaDetalle) {
      return;
    }

    this.workspaceFiltersForm.reset(
      {
        tienda: '',
        cadena: '',
        fecha: '',
        sku: '',
        marca: '',
        categoria: ''
      },
      { emitEvent: false }
    );
    this.loadTabContent(this.campaniaDetalle.campaniaId);
  }

  protected reloadWorkspace(): void {
    if (!this.campaniaDetalle) {
      return;
    }

    this.loadWorkspace(this.campaniaDetalle.campaniaId);
  }

  protected openActionPanel(mode: Exclude<CampaniaActionMode, null>): void {
    if (!this.campaniaDetalle) {
      return;
    }

    this.accionActiva = this.accionActiva === mode ? null : mode;

    if (this.accionActiva === 'fechas' && this.tiendas.length === 0) {
      this.campaniasService
        .getTiendas(this.campaniaDetalle.campaniaId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (response) => {
            this.tiendas = response;
            this.cdr.detectChanges();
          },
          error: () => {
            this.cdr.detectChanges();
          }
        });
    }

    if (this.accionActiva === 'skus' && this.skus.length === 0) {
      this.campaniasService
        .getSkus(this.campaniaDetalle.campaniaId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (response) => {
            this.skus = response;
            this.cdr.detectChanges();
          },
          error: () => {
            this.cdr.detectChanges();
          }
        });
    }

    this.cdr.detectChanges();
  }

  protected closeActionPanel(): void {
    this.accionActiva = null;
    this.resetBatchForms();
  }

  protected showProgramacionDetails(item: CampaniaProgramacion): void {
    if (!this.campaniaDetalle) {
      return;
    }

    this.selectedProgramacionId = item.programacionId;
    this.cargandoDetalles = true;

    this.campaniasService
      .getDetalles(this.campaniaDetalle.campaniaId, item.programacionId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.detallesProgramacion = response;
          this.cargandoDetalles = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.cargandoDetalles = false;
          this.handleError(error, 'No fue posible cargar el detalle de la programacion seleccionada.');
          this.cdr.detectChanges();
        }
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
    const payload: CreateCampaniaPayload = {
      nombreCampania: (formValue.nombreCampania ?? '').trim(),
      descripcion: this.cleanText(formValue.descripcion),
      fechaInicio: this.cleanText(formValue.fechaInicio),
      fechaFin: this.cleanText(formValue.fechaFin),
      estado: this.cleanText(formValue.estado)
    };

    const request$ = this.isEditing && this.selectedId !== null
      ? this.campaniasService.update(this.selectedId, this.toUpdatePayload(payload))
      : this.campaniasService.create(payload);

    request$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.saving = false;
          this.alertService.success(this.isEditing ? 'Campania actualizada correctamente.' : 'Campania creada correctamente.');
          this.startCreate();
          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.saving = false;
          this.handleError(error, 'No fue posible guardar la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected delete(item: Campania): void {
    const confirmed = confirm(`Se eliminara la campania ${item.campaniaId}. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.alertService.clear();

    this.campaniasService
      .delete(item.campaniaId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.alertService.success('Campania eliminada correctamente.');
          if (this.selectedId === item.campaniaId) {
            this.startCreate();
          }

          if (this.campaniaSeleccionada?.campaniaId === item.campaniaId) {
            this.campaniaSeleccionada = null;
          }

          if (this.campaniaDetalle?.campaniaId === item.campaniaId) {
            this.campaniaDetalle = null;
            this.resetWorkspaceState();
          }

          this.load(this.pagedResult.pageNumber);
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.handleError(error, 'No fue posible eliminar la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected formatDate(value: string | null): string {
    if (!value) {
      return '-';
    }

    const parsedDate = this.parseDateValue(value);
    return parsedDate ? this.shortDateFormatter.format(parsedDate) : '-';
  }

  protected getEstadoClass(estadoFuncional: string): string {
    switch (estadoFuncional) {
      case 'Pendiente':
        return 'estado-pendiente';
      case 'ProgramadaHoy':
        return 'estado-hoy';
      case 'NoEjecutada':
        return 'estado-no-ejecutada';
      case 'Ejecutada':
        return 'estado-ejecutada';
      case 'Cancelada':
        return 'estado-cancelada';
      default:
        return 'estado-desconocido';
    }
  }

  protected isSelectedCampania(item: Campania): boolean {
    return this.campaniaSeleccionada?.campaniaId === item.campaniaId;
  }

  protected isSelectedProgramacion(item: CampaniaProgramacion): boolean {
    return this.selectedProgramacionId === item.programacionId;
  }

  protected filteredTiendas(): CampaniaTienda[] {
    const tienda = (this.workspaceFiltersForm.value.tienda ?? '').trim().toLowerCase();
    const cadena = (this.workspaceFiltersForm.value.cadena ?? '').trim().toLowerCase();
    const fecha = this.normalizeDateFilter(this.workspaceFiltersForm.value.fecha);

    return this.tiendas.filter((item) => {
      const matchesTienda = !tienda || [
        item.tiendaCadenaKey,
        item.codigoTiendaB2B ?? '',
        item.nombreTienda ?? '',
        item.nombreTiendaBimbo ?? ''
      ].some((value) => value.toLowerCase().includes(tienda));

      const matchesCadena = !cadena || [
        item.cadena ?? '',
        item.formato ?? '',
        item.region ?? ''
      ].some((value) => value.toLowerCase().includes(cadena));

      const matchesFecha = !fecha || this.programaciones.some((programacion) =>
        programacion.tiendaCadenaKey === item.tiendaCadenaKey && this.normalizeDateFilter(programacion.fecha) === fecha);

      return matchesTienda && matchesCadena && matchesFecha;
    });
  }

  protected filteredSkus(): CampaniaSku[] {
    const sku = (this.workspaceFiltersForm.value.sku ?? '').trim().toLowerCase();
    const marca = (this.workspaceFiltersForm.value.marca ?? '').trim().toLowerCase();
    const categoria = (this.workspaceFiltersForm.value.categoria ?? '').trim().toLowerCase();

    return this.skus.filter((item) => {
      const matchesSku = !sku || [
        item.codigoSkuBimbo,
        item.codigoSkuB2B ?? '',
        item.nombreSkuBimbo ?? '',
        item.nombreSkuB2B ?? ''
      ].some((value) => value.toLowerCase().includes(sku));

      const matchesMarca = !marca || [item.marca ?? '', item.area ?? '']
        .some((value) => value.toLowerCase().includes(marca));

      const matchesCategoria = !categoria || [item.categoria ?? '', item.unidadNegocio ?? '']
        .some((value) => value.toLowerCase().includes(categoria));

      return matchesSku && matchesMarca && matchesCategoria;
    });
  }

  protected filteredProgramaciones(): CampaniaProgramacion[] {
    const tienda = (this.workspaceFiltersForm.value.tienda ?? '').trim().toLowerCase();
    const cadena = (this.workspaceFiltersForm.value.cadena ?? '').trim().toLowerCase();
    const fecha = this.normalizeDateFilter(this.workspaceFiltersForm.value.fecha);

    return this.programaciones.filter((item) => {
      const matchesTienda = !tienda || [
        item.tiendaCadenaKey,
        item.nombreTienda ?? '',
        item.nombreTiendaBimbo ?? ''
      ].some((value) => value.toLowerCase().includes(tienda));

      const matchesCadena = !cadena || (item.cadena ?? '').toLowerCase().includes(cadena);
      const matchesFecha = !fecha || this.normalizeDateFilter(item.fecha) === fecha;

      return matchesTienda && matchesCadena && matchesFecha;
    });
  }

  protected filteredFechas(): CampaniaFecha[] {
    const fecha = this.normalizeDateFilter(this.workspaceFiltersForm.value.fecha);

    if (!fecha) {
      return this.fechas;
    }

    return this.fechas.filter((item) => this.normalizeDateFilter(item.fecha) === fecha);
  }

  protected submitTiendasAction(): void {
    if (!this.campaniaDetalle) {
      return;
    }

    if (this.tiendasBatchForm.invalid) {
      this.tiendasBatchForm.markAllAsTouched();
      return;
    }

    const tiendaCadenaKeys = this.parseBatchValues(this.tiendasBatchForm.value.tiendaCadenaKeys);
    const fechas = this.parseDateBatchValues(this.tiendasBatchForm.value.fechas);
    if (tiendaCadenaKeys.length === 0) {
      this.alertService.error('Ingresa al menos una tienda valida.');
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .addTiendas(this.campaniaDetalle.campaniaId, {
        tiendaCadenaKeys,
        fechas,
        replicarSkusExistentes: !!this.tiendasBatchForm.value.replicarSkusExistentes
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible agregar tiendas a la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected handleTiendasDialogSave(payload: AddCampaniaTiendasPayload): void {
    if (!this.campaniaDetalle) {
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .addTiendas(this.campaniaDetalle.campaniaId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible agregar tiendas a la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected submitFechasAction(): void {
    if (!this.campaniaDetalle) {
      return;
    }

    if (this.fechasBatchForm.invalid) {
      this.fechasBatchForm.markAllAsTouched();
      return;
    }

    const fechas = this.parseDateBatchValues(this.fechasBatchForm.value.fechas);
    const tiendaCadenaKeys = this.parseBatchValues(this.fechasBatchForm.value.tiendaCadenaKeys);
    const aplicarATodasLasTiendas = !!this.fechasBatchForm.value.aplicarATodasLasTiendas;
    if (fechas.length === 0) {
      this.alertService.error('Ingresa al menos una fecha valida en formato yyyy-MM-dd.');
      return;
    }

    if (!aplicarATodasLasTiendas && tiendaCadenaKeys.length === 0) {
      this.alertService.error('Ingresa al menos una tienda o marca la opcion para aplicar a todas las tiendas.');
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .addFechas(this.campaniaDetalle.campaniaId, {
        fechas,
        tiendaCadenaKeys,
        aplicarATodasLasTiendas,
        replicarSkusExistentes: !!this.fechasBatchForm.value.replicarSkusExistentes
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible agregar fechas a la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected handleFechasDialogSave(payload: AddCampaniaFechasPayload): void {
    if (!this.campaniaDetalle) {
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .addFechas(this.campaniaDetalle.campaniaId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible agregar fechas a la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected submitSkusAction(): void {
    if (!this.campaniaDetalle) {
      return;
    }

    if (this.skusBatchForm.invalid) {
      this.skusBatchForm.markAllAsTouched();
      return;
    }

    const codigosSkuBimbo = this.parseBatchValues(this.skusBatchForm.value.codigosSkuBimbo);
    if (codigosSkuBimbo.length === 0) {
      this.alertService.error('Ingresa al menos un codigo SKU Bimbo valido.');
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .addSkus(this.campaniaDetalle.campaniaId, { codigosSkuBimbo })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible agregar SKU a la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected handleSkusDialogSave(payload: AddCampaniaSkusPayload): void {
    if (!this.campaniaDetalle) {
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .addSkus(this.campaniaDetalle.campaniaId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible agregar SKU a la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected removeTienda(item: CampaniaTienda): void {
    if (!this.campaniaDetalle) {
      return;
    }

    const tienda = item.nombreTiendaBimbo || item.nombreTienda || item.tiendaCadenaKey;
    const confirmed = confirm(`Se retirara la tienda ${tienda} de la campania. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .removeTienda(this.campaniaDetalle.campaniaId, item.tiendaCadenaKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible retirar la tienda de la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected removeFecha(item: CampaniaFecha): void {
    if (!this.campaniaDetalle) {
      return;
    }

    const confirmed = confirm(`Se retirara la fecha ${item.fecha} de la campania. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .removeFecha(this.campaniaDetalle.campaniaId, item.fecha)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible retirar la fecha de la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  protected removeSku(item: CampaniaSku): void {
    if (!this.campaniaDetalle) {
      return;
    }

    const sku = item.nombreSkuBimbo || item.codigoSkuBimbo;
    const confirmed = confirm(`Se retirara el SKU ${sku} de la campania. Deseas continuar?`);
    if (!confirmed) {
      return;
    }

    this.procesandoAccion = true;
    this.alertService.clear();

    this.campaniasService
      .removeSku(this.campaniaDetalle.campaniaId, item.codigoSkuBimbo)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.handleWorkspaceOperationSuccess(result),
        error: (error) => {
          this.procesandoAccion = false;
          this.handleError(error, 'No fue posible retirar el SKU de la campania.');
          this.cdr.detectChanges();
        }
      });
  }

  private loadWorkspace(campaniaId: number): void {
    this.cargandoWorkspace = true;
    this.alertService.clear();

    forkJoin({
      resumen: this.campaniasService.getResumen(campaniaId),
      fechas: this.campaniasService.getFechas(campaniaId),
      programaciones: this.campaniasService.getProgramaciones(campaniaId),
      contenido: this.tabActiva === 'tiendas'
        ? this.campaniasService.getTiendas(campaniaId)
        : this.campaniasService.getSkus(campaniaId)
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ resumen, fechas, programaciones, contenido }) => {
          this.resumen = resumen;
          this.fechas = fechas;
          this.programaciones = programaciones;
          if (this.tabActiva === 'tiendas') {
            this.tiendas = contenido as CampaniaTienda[];
          } else {
            this.skus = contenido as CampaniaSku[];
          }
          this.cargandoWorkspace = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.cargandoWorkspace = false;
          this.handleError(error, 'No fue posible cargar el modulo de campania seleccionado.');
          this.cdr.detectChanges();
        }
      });
  }

  private loadTabContent(campaniaId: number): void {
    this.cargandoWorkspace = true;

    if (this.tabActiva === 'tiendas') {
      this.campaniasService
        .getTiendas(campaniaId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (response: CampaniaTienda[]) => {
            this.tiendas = response;
            this.cargandoWorkspace = false;
            this.cdr.detectChanges();
          },
          error: (error: unknown) => {
            this.cargandoWorkspace = false;
            this.handleError(error, 'No fue posible cargar el contenido de la pestana seleccionada.');
            this.cdr.detectChanges();
          }
        });
      return;
    }

    this.campaniasService
      .getSkus(campaniaId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response: CampaniaSku[]) => {
          this.skus = response;
          this.cargandoWorkspace = false;
          this.cdr.detectChanges();
        },
        error: (error: unknown) => {
          this.cargandoWorkspace = false;
          this.handleError(error, 'No fue posible cargar el contenido de la pestana seleccionada.');
          this.cdr.detectChanges();
        }
      });
  }

  private toUpdatePayload(payload: CreateCampaniaPayload): UpdateCampaniaPayload {
    return payload;
  }

  private handleWorkspaceOperationSuccess(result: CampaniaOperacionResultado): void {
    this.procesandoAccion = false;
    this.selectedProgramacionId = null;
    this.detallesProgramacion = [];
    this.closeActionPanel();
    this.alertService.success(this.buildOperationMessage(result));

    if (this.campaniaDetalle) {
      this.loadWorkspace(this.campaniaDetalle.campaniaId);
    }

    this.cdr.detectChanges();
  }

  private buildOperationMessage(result: CampaniaOperacionResultado): string {
    const summary = [
      result.mensaje,
      `Procesados: ${result.procesados}`,
      `Creados: ${result.creados}`,
      `Reactivados: ${result.reactivados}`,
      `Actualizados: ${result.actualizados}`,
      `Eliminados: ${result.eliminados}`,
      `Omitidos: ${result.omitidos}`
    ];

    if (result.detallesCreados > 0) {
      summary.push(`Detalles creados: ${result.detallesCreados}`);
    }

    if (result.detallesEliminados > 0) {
      summary.push(`Detalles eliminados: ${result.detallesEliminados}`);
    }

    if (result.advertencias?.length) {
      summary.push(`Advertencias: ${result.advertencias.join(' | ')}`);
    }

    return summary.join(' | ');
  }

  private parseBatchValues(value: string | null | undefined): string[] {
    return (value ?? '')
      .split(/[\n,;]+/)
      .map((item) => item.trim())
      .filter((item) => item.length > 0);
  }

  private parseDateBatchValues(value: string | null | undefined): string[] {
    return this.parseBatchValues(value).filter((item) => /^\d{4}-\d{2}-\d{2}$/.test(item));
  }

  private resetBatchForms(): void {
    this.tiendasBatchForm.reset({
      tiendaCadenaKeys: '',
      fechas: '',
      replicarSkusExistentes: true
    });
    this.fechasBatchForm.reset({
      fechas: '',
      tiendaCadenaKeys: '',
      aplicarATodasLasTiendas: true,
      replicarSkusExistentes: true
    });
    this.skusBatchForm.reset({ codigosSkuBimbo: '' });
  }

  private resetWorkspaceState(): void {
    this.selectedProgramacionId = null;
    this.detallesProgramacion = [];
    this.tabActiva = 'tiendas';
    this.accionActiva = null;
    this.resumen = null;
    this.tiendas = [];
    this.fechas = [];
    this.skus = [];
    this.programaciones = [];
    this.cargandoWorkspace = false;
    this.cargandoDetalles = false;
    this.resetBatchForms();
    this.workspaceFiltersForm.reset(
      {
        tienda: '',
        cadena: '',
        fecha: '',
        sku: '',
        marca: '',
        categoria: ''
      },
      { emitEvent: false }
    );
  }

  private normalizeDateFilter(value: string | null | undefined): string {
    if (!value) {
      return '';
    }

    const trimmed = value.trim();
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(trimmed);
    return match ? `${match[1]}-${match[2]}-${match[3]}` : '';
  }

  private parseDateValue(value: string): Date | null {
    const trimmed = value.trim();
    const dateOnlyMatch = /^(\d{4})-(\d{2})-(\d{2})$/.exec(trimmed);

    if (dateOnlyMatch) {
      const [, year, month, day] = dateOnlyMatch;
      return new Date(Number(year), Number(month) - 1, Number(day));
    }

    const parsed = new Date(trimmed);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
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
