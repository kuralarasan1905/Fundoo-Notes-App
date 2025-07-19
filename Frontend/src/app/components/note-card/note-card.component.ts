import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Note } from 'src/app/model/note';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { IconsComponent } from '../icons/icons.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NoteService } from 'src/app/services/note_service/note.service';
import { ReminderService } from 'src/app/services/reminder.service';
import { NotificationService } from 'src/app/services/notification.service';

@Component({
  selector: 'app-note-card',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    IconsComponent,
    MatDialogModule,
    MatTooltipModule,
  ],
  templateUrl: './note-card.component.html',
  styleUrls: ['./note-card.component.scss'],
})
export class NoteCardComponent {
  @Input() note!: Note;
  @Input() inTrash: boolean = false;
  @Input() viewMode: 'grid' | 'list' = 'grid';
  @Output() editNote = new EventEmitter<{
    noteId: string;
    title: string;
    content: string; // Changed from 'description' to 'content'
    color: string;
  }>();
  @Output() openDialog = new EventEmitter<void>();
  @Output() deleteNote = new EventEmitter<string>();
  @Output() moveToTrash = new EventEmitter<string>();
  @Output() restoreNote = new EventEmitter<string>();
  @Output() deletePermanently = new EventEmitter<string>();
  @Output() archiveNote = new EventEmitter<{
    id: string;
    isArchived: boolean;
  }>();
  @Output() togglePin = new EventEmitter<{ id: string; isPinned: boolean }>();
  @Output() refreshNotes = new EventEmitter<void>();

  isEditing = false;
  editTitle = '';
  editContent = ''; // Changed from 'editDescription' to 'editContent'
  noteColor: string = '#202124';
  hover: boolean = false;

  constructor(
    private noteService: NoteService,
    private reminderService: ReminderService,
    private notificationService: NotificationService
  ) {}

  toggleArchiveStatus(isArchived: boolean) {
    this.archiveNote.emit({ id: this.note.id, isArchived });
  }

  onPinClick(event: Event) {
    event.stopPropagation(); // Prevent event bubbling
    console.log(' Pin button clicked for note:', this.note.id);
    console.log(' Current pin status:', this.note.isPined);
    console.log(' New pin status will be:', !this.note.isPined);

    // Validate note data
    if (!this.note.id) {
      console.error(' Note ID is missing:', this.note);
      return;
    }

    const pinPayload = { id: this.note.id, isPinned: !this.note.isPined };
    console.log(' Emitting togglePin event with payload:', pinPayload);

    this.togglePin.emit(pinPayload);
  }

  onDelete(): void {
    if (this.note.id) {
      this.moveToTrash.emit(this.note.id);
    }
  }

  restoreThisNote() {
    this.restoreNote.emit(this.note.id);
  }

  deleteThisNoteForever() {
    this.deletePermanently.emit(this.note.id);
  }

  onEdit(): void {
    this.openDialog.emit();
  }

  setColor(event: { color: string; index: number }) {
    this.noteColor = event.color;

    console.log(' Changing note color:', {
      noteId: this.note.id,
      newColor: event.color,
      colorIndex: event.index
    });

    // Update the note color immediately for better UX
    this.note.color = event.color;

    this.noteService.changeNoteColor(this.note.id, event.color).subscribe({
      next: (response) => {
        console.log(' Color changed successfully in card:', response);
      },
      error: (err) => {
        console.error(' Failed to change color in card:', err);
        console.error('Error details:', {
          status: err.status,
          message: err.message,
          url: err.url
        });
        // Revert color on error
        this.note.color = this.noteColor;
      },
    });
  }

  onSave(): void {
    if (this.editTitle.trim() && this.editContent.trim() && this.note.id) {
      this.editNote.emit({
        noteId: this.note.id,
        title: this.editTitle.trim(),
        content: this.editContent.trim(), // Changed from 'description' to 'content'
        color: this.noteColor,
      });

      this.isEditing = false;
    }
  }

  onCancel(): void {
    this.isEditing = false;
    this.editTitle = '';
    this.editContent = ''; // Changed from 'editDescription' to 'editContent'
  }

  addReminder(date: string) {
    console.log(' REMINDER BUTTON CLICKED - Adding reminder to note:', this.note.id, 'Date:', date);

    // Check if user is authenticated
    const token = localStorage.getItem('token');
    if (!token) {
      console.error(' No authentication token found. User needs to log in.');
      this.notificationService.showAuthError();
      return;
    }

    console.log(' Using ReminderService to add reminder...');
    console.log(' Note ID:', this.note.id, 'Date:', date);

    // Format the reminder time for user feedback
    const formattedTime = this.reminderService.formatReminderTime(date);

    this.reminderService
      .addReminderToNote(this.note.id, date)
      .subscribe({
        next: (response) => {
          console.log(' Reminder added successfully:', response);
          this.note.reminder = date;

          // Show success notification with formatted time
          this.notificationService.showReminderSuccess('added', formattedTime);

          // Emit event to refresh notes list if needed
          this.refreshNotes.emit();
        },
        error: (error) => {
          console.error(' Failed to add reminder:', error);
          console.error(' Error status:', error.status);
          console.error(' Error message:', error.message);

          // Show appropriate error notification
          this.notificationService.showReminderError('add', error);

          if (error.status === 401) {
            console.error(' Authentication failed. User may need to log in again.');
            this.notificationService.showAuthError();
          } else if (error.status === 0) {
            this.notificationService.showNetworkError();
          }
        }
      });
  }

  removeReminder() {
    console.log(' Removing reminder from note:', this.note.id);

    // Check if user is authenticated
    const token = localStorage.getItem('token');
    if (!token) {
      console.error(' No authentication token found. User needs to log in.');
      this.notificationService.showAuthError();
      return;
    }

    console.log(' Using ReminderService to remove reminder...');

    this.reminderService
      .removeReminderFromNote(this.note.id)
      .subscribe({
        next: (response) => {
          console.log(' Reminder removed successfully:', response);
          this.note.reminder = undefined;

          // Show success notification
          this.notificationService.showReminderSuccess('removed');

          // Emit event to refresh notes list if needed
          this.refreshNotes.emit();
        },
        error: (error) => {
          console.error(' Failed to remove reminder:', error);
          console.error(' Error status:', error.status);

          // Show appropriate error notification
          this.notificationService.showReminderError('remove', error);

          if (error.status === 401) {
            console.error(' Authentication failed. User may need to log in again.');
            this.notificationService.showAuthError();
          } else if (error.status === 0) {
            this.notificationService.showNetworkError();
          }
        }
      });
  }

  onLabelsUpdated(labels: any): void {
    this.note.noteLabels = labels;
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

  getFormattedReminderTime(reminderString: string): string {
    if (!reminderString) return '';

    // Use the ReminderService for consistent formatting
    return this.reminderService.formatReminderTime(reminderString);
  }

  // Get reminder status for styling
  getReminderStatus(reminderString: string): 'upcoming' | 'overdue' | 'today' | 'none' {
    if (!reminderString) return 'none';
    return this.reminderService.getReminderStatus(reminderString);
  }

  // Check if reminder is overdue
  isReminderOverdue(reminderString: string): boolean {
    if (!reminderString) return false;
    return this.reminderService.isReminderOverdue(reminderString);
  }
}
