import { AsyncPipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { FotoMaterialResumen } from '../../core/models/material-impulso.model';
import {
  SupervisorMaterial,
  SupervisorPanel,
  SupervisorStore
} from '../../core/models/supervisor-panel.model';
import { AuthService } from '../../core/services/auth.service';
import { MaterialesImpulsoService } from '../../core/services/materiales-impulso.service';
import { SupervisorPanelService } from '../../core/services/supervisor-panel.service';

@Component({
  selector: 'app-supervisores',
  standalone: true,
  imports: [AsyncPipe, FormsModule, RouterLink],
  templateUrl: './supervisores.component.html',
  styleUrl: './supervisores.component.css'
})
export class SupervisoresComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly panelService = inject(SupervisorPanelService);
  private readonly materialsService = inject(MaterialesImpulsoService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  protected readonly user$ = this.authService.currentUser$;
  protected loggingOut = false;
  protected loading = true;
  protected errorMessage = '';
  protected panel: SupervisorPanel | null = null;
  protected editingAttendance = false;
  protected attendanceEntry = '';
  protected attendanceExit = '';
  protected savingAttendance = false;
  protected readonly exchangeDrafts: Record<number, number> = {};
  protected savingMaterialId: number | null = null;
  protected materialMessage = '';
  protected galleryOpen = false;
  protected galleryLoading = false;
  protected galleryError = '';
  protected galleryTitle = '';
  protected galleryPhotos: FotoMaterialResumen[] = [];

  ngOnInit(): void {
    this.loadPanel();
  }

  protected loadPanel(): void {
    this.loading = true;
    this.errorMessage = '';
    this.panelService.getPanel().subscribe({
      next: panel => {
        this.panel = panel;
        for (const store of panel.tiendas) {
          for (const material of store.materiales) {
            this.exchangeDrafts[material.materialImpulsoTiendaId] = material.canjesHoy;
          }
        }
        this.resetAttendanceDraft();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'No fue posible cargar la información del supervisor.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  protected editAttendance(): void {
    this.resetAttendanceDraft();
    this.editingAttendance = true;
  }

  protected cancelAttendance(): void {
    this.editingAttendance = false;
    this.resetAttendanceDraft();
  }

  protected saveAttendance(): void {
    if (!this.attendanceEntry || this.savingAttendance) return;
    this.savingAttendance = true;
    this.panelService.updateAttendance({
      horaIngreso: `${this.attendanceEntry}:00`,
      horaSalida: this.attendanceExit ? `${this.attendanceExit}:00` : null
    }).subscribe({
      next: attendance => {
        if (this.panel) this.panel.asistencia = attendance;
        this.editingAttendance = false;
        this.savingAttendance = false;
        this.resetAttendanceDraft();
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = error?.error?.message ?? 'No fue posible actualizar la asistencia.';
        this.savingAttendance = false;
        this.cdr.markForCheck();
      }
    });
  }

  protected saveExchanges(store: SupervisorStore, material: SupervisorMaterial): void {
    if (this.savingMaterialId !== null) return;
    const amount = Math.max(0, Math.trunc(Number(
      this.exchangeDrafts[material.materialImpulsoTiendaId] ?? 0
    )));
    this.exchangeDrafts[material.materialImpulsoTiendaId] = amount;
    this.savingMaterialId = material.materialImpulsoTiendaId;
    this.materialMessage = '';
    this.materialsService.updateDailyExchanges(material.materialImpulsoTiendaId, amount).subscribe({
      next: result => {
        material.canjesHoy = result.cantidad;
        store.totalCanjesHoy = store.materiales.reduce((total, item) => total + item.canjesHoy, 0);
        this.materialMessage = `${material.nombreMaterial}: canjes actualizados.`;
        this.savingMaterialId = null;
        this.cdr.markForCheck();
      },
      error: error => {
        this.materialMessage = error?.error?.message ?? 'No fue posible actualizar los canjes.';
        this.savingMaterialId = null;
        this.cdr.markForCheck();
      }
    });
  }

  protected showEvidence(store: SupervisorStore, material: SupervisorMaterial): void {
    if (material.evidenciasHoy < 1) return;
    this.galleryOpen = true;
    this.galleryLoading = true;
    this.galleryError = '';
    this.galleryPhotos = [];
    this.galleryTitle = `${store.nombreTienda} · ${material.nombreMaterial}`;
    this.materialsService.getPhotos(material.materialImpulsoTiendaId, true).subscribe({
      next: photos => {
        this.galleryPhotos = photos;
        this.galleryLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.galleryError = 'No fue posible cargar las fotografías.';
        this.galleryLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  protected closeGallery(): void {
    this.galleryOpen = false;
    this.galleryPhotos = [];
  }

  protected photoUrl(photoId: number): string {
    return this.materialsService.getPhotoUrl(photoId);
  }

  protected formatTime(value: string | null): string {
    if (!value) return 'Pendiente';
    return new Intl.DateTimeFormat('es-PE', {
      timeZone: 'America/Lima',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }).format(new Date(value));
  }

  protected formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      timeZone: 'UTC',
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    }).format(new Date(`${value}T00:00:00Z`));
  }

  private resetAttendanceDraft(): void {
    const attendance = this.panel?.asistencia;
    this.attendanceEntry = attendance ? this.toTimeInput(attendance.horaIngreso) : '08:00';
    this.attendanceExit = attendance?.horaSalida ? this.toTimeInput(attendance.horaSalida) : '';
  }

  private toTimeInput(value: string): string {
    const parts = new Intl.DateTimeFormat('en-GB', {
      timeZone: 'America/Lima',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }).formatToParts(new Date(value));
    const hour = parts.find(part => part.type === 'hour')?.value ?? '00';
    const minute = parts.find(part => part.type === 'minute')?.value ?? '00';
    return `${hour}:${minute}`;
  }

  protected logout(): void {
    if (this.loggingOut) return;
    this.loggingOut = true;
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/canjes_Agosto']),
      error: () => {
        this.loggingOut = false;
        this.cdr.markForCheck();
      }
    });
  }
}
