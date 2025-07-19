import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LabelService } from 'src/app/services/label_service/label.service';
import { Label } from 'src/app/model/label';

@Component({
  selector: 'app-edit-labels',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatTooltipModule
  ],
  templateUrl: './edit-labels.component.html',
  styleUrls: ['./edit-labels.component.scss']
})
export class EditLabelsComponent implements OnInit {
  labels: Label[] = [];
  newLabelName: string = '';
  editingLabelId: string | null = null;
  editingLabelName: string = '';
  isCreatingNew: boolean = false;

  constructor(private labelService: LabelService) {}

  ngOnInit(): void {
    this.loadLabels();
  }

  loadLabels(): void {
    this.labelService.getAllLabels().subscribe({
      next: (response: any) => {
        console.log('Labels API response:', response);
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
      error: (error: any) => {
        console.error('Error loading labels:', error);
        this.labels = [];
      }
    });
  }

  startCreatingLabel(): void {
    this.isCreatingNew = true;
    this.newLabelName = '';
  }

  cancelCreateLabel(): void {
    this.isCreatingNew = false;
    this.newLabelName = '';
  }

  createLabel(): void {
    if (this.newLabelName.trim()) {
      console.log('Creating label:', this.newLabelName.trim());
      this.labelService.createLabel({ label: this.newLabelName.trim() }).subscribe({
        next: (response: any) => {
          console.log(' Label created successfully:', response);
          this.loadLabels();
          this.cancelCreateLabel();
        },
        error: (error) => {
          console.error(' Error creating label:', error);
          console.error('Error details:', {
            status: error.status,
            statusText: error.statusText,
            message: error.message,
            url: error.url,
            error: error.error
          });

          // Show user-friendly error message
          if (error.status === 0) {
            alert('Network error: Please check if the backend server is running.');
          } else if (error.status === 401) {
            alert('Authentication error: Please log in again.');
          } else if (error.status === 400) {
            alert('Invalid label data. Please check your input.');
          } else {
            alert(`Failed to create label: ${error.message || 'Unknown error'}`);
          }
        }
      });
    }
  }

  startEditingLabel(label: Label): void {
    this.editingLabelId = label.id;
    this.editingLabelName = label.label || label.name || '';
  }

  cancelEditLabel(): void {
    this.editingLabelId = null;
    this.editingLabelName = '';
  }

  updateLabel(): void {
    if (this.editingLabelId && this.editingLabelName.trim()) {
      console.log('Updating label:', { id: this.editingLabelId, name: this.editingLabelName.trim() });
      this.labelService.updateLabel({
        id: this.editingLabelId,
        label: this.editingLabelName.trim()
      }).subscribe({
        next: (response: any) => {
          console.log(' Label updated successfully:', response);
          this.loadLabels();
          this.cancelEditLabel();
        },
        error: (error) => {
          console.error(' Error updating label:', error);
          console.error('Error details:', {
            status: error.status,
            statusText: error.statusText,
            message: error.message,
            url: error.url,
            error: error.error
          });

          // Show user-friendly error message
          if (error.status === 0) {
            alert('Network error: Please check if the backend server is running.');
          } else if (error.status === 401) {
            alert('Authentication error: Please log in again.');
          } else if (error.status === 404) {
            alert('Label not found. It may have been deleted.');
          } else if (error.status === 400) {
            alert('Invalid label data. Please check your input.');
          } else {
            alert(`Failed to update label: ${error.message || 'Unknown error'}`);
          }
        }
      });
    }
  }

  deleteLabel(labelId: string): void {
    console.log('Deleting label:', labelId);
    this.labelService.deleteLabel(labelId).subscribe({
      next: (response: any) => {
        console.log(' Label deleted successfully:', response);
        this.loadLabels();
      },
      error: (error) => {
        console.error(' Error deleting label:', error);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url,
          error: error.error
        });

        // Show user-friendly error message
        if (error.status === 0) {
          alert('Network error: Please check if the backend server is running.');
        } else if (error.status === 401) {
          alert('Authentication error: Please log in again.');
        } else if (error.status === 404) {
          alert('Label not found. It may have been already deleted.');
        } else {
          alert(`Failed to delete label: ${error.message || 'Unknown error'}`);
        }
      }
    });
  }

  onKeyPress(event: KeyboardEvent, action: 'create' | 'edit'): void {
    if (event.key === 'Enter') {
      if (action === 'create') {
        this.createLabel();
      } else if (action === 'edit') {
        this.updateLabel();
      }
    } else if (event.key === 'Escape') {
      if (action === 'create') {
        this.cancelCreateLabel();
      } else if (action === 'edit') {
        this.cancelEditLabel();
      }
    }
  }
}
