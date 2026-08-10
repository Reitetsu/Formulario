import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, combineLatest, debounceTime, distinctUntilChanged, finalize, of, startWith, switchMap } from 'rxjs';
import { MaterialImpulsoAdmin } from '../../core/models/material-impulso.model';
import { PagedResult } from '../../core/models/paged-result.model';
import { Tienda } from '../../core/models/tienda.model';
import { AlertService } from '../../core/services/alert.service';
import { MaterialesImpulsoService } from '../../core/services/materiales-impulso.service';
import { TiendasService } from '../../core/services/tiendas.service';
import { PaginationComponent } from '../../shared/components/pagination.component';

@Component({
  selector: 'app-materiales-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './materiales-page.component.html',
  styleUrl: './materiales-page.component.css'
})
export class MaterialesPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly alertService = inject(AlertService);
  private readonly materialesService = inject(MaterialesImpulsoService);
  private readonly tiendasService = inject(TiendasService);

  protected readonly marcas = ['TOTTUS', 'METRO', 'MAKRO', 'PLAZA VEA'];
  protected readonly filtersForm = this.fb.group({
    tienda: [''],
    marca: [''],
    material: [''],
    pageSize: [10, Validators.required]
  });

  protected readonly editorForm = this.fb.group({
    marca: ['', Validators.required],
    tiendaSearch: ['', Validators.required],
    tiendaCadenaKey: ['', Validators.required],
    nombreMaterial: ['', Validators.required],
    cuotaDiaria: [null as number | null, [Validators.required, Validators.min(1)]],
    descripcion: ['']
  });

  protected pagedResult: PagedResult<MaterialImpulsoAdmin> = {
    items: [], pageNumber: 1, pageSize: 10, totalCount: 0, totalPages: 0
  };
  protected storeSuggestions: Tienda[] = [];
  protected selectedStore: Tienda | null = null;
  protected loading = false;
  protected loadingStores = false;
  protected saving = false;
  protected exporting = false;
  protected dropdownOpen = false;
  protected isEditing = false;
  protected selectedId: number | null = null;

  ngOnInit(): void {
    this.editorForm.controls.marca.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.isEditing) return;
        this.selectedStore = null;
        this.editorForm.patchValue({ tiendaSearch: '', tiendaCadenaKey: '' }, { emitEvent: false });
        this.storeSuggestions = [];
      });

    this.editorForm.controls.tiendaSearch.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(value => {
        if (this.selectedStore && value !== this.storeLabel(this.selectedStore)) {
          this.selectedStore = null;
          this.editorForm.controls.tiendaCadenaKey.setValue('', { emitEvent: false });
        }
      });

    this.configureStoreSearch();
    this.startCreate();
    this.load();
  }

  protected load(pageNumber = 1): void {
    const filters = this.filtersForm.getRawValue();
    this.loading = true;
    this.materialesService.list({
      tienda: this.clean(filters.tienda),
      marca: this.clean(filters.marca),
      material: this.clean(filters.material),
      pageNumber,
      pageSize: Number(filters.pageSize) || 10
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: result => {
        this.pagedResult = result;
        this.cdr.markForCheck();
      },
      error: error => {
        this.handleError(error, 'No fue posible cargar los materiales.');
        this.cdr.markForCheck();
      }
    });
  }

  protected clearFilters(): void {
    this.filtersForm.reset({ tienda: '', marca: '', material: '', pageSize: 10 });
    this.load(1);
  }

  protected exportExcel(): void {
    const filters = this.filtersForm.getRawValue();
    this.exporting = true;

    this.materialesService.exportExcel({
      tienda: this.clean(filters.tienda),
      marca: this.clean(filters.marca),
      material: this.clean(filters.material),
      soloActivos: true,
      pageNumber: 1,
      pageSize: 1
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.exporting = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: file => {
        const url = URL.createObjectURL(file);
        const link = document.createElement('a');
        const stamp = new Date().toISOString().slice(0, 16).replace(/[-:T]/g, '');
        link.href = url;
        link.download = `materiales-impulso-${stamp}.xlsx`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.setTimeout(() => URL.revokeObjectURL(url), 1000);
        this.alertService.success('Reporte Excel descargado correctamente.');
      },
      error: error => this.handleError(error, 'No fue posible generar el reporte Excel.')
    });
  }

  protected startCreate(): void {
    this.isEditing = false;
    this.selectedId = null;
    this.selectedStore = null;
    this.storeSuggestions = [];
    this.editorForm.enable({ emitEvent: false });
    this.editorForm.reset({
      marca: '', tiendaSearch: '', tiendaCadenaKey: '', nombreMaterial: '', cuotaDiaria: null, descripcion: ''
    });
  }

  protected startEdit(item: MaterialImpulsoAdmin): void {
    this.isEditing = true;
    this.selectedId = item.materialImpulsoTiendaId;
    this.selectedStore = null;
    this.editorForm.reset({
      marca: item.formato ?? '',
      tiendaSearch: item.nombreTienda,
      tiendaCadenaKey: item.tiendaCadenaKey,
      nombreMaterial: item.nombreMaterial,
      cuotaDiaria: item.cuotaDiaria || null,
      descripcion: item.descripcion ?? ''
    }, { emitEvent: false });
    this.editorForm.controls.marca.disable({ emitEvent: false });
    this.editorForm.controls.tiendaSearch.disable({ emitEvent: false });
  }

  protected selectStore(store: Tienda): void {
    this.selectedStore = store;
    this.editorForm.patchValue({
      tiendaSearch: this.storeLabel(store),
      tiendaCadenaKey: store.tiendaCadenaKey
    }, { emitEvent: false });
    this.dropdownOpen = false;
  }

  protected openDropdown(): void {
    if (!this.isEditing && this.editorForm.controls.marca.value) this.dropdownOpen = true;
  }

  protected closeDropdown(): void {
    window.setTimeout(() => (this.dropdownOpen = false), 160);
  }

  protected submit(): void {
    if (this.editorForm.invalid) {
      this.editorForm.markAllAsTouched();
      return;
    }

    const value = this.editorForm.getRawValue();
    this.setSaving(true);
    const request$ = this.isEditing && this.selectedId
      ? this.materialesService.update(this.selectedId, {
          nombreMaterial: value.nombreMaterial!.trim(),
          cuotaDiaria: Number(value.cuotaDiaria),
          descripcion: this.cleanNull(value.descripcion)
        })
      : this.materialesService.create({
          tiendaCadenaKey: value.tiendaCadenaKey!,
          nombreMaterial: value.nombreMaterial!.trim(),
          cuotaDiaria: Number(value.cuotaDiaria),
          descripcion: this.cleanNull(value.descripcion)
        });

    request$.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.setSaving(false))
    ).subscribe({
      next: () => {
        this.alertService.success(this.isEditing ? 'Material actualizado correctamente.' : 'Material asignado correctamente.');
        this.startCreate();
        this.load(this.pagedResult.pageNumber);
        this.cdr.markForCheck();
      },
      error: error => {
        this.handleError(error, 'No fue posible guardar el material.');
        this.cdr.markForCheck();
      }
    });
  }

  protected delete(item: MaterialImpulsoAdmin): void {
    if (!confirm(`Se desactivara el material de ${item.nombreTienda}. Deseas continuar?`)) return;

    this.materialesService.delete(item.materialImpulsoTiendaId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.alertService.success('Material desactivado. Las evidencias se conservaron.');
          if (this.selectedId === item.materialImpulsoTiendaId) this.startCreate();
          this.load(this.pagedResult.pageNumber);
          this.cdr.markForCheck();
        },
        error: error => {
          this.handleError(error, 'No fue posible desactivar el material.');
          this.cdr.markForCheck();
        }
      });
  }

  protected storeLabel(store: Tienda): string {
    return store.nombreTiendaBimbo || store.nombreTienda || store.tiendaCadenaKey;
  }

  private configureStoreSearch(): void {
    combineLatest([
      this.editorForm.controls.marca.valueChanges.pipe(startWith(''), distinctUntilChanged()),
      this.editorForm.controls.tiendaSearch.valueChanges.pipe(startWith(''), debounceTime(250), distinctUntilChanged())
    ]).pipe(
      switchMap(([marca, search]) => {
        if (this.isEditing || !marca) return of({ items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
        this.loadingStores = true;
        return this.tiendasService.list({
          marca,
          nombre: this.clean(search),
          pageNumber: 1,
          pageSize: 20
        }).pipe(catchError(() => of({ items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 })));
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(result => {
      this.storeSuggestions = result.items;
      this.loadingStores = false;
      const current = this.editorForm.controls.tiendaSearch.value;
      if (this.selectedStore && current !== this.storeLabel(this.selectedStore)) {
        this.selectedStore = null;
        this.editorForm.controls.tiendaCadenaKey.setValue('', { emitEvent: false });
      }
      this.cdr.markForCheck();
    });
  }

  private setSaving(value: boolean): void {
    queueMicrotask(() => {
      this.saving = value;
      this.cdr.markForCheck();
    });
  }

  private clean(value: string | null | undefined): string | undefined {
    return value?.trim() || undefined;
  }

  private cleanNull(value: string | null | undefined): string | null {
    return value?.trim() || null;
  }

  private handleError(error: unknown, fallback: string): void {
    if (error instanceof HttpErrorResponse) {
      this.alertService.error(error.status === 0
        ? 'No se pudo conectar con la API.'
        : error.error?.message ?? error.error?.detail ?? fallback);
      return;
    }
    this.alertService.error(fallback);
  }
}
