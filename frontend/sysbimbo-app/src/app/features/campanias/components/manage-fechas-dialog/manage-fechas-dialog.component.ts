import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AddCampaniaFechasPayload } from '../../../../core/models/campania-operacion.model';
import { CampaniaFecha } from '../../../../core/models/campania-fecha.model';
import { CampaniaTienda } from '../../../../core/models/campania-tienda.model';

@Component({
  selector: 'app-manage-fechas-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './manage-fechas-dialog.component.html',
  styleUrl: './manage-fechas-dialog.component.css'
})
export class ManageFechasDialogComponent implements OnChanges {
  private readonly fb = new FormBuilder();

  @Input({ required: true }) fechasActuales: CampaniaFecha[] = [];
  @Input({ required: true }) tiendasDisponibles: CampaniaTienda[] = [];
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<AddCampaniaFechasPayload>();

  protected readonly optionsForm = this.fb.group({
    fechasTexto: ['', [Validators.required]],
    search: [''],
    aplicarATodasLasTiendas: [true, [Validators.required]],
    replicarSkusExistentes: [true, [Validators.required]]
  });

  protected readonly selectedKeys = signal(new Set<string>());

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['tiendasDisponibles']) {
      this.selectedKeys.set(new Set(this.tiendasDisponibles.map((item) => item.tiendaCadenaKey)));
    }
  }

  protected filteredTiendas(): CampaniaTienda[] {
    const search = (this.optionsForm.value.search ?? '').trim().toLowerCase();

    return this.tiendasDisponibles.filter((item) => {
      if (!search) {
        return true;
      }

      return [
        item.tiendaCadenaKey,
        item.nombreTiendaBimbo ?? '',
        item.nombreTienda ?? '',
        item.cadena ?? '',
        item.region ?? ''
      ].some((value) => value.toLowerCase().includes(search));
    });
  }

  protected toggleTienda(key: string): void {
    const current = new Set(this.selectedKeys());
    if (current.has(key)) {
      current.delete(key);
    } else {
      current.add(key);
    }

    this.selectedKeys.set(current);
  }

  protected isSelected(key: string): boolean {
    return this.selectedKeys().has(key);
  }

  protected selectFiltered(selectAll: boolean): void {
    const current = new Set(this.selectedKeys());

    this.filteredTiendas().forEach((item) => {
      if (selectAll) {
        current.add(item.tiendaCadenaKey);
        return;
      }

      current.delete(item.tiendaCadenaKey);
    });

    this.selectedKeys.set(current);
  }

  protected selectedPreview(): string[] {
    return Array.from(this.selectedKeys()).sort().slice(0, 6);
  }

  protected parsedFechas(): string[] {
    return this.parseDateBatchValues(this.optionsForm.value.fechasTexto);
  }

  protected canSubmit(): boolean {
    if (this.parsedFechas().length === 0) {
      return false;
    }

    return !!this.optionsForm.value.aplicarATodasLasTiendas || this.selectedKeys().size > 0;
  }

  protected clearSelection(): void {
    this.selectedKeys.set(new Set<string>());
  }

  protected submit(): void {
    if (this.optionsForm.invalid) {
      this.optionsForm.markAllAsTouched();
      return;
    }

    const fechas = this.parsedFechas();
    if (!this.canSubmit()) {
      return;
    }

    const aplicarATodasLasTiendas = !!this.optionsForm.value.aplicarATodasLasTiendas;
    const tiendaCadenaKeys = aplicarATodasLasTiendas ? [] : Array.from(this.selectedKeys()).sort();

    this.saved.emit({
      fechas,
      tiendaCadenaKeys,
      aplicarATodasLasTiendas,
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
}
