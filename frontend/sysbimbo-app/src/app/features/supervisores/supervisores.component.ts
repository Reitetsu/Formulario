import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-supervisores',
  standalone: true,
  imports: [AsyncPipe, RouterLink],
  templateUrl: './supervisores.component.html',
  styleUrl: './supervisores.component.css'
})
export class SupervisoresComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user$ = this.authService.currentUser$;
  protected loggingOut = false;

  protected logout(): void {
    if (this.loggingOut) return;
    this.loggingOut = true;
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/canjes_Agosto']),
      error: () => {
        this.loggingOut = false;
      }
    });
  }
}
