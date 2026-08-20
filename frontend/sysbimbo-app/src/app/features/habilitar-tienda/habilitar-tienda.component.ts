import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  catchError,
  combineLatest,
  concatMap,
  debounceTime,
  distinctUntilChanged,
  finalize,
  from,
  map,
  of,
  startWith,
  switchMap,
  tap
} from 'rxjs';
import { MaterialImpulsoTienda } from '../../core/models/material-impulso.model';
import { AuthUser } from '../../core/models/auth-user.model';
import { Tienda } from '../../core/models/tienda.model';
import { AuthService } from '../../core/services/auth.service';
import { MaterialesImpulsoService } from '../../core/services/materiales-impulso.service';
import { TiendasService } from '../../core/services/tiendas.service';

@Component({
  selector: 'app-habilitar-tienda',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './habilitar-tienda.component.html',
  styleUrl: './habilitar-tienda.component.css'
})
export class HabilitarTiendaComponent implements OnInit, OnDestroy {
  @ViewChild('cameraVideo') private cameraVideo?: ElementRef<HTMLVideoElement>;

  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly tiendasService = inject(TiendasService);
  private readonly materialesService = inject(MaterialesImpulsoService);
  private readonly authService = inject(AuthService);

  protected readonly marcas = ['TOTTUS', 'METRO', 'MAKRO', 'PLAZA VEA'];
  protected readonly marcaControl = new FormControl('', { nonNullable: true });
  protected readonly tiendaSearch = new FormControl('', { nonNullable: true });
  protected readonly canjesControl = new FormControl<number | null>(null, {
    validators: [Validators.required, Validators.min(0), Validators.max(1_000_000)]
  });

  protected currentUser: AuthUser | null = null;
  protected suggestions: Tienda[] = [];
  protected selectedTienda: Tienda | null = null;
  protected materials: MaterialImpulsoTienda[] = [];
  protected material: MaterialImpulsoTienda | null = null;
  protected loadingStores = false;
  protected loadingMaterial = false;
  protected uploadingPhoto = false;
  protected dropdownOpen = false;
  protected materialSearched = false;
  protected loadError = '';
  protected materialError = '';
  protected photoMessage = '';
  protected photoError = '';
  protected photoPreviewUrl = '';
  protected uploadProgress = '';
  protected cameraOpen = false;
  protected cameraStarting = false;
  protected cameraError = '';
  protected savingCanjes = false;
  protected canjesMessage = '';
  protected canjesError = '';
  private cameraStream: MediaStream | null = null;

  ngOnInit(): void {
    this.authService.getSession()
      .pipe(
        catchError(() => of(null)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(user => {
        this.currentUser = user;
        this.cdr.markForCheck();
      });

    this.marcaControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.resetStore());

    combineLatest([
      this.marcaControl.valueChanges.pipe(startWith(''), distinctUntilChanged()),
      this.tiendaSearch.valueChanges.pipe(startWith(''), distinctUntilChanged())
    ])
      .pipe(
        debounceTime(250),
        tap(([, term]) => {
          this.loadError = '';
          this.materials = [];
          this.material = null;
          this.materialSearched = false;
          this.photoMessage = '';

          if (this.selectedTienda && term !== this.storeLabel(this.selectedTienda)) {
            this.selectedTienda = null;
          }
        }),
        switchMap(([marca, term]) => {
          if (!marca) {
            this.loadingStores = false;
            return of({ items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
          }

          this.loadingStores = true;
          return this.tiendasService
            .list({
              marca,
              nombre: term.trim() || undefined,
              soloConMaterialActivo: true,
              pageNumber: 1,
              pageSize: 50
            })
            .pipe(
              catchError((error: unknown) => {
                this.loadError = this.errorMessage(error, 'No fue posible cargar las tiendas.');
                return of({ items: [], pageNumber: 1, pageSize: 50, totalCount: 0, totalPages: 0 });
              })
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.suggestions = result.items;
        this.loadingStores = false;
        this.dropdownOpen = Boolean(this.marcaControl.value) && !this.selectedTienda;
        this.cdr.markForCheck();
      });
  }

  ngOnDestroy(): void {
    this.stopCameraStream();
    if (this.photoPreviewUrl) URL.revokeObjectURL(this.photoPreviewUrl);
  }

  protected openDropdown(): void {
    if (this.marcaControl.value) {
      this.dropdownOpen = true;
    }
  }

  protected closeDropdown(): void {
    window.setTimeout(() => (this.dropdownOpen = false), 160);
  }

  protected selectTienda(tienda: Tienda): void {
    this.selectedTienda = tienda;
    this.materials = [];
    this.material = null;
    this.materialSearched = false;
    this.photoMessage = '';
    this.photoError = '';
    this.resetDailyExchanges();
    this.tiendaSearch.setValue(this.storeLabel(tienda), { emitEvent: false });
    this.dropdownOpen = false;
    this.cdr.markForCheck();

    queueMicrotask(() => this.searchMaterial());
  }

  protected clearSelection(): void {
    this.closeCamera();
    this.selectedTienda = null;
    this.materials = [];
    this.material = null;
    this.materialSearched = false;
    this.photoMessage = '';
    this.photoError = '';
    this.cameraError = '';
    this.resetDailyExchanges();
    if (this.photoPreviewUrl) {
      URL.revokeObjectURL(this.photoPreviewUrl);
      this.photoPreviewUrl = '';
    }
    this.tiendaSearch.setValue('');
    this.dropdownOpen = Boolean(this.marcaControl.value);
  }

  protected searchMaterial(): void {
    if (!this.selectedTienda) {
      return;
    }

    this.loadingMaterial = true;
    this.materialSearched = false;
    this.materialError = '';
    this.photoMessage = '';

    this.materialesService
      .getByTienda(this.selectedTienda.tiendaCadenaKey)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loadingMaterial = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (materials) => {
          this.materials = materials ?? [];
          this.material = this.materials.length === 1 ? this.materials[0] : null;
          this.syncDailyExchanges();
          this.materialSearched = true;
          this.cdr.markForCheck();
          this.scrollToMaterial();
        },
        error: (error: unknown) => {
          this.materials = [];
          this.material = null;
          this.materialSearched = true;
          this.materialError = this.errorMessage(error, 'No fue posible consultar el material.');
          this.cdr.markForCheck();
          this.scrollToMaterial();
        }
      });
  }

  protected selectMaterialById(event: Event): void {
    const materialId = Number((event.target as HTMLSelectElement).value);
    const selected = this.materials.find(item => item.materialImpulsoTiendaId === materialId) ?? null;
    this.selectMaterial(selected);
  }

  private selectMaterial(material: MaterialImpulsoTienda | null): void {
    this.closeCamera();
    this.material = material;
    this.photoMessage = '';
    this.photoError = '';
    this.cameraError = '';
    this.syncDailyExchanges();

    if (this.photoPreviewUrl) {
      URL.revokeObjectURL(this.photoPreviewUrl);
      this.photoPreviewUrl = '';
    }

    this.cdr.markForCheck();
  }

  protected async openCamera(): Promise<void> {
    if (!this.material || this.uploadingPhoto) {
      return;
    }

    this.cameraError = '';
    this.photoError = '';
    if (!navigator.mediaDevices?.getUserMedia) {
      this.cameraError = 'La camara requiere abrir el formulario mediante HTTPS desde el celular.';
      this.cdr.markForCheck();
      return;
    }

    this.cameraOpen = true;
    this.cameraStarting = true;
    this.cdr.detectChanges();

    try {
      this.cameraStream = await navigator.mediaDevices.getUserMedia({
        audio: false,
        video: {
          facingMode: { ideal: 'environment' },
          width: { ideal: 1920 },
          height: { ideal: 1080 }
        }
      });

      if (!this.cameraOpen) {
        this.stopCameraStream();
        return;
      }

      const video = this.cameraVideo?.nativeElement;
      if (!video) throw new Error('No se pudo inicializar la vista de la camara.');
      video.srcObject = this.cameraStream;
      await video.play();
    } catch (error: unknown) {
      this.stopCameraStream();
      this.cameraOpen = false;
      this.cameraError = this.cameraAccessError(error);
    } finally {
      this.cameraStarting = false;
      this.cdr.markForCheck();
    }
  }

  protected get canManageCanjes(): boolean {
    return this.currentUser?.roles.some(
      role => role === 'Administrador' || role === 'Supervisor'
    ) ?? false;
  }

  protected saveDailyExchanges(): void {
    if (!this.material || !this.canManageCanjes || this.canjesControl.invalid || this.savingCanjes) {
      this.canjesControl.markAsTouched();
      return;
    }

    const cantidad = this.canjesControl.value;
    if (cantidad === null) return;

    const materialId = this.material.materialImpulsoTiendaId;
    this.savingCanjes = true;
    this.canjesMessage = '';
    this.canjesError = '';
    this.materialesService.updateDailyExchanges(materialId, cantidad)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.savingCanjes = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: result => {
          this.materials = this.materials.map(item =>
            item.materialImpulsoTiendaId === materialId
              ? { ...item, canjesHoy: result.cantidad }
              : item
          );
          if (this.material?.materialImpulsoTiendaId === materialId) {
            this.material = { ...this.material, canjesHoy: result.cantidad };
          }
          this.canjesControl.setValue(result.cantidad);
          this.canjesMessage = `Total diario actualizado: ${result.cantidad} canjes.`;
        },
        error: (error: unknown) => {
          this.canjesError = this.errorMessage(error, 'No fue posible guardar el total de canjes.');
        }
      });
  }

  protected captureCameraPhoto(): void {
    const video = this.cameraVideo?.nativeElement;
    if (!video || !video.videoWidth || !video.videoHeight || !this.material) return;

    const maxWidth = 1600;
    const scale = Math.min(1, maxWidth / video.videoWidth);
    const canvas = document.createElement('canvas');
    canvas.width = Math.round(video.videoWidth * scale);
    canvas.height = Math.round(video.videoHeight * scale);
    const context = canvas.getContext('2d');

    if (!context) {
      this.cameraError = 'No fue posible procesar la fotografia.';
      return;
    }

    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    canvas.toBlob((blob) => {
      if (!blob) {
        this.cameraError = 'No fue posible generar la fotografia.';
        this.cdr.markForCheck();
        return;
      }

      const file = new File([blob], `evidencia-${Date.now()}.jpg`, { type: 'image/jpeg' });
      this.closeCamera();
      this.uploadPhoto(file);
    }, 'image/jpeg', 0.82);
  }

  protected uploadSelectedPhotos(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';

    if (files.length > 0) {
      this.uploadPhotos(files);
    }
  }

  protected closeCamera(): void {
    this.stopCameraStream();
    this.cameraOpen = false;
    this.cameraStarting = false;
    this.cdr.markForCheck();
  }

  private uploadPhoto(file: File): void {
    this.uploadPhotos([file]);
  }

  private uploadPhotos(files: File[]): void {
    if (!this.material || this.uploadingPhoto || files.length === 0) return;

    const materialId = this.material.materialImpulsoTiendaId;
    let savedPhotos = 0;
    let failedPhotos = 0;

    this.uploadingPhoto = true;
    this.photoMessage = '';
    this.photoError = '';
    this.cameraError = '';

    from(files)
      .pipe(
        concatMap((file, index) => {
          this.uploadProgress = files.length > 1
            ? `Guardando foto ${index + 1} de ${files.length}...`
            : 'Guardando foto...';
          this.setPhotoPreview(file);
          this.cdr.markForCheck();

          return this.materialesService.savePhoto(materialId, file).pipe(
            map(result => ({ result, error: null as unknown })),
            catchError((error: unknown) => of({ result: null, error }))
          );
        }),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.uploadingPhoto = false;
          this.uploadProgress = '';
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: ({ result, error }) => {
          if (!result) {
            failedPhotos++;
            this.photoError = this.errorMessage(error, 'No fue posible guardar una de las fotografias.');
            return;
          }

          savedPhotos++;
          this.materials = this.materials.map(item =>
            item.materialImpulsoTiendaId === result.materialImpulsoTiendaId
              ? { ...item, acumulado: result.acumulado }
              : item
          );
          if (this.material?.materialImpulsoTiendaId === result.materialImpulsoTiendaId) {
            this.material = { ...this.material, acumulado: result.acumulado };
          }
          this.cdr.markForCheck();
        },
        complete: () => {
          if (savedPhotos > 0) {
            this.photoMessage = savedPhotos === 1
              ? 'Foto guardada correctamente. Se registro 1 entrega.'
              : `${savedPhotos} fotos guardadas correctamente. Se registraron ${savedPhotos} entregas.`;
          }

          if (failedPhotos > 0) {
            this.photoError = `${failedPhotos} ${failedPhotos === 1 ? 'foto no pudo' : 'fotos no pudieron'} guardarse. Las demas entregas si fueron registradas.`;
          }
          this.cdr.markForCheck();
        }
      });
  }

  private setPhotoPreview(file: File): void {
    if (this.photoPreviewUrl) {
      URL.revokeObjectURL(this.photoPreviewUrl);
    }

    this.photoPreviewUrl = URL.createObjectURL(file);
  }

  protected storeLabel(tienda: Tienda): string {
    return tienda.nombreTiendaBimbo || tienda.nombreTienda || tienda.tiendaCadenaKey;
  }

  private resetStore(): void {
    this.selectedTienda = null;
    this.materials = [];
    this.material = null;
    this.suggestions = [];
    this.materialSearched = false;
    this.photoMessage = '';
    this.photoError = '';
    this.loadError = '';
    this.resetDailyExchanges();
    this.tiendaSearch.setValue('');
    this.closeCamera();
    this.cdr.markForCheck();
  }

  private syncDailyExchanges(): void {
    this.canjesControl.setValue(this.material?.canjesHoy ?? null);
    this.canjesMessage = '';
    this.canjesError = '';
  }

  private resetDailyExchanges(): void {
    this.canjesControl.setValue(null);
    this.canjesMessage = '';
    this.canjesError = '';
    this.savingCanjes = false;
  }

  private stopCameraStream(): void {
    this.cameraStream?.getTracks().forEach(track => track.stop());
    this.cameraStream = null;
    if (this.cameraVideo?.nativeElement) this.cameraVideo.nativeElement.srcObject = null;
  }

  private cameraAccessError(error: unknown): string {
    if (error instanceof DOMException) {
      if (error.name === 'NotAllowedError') return 'Debes permitir el acceso a la camara para registrar la evidencia.';
      if (error.name === 'NotFoundError') return 'No se encontro una camara disponible en este dispositivo.';
      if (error.name === 'NotReadableError') return 'La camara esta siendo utilizada por otra aplicacion.';
    }

    return 'No fue posible abrir la camara. Verifica los permisos del navegador.';
  }

  private scrollToMaterial(): void {
    window.setTimeout(() => {
      document.querySelector('.result-panel')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }

  private errorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'No se pudo conectar con la API. Verifica que el backend este activo.';
      }

      return error.error?.message ?? error.error?.detail ?? fallback;
    }

    return fallback;
  }
}
