import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoadingService } from '../../services/loading.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="loading-overlay" *ngIf="loading$ | async">
      <div class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p class="loading-text">Loading...</p>
      </div>
    </div>
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(255, 255, 255, 0.8);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 9999;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
    }

    .loading-text {
      margin: 0;
      font-size: 16px;
      color: #666;
      font-weight: 500;
    }

    /* Dark theme support */
    @media (prefers-color-scheme: dark) {
      .loading-overlay {
        background-color: rgba(0, 0, 0, 0.8);
      }
      
      .loading-text {
        color: #ccc;
      }
    }
  `]
})
export class LoadingSpinnerComponent {
  loading$: Observable<boolean>;

  constructor(private loadingService: LoadingService) {
    this.loading$ = this.loadingService.loading$;
  }
}
