import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { LabelService } from 'src/app/services/label_service/label.service';
import { Label } from 'src/app/model/label';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatTooltipModule, RouterModule],
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],
})
export class SidenavComponent implements OnInit {
  @Input() isExpanded: boolean = false;
  selectedLabel: string = '';
  hoverLabel: string = '';
  labels: Label[] = [];

  navItems = [
    { icon: 'lightbulb_outline', label: 'Notes', route: 'notes' },
    { icon: 'notifications', label: 'Reminders', route: 'reminders' },
    { icon: 'edit', label: 'Edit labels', route: 'edit-labels' },
    { icon: 'archive', label: 'Archive', route: 'archive' },
    { icon: 'delete_outline', label: 'Trash', route: 'trash' },
  ];

  constructor(private router: Router, private labelService: LabelService) {}

  ngOnInit(): void {
    this.updateSelectedLabel(this.router.url);
    this.loadLabels();

    // Subscribe to router events
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event) => {
        const url = (event as NavigationEnd).urlAfterRedirects;
        this.updateSelectedLabel(url);
      });

    // Subscribe to label updates to refresh the sidebar
    this.labelService.labelsUpdated$.subscribe(() => {
      console.log(' Labels updated, refreshing sidenav...');
      this.loadLabels();
    });
  }

  loadLabels(): void {
    this.labelService.getAllLabels().subscribe({
      next: (response: any) => {
        console.log('Sidenav Labels API response:', response);
        let labelsArray: any[] = [];

        // Handle different possible response formats
        if (response.data?.details) {
          labelsArray = response.data.details;
        } else if (response.data) {
          labelsArray = response.data;
        } else if (Array.isArray(response)) {
          labelsArray = response;
        }

        // Map backend response to frontend format
        this.labels = labelsArray.map(label => ({
          ...label,
          label: label.name || label.label, // Map 'name' to 'label' for display
          name: label.name || label.label   // Keep both for compatibility
        }));
      },
      error: (error) => {
        console.error('Error loading labels:', error);
        this.labels = [];
      }
    });
  }

  updateSelectedLabel(url: string) {
    const urlParts = url.split('/');
    const path = urlParts[urlParts.length - 1];

    // Check if it's a label-specific route (/dashboard/label/{labelId})
    if (urlParts.includes('label') && urlParts.length >= 4) {
      const labelId = urlParts[urlParts.length - 1];
      console.log('Detected label route with ID:', labelId, 'Type:', typeof labelId);
      console.log('Available labels:', this.labels.map(l => ({ id: l.id, idType: typeof l.id, name: l.name || l.label })));

      const label = this.labels.find(l => {
        console.log(' Comparing:', l.id, 'Type:', typeof l.id, 'with:', labelId);
        return l.id === labelId ||
               String(l.id) === String(labelId) ||
               Number(l.id) === Number(labelId);
      });

      if (label) {
        this.selectedLabel = label.label || label.name || '';
        console.log(' Selected label:', this.selectedLabel);
        return;
      } else {
        console.warn(' Label not found in sidebar for ID:', labelId);
      }
    }

    // Check for legacy query parameter format (backward compatibility)
    const queryParams = new URLSearchParams(url.split('?')[1] || '');
    const labelId = queryParams.get('label');
    const labelName = queryParams.get('labelName');

    if (labelId && path === 'notes') {
      // Use labelName from query params if available, otherwise find in labels array
      if (labelName) {
        this.selectedLabel = labelName;
        return;
      }

      const label = this.labels.find(l => l.id === labelId);
      if (label) {
        this.selectedLabel = label.label || label.name || '';
        return;
      }
    }

    // Check regular navigation items
    const found = this.navItems.find((item) => item.route === path);
    if (found) {
      this.selectedLabel = found.label;
    }
  }

  select(label: string, route: string) {
    this.selectedLabel = label;
    this.router.navigate(['/dashboard', route]);
  }

  selectLabel(label: Label) {
    this.selectedLabel = label.label || label.name || '';
    // Navigate to the dedicated label-specific route
    console.log('Navigating to label:', {
      id: label.id,
      name: label.label || label.name,
      idType: typeof label.id,
      fullLabel: label
    });
    this.router.navigate(['/dashboard/label', label.id]);
  }

  setHover(label: string) {
    this.hoverLabel = label;
  }

  clearHover() {
    this.hoverLabel = '';
  }
}
