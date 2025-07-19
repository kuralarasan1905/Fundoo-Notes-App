import { Component, Inject, OnInit, AfterViewInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Note } from 'src/app/model/note';
import { IconsComponent } from '../icons/icons.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NoteService } from 'src/app/services/note_service/note.service';

@Component({
  selector: 'app-note-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    IconsComponent,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './note-dialog.component.html',
  styleUrls: ['./note-dialog.component.scss'],
})
export class NoteDialogComponent implements AfterViewInit {
  title: string;
  content: string; // Changed from 'description' to 'content'
  noteColor: string;

  constructor(
    public dialogRef: MatDialogRef<NoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public note: Note,
    private noteService: NoteService
  ) {
    this.title = note.title;
    this.content = note.previewContent; // Changed from 'description' to 'content'
    this.noteColor = note.color || '#ffffff';
  }

  ngAfterViewInit() {
    // Apply the note color to the dialog container after view is initialized
    setTimeout(() => {
      this.applyColorToDialog(this.noteColor);
    }, 150);
  }

  onClose() {
    // Save note immediately if there's content, otherwise just close
    if (!this.title.trim() && !this.content.trim()) {
      this.dialogRef.close();
      return;
    }

    this.note.color = this.noteColor;

    // Google Keep style: Use RESTful PUT for single note update
    const payload = {
      title: this.title.trim(),
      content: this.content.trim(), // Changed from 'description' to 'content'
      color: this.noteColor,
    };

    // Save in background and close immediately
    this.noteService.updateNote(this.note.id, payload).subscribe({
      next: (response) => {
        console.log(' Note saved successfully:', response);
      },
      error: (err) => {
        console.error(' Failed to save note:', err);
      },
    });

    // Close dialog immediately without waiting for save
    this.dialogRef.close({ ...payload, noteId: this.note.id });
  }

  private applyColorToDialog(color: string) {
    // Try multiple selectors to find the dialog container
    const selectors = [
      '.mat-mdc-dialog-container',
      '.mat-dialog-container',
      '.cdk-dialog-container',
      '[role="dialog"]'
    ];

    let dialogContainer: HTMLElement | null = null;

    for (const selector of selectors) {
      dialogContainer = document.querySelector(selector) as HTMLElement;
      if (dialogContainer) {
        console.log(' Found dialog container with selector:', selector);
        break;
      }
    }

    if (dialogContainer) {
      dialogContainer.style.backgroundColor = color;
      dialogContainer.style.setProperty('background-color', color, 'important');
      console.log(' Applied color to dialog:', color);
    } else {
      console.warn(' Dialog container not found for color application');
    }
  }

  setColor(event: { color: string; index: number }) {
    this.noteColor = event.color;

    // Apply the new color to the dialog container immediately
    this.applyColorToDialog(event.color);

    console.log(' Changing note color in dialog:', {
      noteId: this.note.id,
      newColor: event.color,
      colorIndex: event.index
    });

    // Update note color immediately for better UX
    this.note.color = event.color;

    this.noteService.changeNoteColor(this.note.id, event.color).subscribe({
      next: (response) => {
        console.log(' Color changed successfully in dialog:', response);
      },
      error: (err) => {
        console.error(' Failed to change color in dialog:', err);
        console.error('Error details:', {
          status: err.status,
          message: err.message,
          url: err.url
        });
        // Revert color on error
        this.note.color = this.noteColor;
        this.applyColorToDialog(this.noteColor);
      },
    });
  }

  addReminder(date: string) {
    this.noteService
      .addUpdateReminder({
        noteIdList: [this.note.id],
        reminder: date,
      })
      .subscribe(() => {
        this.note.reminder = date;
      });
  }

  removeReminder() {
    this.noteService
      .removeReminder({
        noteIdList: [this.note.id],
      })
      .subscribe(() => {
        this.note.reminder = undefined;
      });
  }

  onLabelsUpdated(labels: any): void {
    this.note.noteLabels = labels;
  }

  onToggleArchive(isArchived: boolean): void {
    // Debug: Check note data
    console.log(' Note data:', this.note);
    console.log(' Note ID:', this.note.id);
    console.log(' Note ID type:', typeof this.note.id);

    console.log(' Attempting to toggle archive status for note:', this.note.id);
    console.log(' Full API URL will be: https://localhost:7256/api/Notes/' + this.note.id + '/archive');

    this.noteService
      .toggleArchive(this.note.id)
      .subscribe({
        next: (response) => {
          // Toggle the archive status based on current state
          this.note.isArchived = !this.note.isArchived;
          console.log(` Note archive status toggled successfully:`, response);
          console.log(` Note is now ${this.note.isArchived ? 'archived' : 'unarchived'}`);

          // Close dialog after archiving
          this.dialogRef.close({ archived: true, noteId: this.note.id });
        },
        error: (err) => {
          console.error(' Failed to toggle archive status:', err);
          console.error('Error details:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            error: err.error,
            url: err.url
          });

          // Don't close dialog on error, let user try again
          alert(`Failed to toggle archive status. Please check console for details.`);
        },
      });
  }

  onMoveToTrash(): void {
    this.noteService
      .moveNoteToTrash(this.note.id)
      .subscribe({
        next: () => {
          this.note.isDeleted = true;
          console.log('Note moved to trash successfully');
          // Close dialog after moving to trash
          this.dialogRef.close({ deleted: true, noteId: this.note.id });
        },
        error: (err) => {
          console.error('Failed to move note to trash:', err);
        },
      });
  }

  // Google Keep style time formatting
  getFormattedTime(dateString: string): string {
    if (!dateString) return '';

    const date = new Date(dateString);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    const diffInHours = Math.floor(diffInMinutes / 60);
    const diffInDays = Math.floor(diffInHours / 24);

    if (diffInMinutes < 1) {
      return 'now';
    } else if (diffInMinutes < 60) {
      return `${diffInMinutes} min ago`;
    } else if (diffInHours < 24) {
      return `${diffInHours} hr ago`;
    } else if (diffInDays === 1) {
      return 'yesterday';
    } else if (diffInDays < 7) {
      return `${diffInDays} days ago`;
    } else {
      // For older dates, show the actual date
      return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
      });
    }
  }
}
