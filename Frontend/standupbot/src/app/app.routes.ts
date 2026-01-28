import { Routes } from '@angular/router';
import { authGuard, noAuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/pages/login-page/login-page').then(m => m.LoginPageComponent),
    canActivate: [noAuthGuard]
  },
  {
    path: '',
    loadComponent: () => import('./features/dashboard/pages/dashboard-page/dashboard-page').then(m => m.DashboardPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'standup',
    loadComponent: () => import('./features/standup/pages/today-standup-page/today-standup-page').then(m => m.TodayStandupPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'history',
    loadComponent: () => import('./features/standup/pages/history-page/history-page').then(m => m.HistoryPageComponent),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
