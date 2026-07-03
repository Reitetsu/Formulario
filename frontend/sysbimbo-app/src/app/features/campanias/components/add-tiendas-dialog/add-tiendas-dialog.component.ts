import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { debounceTime } from 'rxjs';
import { AddCampaniaTiendasPayload } from '../../../../core/models/campania-operacion.model';
import { CampaniaFecha } from '../../../../core/models/campania-fecha.model';
import { PagedResult } from '../../../../core/models/paged-result.model';
import { Tienda } from '../../../../core/models/tienda.model';
import { TiendasService } from '../../../../core/services/tiendas.service';
import { PaginationComponent } from '../../../../shared/components/pagination.component';

@Component({
  selector: 'app-add-tiendas-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './add-tiendas-dialog.component.html',
  styleUrl: './add-tiendas-dialog.component.css'
})
export class AddTiendasDialogComponent implements OnChanges, OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly tiendasService = inject(TiendasService);

  @Input({ required: true }) fechasDisponibles: CampaniaFecha[] = [];
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<AddCampaniaTiendasPayload>();

  protected readonly filtersForm = this.fb.group({
    nombre: [''],
    cadena: [''],
    region: [''],
    codigoTiendaB2B: [''],
    pageSize: [10, [Validators.required]]
  });

  protected readonly optionsForm = this.fb.group({
    fechasTexto: [''],
    replicarSkusExistentes: [true, [Validators.required]]
  });

  protected pagedResult: PagedResult<Tienda> = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  };

  protected loading = false;
  protected selectedKeys = new Set<string>();
  protected readonly selectedFechas = new Set<string>();

  ngOnInit(): void {
    this.filtersForm.valueChanges
      .pipe(debounceTime(250), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load(1));

    this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fechasDisponibles']) {
      this.selectedFechas.clear();
      this.fechasDisponibles.forEach((item) => this.selectedFechas.add(item.fecha));
    }
  }

  protected load(pageNumber = 1): void {
    this.loading = true;
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
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  protected search(): void {
    this.load(1);
  }

  protected toggleTienda(item: Tienda): void {
    if (this.selectedKeys.has(item.tiendaCadenaKey)) {
      this.selectedKeys.delete(item.tiendaCadenaKey);
      return;
    }

    this.selectedKeys.add(item.tiendaCadenaKey);
  }

  protected toggleFecha(fecha: string): void {
    if (this.selectedFechas.has(fecha)) {
      this.selectedFechas.delete(fecha);
      return;
    }

    this.selectedFechas.add(fecha);
  }

  protected isSelected(item: Tienda): boolean {
    return this.selectedKeys.has(item.tiendaCadenaKey);
  }

  protected isFechaSelected(fecha: string): boolean {
    return this.selectedFechas.has(fecha);
  }

  protected toggleCurrentPage(selectAll: boolean): void {
    this.pagedResult.items.forEach((item) => {
      if (selectAll) {
        this.selectedKeys.add(item.tiendaCadenaKey);
        return;
      }

      this.selectedKeys.delete(item.tiendaCadenaKey);
    });
  }

  protected selectedPreview(): string[] {
    return Array.from(this.selectedKeys).sort().slice(0, 6);
  }

  protected selectedFechasPreview(): string[] {
    return Array.from(this.selectedFechas).sort().slice(0, 6);
  }

  protected parsedManualFechas(): string[] {
    return this.parseDateBatchValues(this.optionsForm.value.fechasTexto);
  }

  protected canSubmit(): boolean {
    return this.selectedKeys.size > 0;
  }

  protected clearSelection(): void {
    this.selectedKeys.clear();
  }

  protected toggleAllFechas(selectAll: boolean): void {
    if (selectAll) {
      this.fechasDisponibles.forEach((item) => this.selectedFechas.add(item.fecha));
      return;
    }

    this.selectedFechas.clear();
  }

  protected submit(): void {
    const tiendaCadenaKeys = Array.from(this.selectedKeys);
    if (!this.canSubmit()) {
      return;
    }

    const fechasManual = this.parseDateBatchValues(this.optionsForm.value.fechasTexto);
    const fechas = Array.from(new Set([...Array.from(this.selectedFechas), ...fechasManual])).sort();

    this.saved.emit({
      tiendaCadenaKeys,
      fechas,
      replicarSkusExistentes: !!this.optionsForm.value.replicarSkusExistentes
    });
  }

  protected close(): void {
    this.closed.emit();
  }

  private parseDateBatchValues(value: string | null | undefined): string[] {
    return (value ?? '')
      .split(/[\n,;]+/)
      .map((item) => item.trim())
      .filter((item) => /^\d{4}-\d{2}-\d{2}$/.test(item));
  }

  private cleanFilter(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }
}
