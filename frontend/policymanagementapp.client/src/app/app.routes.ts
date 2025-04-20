import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'policies', loadComponent: () => import('./app.component').then(m => m.AppComponent) },
  { path: '**', redirectTo: '' }
]; 