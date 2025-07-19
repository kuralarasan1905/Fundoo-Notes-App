import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LabelService } from 'src/app/services/label_service/label.service';
import { Label } from 'src/app/model/label';
import { Note } from 'src/app/model/note';

@Component({
  selector: 'app-label-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatTooltipModule
  ],
  templateUrl: './label-dialog.component.html',
  styleUrls: ['./label-dialog.component.scss']
})
export class LabelDialogComponent implements OnInit {
  allLabels: Label[] = [];
  noteLabels: Label[] = [];
  newLabelName: string = '';
  searchText: string = '';

  constructor(
    public dialogRef: MatDialogRef<LabelDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { note: Note },
    private labelService: LabelService
  ) {
    this.noteLabels = data.note.noteLabels || [];
  }

  ngOnInit(): void {
    this.loadLabels();
  }

  loadLabels(): void {
    this.labelService.getAllLabels().subscribe({
      next: (response: any) => {
        console.log('Label Dialog API response:', response);
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
        this.allLabels = labelsArray.map(label => ({
          ...label,
          label: label.name || label.label, // Map 'name' to 'label' for display
          name: label.name || label.label   // Keep both for compatibility
        }));
      },
      error: (error) => {
        console.error('Error loading labels:', error);
        this.allLabels = [];
      }
    });
  }

  get filteredLabels(): Label[] {
    if (!this.searchText.trim()) {
      return this.allLabels;
    }
    return this.allLabels.filter(label => {
      const labelText = label.label || label.name || '';
      return labelText.toLowerCase().includes(this.searchText.toLowerCase());
    });
  }

  get shouldShowCreateLabel(): boolean {
    if (!this.searchText.trim()) {
      return false;
    }
    return !this.filteredLabels.some(label => {
      const labelText = label.label || label.name || '';
      return labelText.toLowerCase() === this.searchText.toLowerCase();
    });
  }

  isLabelSelected(label: Label): boolean {
    return this.noteLabels.some(nl => nl.id === label.id);
  }

  toggleLabel(label: Label): void {
    const isSelected = this.isLabelSelected(label);

    if (isSelected) {
      // Remove label from note
      console.log('Removing label from note:', { noteId: this.data.note.id, labelId: label.id });
      this.labelService.removeLabelFromNote({
        noteId: this.data.note.id,
        labelId: label.id
      }).subscribe({
        next: (response) => {
          console.log(' Label removed successfully:', response);
          this.noteLabels = this.noteLabels.filter(nl => nl.id !== label.id);

          // Update the note object to reflect the change
          if (this.data.note.noteLabels) {
            this.data.note.noteLabels = this.data.note.noteLabels.filter(nl => nl.id !== label.id);
          }

          // Show success feedback
          this.showFeedback(`Removed label "${label.name || label.label}"`);
        },
        error: (error) => {
          console.error(' Error removing label:', error);
          this.showFeedback('Failed to remove label', true);
        }
      });
    } else {
      // Add label to note
      console.log('Adding label to note:', { noteId: this.data.note.id, labelId: label.id });
      this.labelService.addLabelToNote({
        noteId: this.data.note.id,
        labelId: label.id
      }).subscribe({
        next: (response) => {
          console.log(' Label added successfully:', response);
          this.noteLabels.push(label);

          // Update the note object to reflect the change
          if (!this.data.note.noteLabels) {
            this.data.note.noteLabels = [];
          }
          this.data.note.noteLabels.push(label);

          // Show success feedback
          this.showFeedback(`Added label "${label.name || label.label}"`);
        },
        error: (error) => {
          console.error(' Error adding label:', error);
          this.showFeedback('Failed to add label', true);
        }
      });
    }
  }

  // Show user feedback for label operations
  private showFeedback(message: string, isError: boolean = false): void {
    // You can implement a toast notification here
    // For now, we'll just log it
    if (isError) {
      console.error(' User Feedback:', message);
    } else {
      console.log(' User Feedback:', message);
    }

    // TODO: Implement actual toast notification or snackbar
    // Example: this.snackBar.open(message, 'Close', { duration: 3000 });
  }

  createNewLabel(): void {
    const labelName = this.searchText.trim() || this.newLabelName.trim();
    if (labelName) {
      console.log('Creating new label:', labelName);
      this.labelService.createLabel({ label: labelName }).subscribe({
        next: (response: any) => {
          console.log(' Label created successfully:', response);

          // Reload labels to get the new label
          this.loadLabels();

          // Auto-select the newly created label for the current note
          if (response && response.id) {
            const newLabel: Label = {
              id: response.id,
              name: response.name || labelName,
              label: response.name || labelName
            };

            // Add the new label to the current note
            this.labelService.addLabelToNote({
              noteId: this.data.note.id,
              labelId: newLabel.id
            }).subscribe({
              next: () => {
                console.log(' New label automatically added to note');
                this.noteLabels.push(newLabel);

                // Update the note object
                if (!this.data.note.noteLabels) {
                  this.data.note.noteLabels = [];
                }
                this.data.note.noteLabels.push(newLabel);

                this.showFeedback(`Created and added label "${newLabel.name}"`);
              },
              error: (error) => {
                console.error(' Error adding new label to note:', error);
                this.showFeedback('Label created but failed to add to note', true);
              }
            });
          }

          // Clear input fields
          this.searchText = '';
          this.newLabelName = '';
        },
        error: (error) => {
          console.error(' Error creating label:', error);
          this.showFeedback('Failed to create label', true);
        }
      });
    }
  }

  // Bulk update labels for the note
  updateNoteLabels(labelIds: string[]): void {
    console.log('Bulk updating note labels:', { noteId: this.data.note.id, labelIds });

    this.labelService.updateNoteLabels({
      noteId: this.data.note.id,
      labelIds: labelIds
    }).subscribe({
      next: (response) => {
        console.log(' Note labels updated successfully:', response);

        // Update local state
        this.noteLabels = this.allLabels.filter(label => labelIds.includes(label.id));
        this.data.note.noteLabels = [...this.noteLabels];

        this.showFeedback('Labels updated successfully');
      },
      error: (error) => {
        console.error(' Error updating note labels:', error);
        this.showFeedback('Failed to update labels', true);
      }
    });
  }

  onClose(): void {
    this.dialogRef.close(this.noteLabels);
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.createNewLabel();
    }
  }
}
