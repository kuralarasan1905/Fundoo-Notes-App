import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AuthGuardService } from './services/auth-guard.service';
import { NotesComponent } from './components/notes/notes.component';
import { RemindersComponent } from './components/reminders/reminders.component';
import { EditLabelsComponent } from './components/edit-labels/edit-labels.component';
import { ArchiveComponent } from './components/archive/archive.component';
import { TrashComponent } from './components/trash/trash.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'signup',
    loadComponent: () =>
      import('./pages/register/register.component').then(
        (m) => m.RegisterComponent
      ),
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
    canActivate: [AuthGuardService],
    children: [
      { path: '', redirectTo: 'notes', pathMatch: 'full' },
      {
        path: 'notes',
        loadComponent: () =>
          import('./components/notes/notes.component').then(
            (m) => m.NotesComponent
          ),
      },
      {
        path: 'reminders',
        loadComponent: () =>
          import('./components/reminders/reminders.component').then(
            (m) => m.RemindersComponent
          ),
      },
      {
        path: 'edit-labels',
        loadComponent: () =>
          import('./components/edit-labels/edit-labels.component').then(
            (m) => m.EditLabelsComponent
          ),
      },
      {
        path: 'archive',
        loadComponent: () =>
          import('./components/archive/archive.component').then(
            (m) => m.ArchiveComponent
          ),
      },
      {
        path: 'trash',
        loadComponent: () =>
          import('./components/trash/trash.component').then(
            (m) => m.TrashComponent
          ),
      },
    ],
  },
];
