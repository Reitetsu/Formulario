import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { ShellComponent } from './layout/shell.component';
import { SkusPageComponent } from './features/skus/skus-page.component';
import { TiendasPageComponent } from './features/tiendas/tiendas-page.component';
import { CampaniasPageComponent } from './features/campanias/campanias-page.component';
import { CuotasPageComponent } from './features/cuotas/cuotas-page.component';
import { ProgramacionesPageComponent } from './features/programaciones/programaciones-page.component';
import { HabilitarTiendaComponent } from './features/habilitar-tienda/habilitar-tienda.component';
import { MaterialesPageComponent } from './features/materiales/materiales-page.component';

export const routes: Routes = [
  { path: 'canjes_Agosto', component: HabilitarTiendaComponent },
  { path: 'habilitar-tienda', redirectTo: 'canjes_Agosto', pathMatch: 'full' },
  { path: '', redirectTo: 'canjes_Agosto', pathMatch: 'full' },
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: 'home', component: HomeComponent },
      { path: 'tiendas', component: TiendasPageComponent },
      { path: 'materiales', component: MaterialesPageComponent },
      { path: 'skus', component: SkusPageComponent },
      { path: 'campanias', component: CampaniasPageComponent },
      { path: 'cuotas', component: CuotasPageComponent },
      { path: 'programaciones', component: ProgramacionesPageComponent }
    ]
  },
  { path: '**', redirectTo: 'canjes_Agosto' }
];
