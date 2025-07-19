import { Routes } from '@angular/router';
import { AuthGuardService } from './services/auth-guard.service';


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
        path: 'label/:labelId',
        loadComponent: () =>
          import('./components/label-notes/label-notes.component').then(
            (m) => m.LabelNotesComponent
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
      {
        path: 'account',
        loadComponent: () =>
          import('./components/account/account.component').then(
            (m) => m.AccountComponent
          ),
      },
    ],
  },
];
