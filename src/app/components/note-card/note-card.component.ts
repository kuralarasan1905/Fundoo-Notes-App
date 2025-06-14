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
  @Output() editNote = new EventEmitter<{
    noteId: string;
    title: string;
    description: string;
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
  @Output() togglePin = new EventEmitter<{ id: string; isPined: boolean }>();

  isEditing = false;
  editTitle = '';
  editDescription = '';
  noteColor: string = '#202124';
  hover: boolean = false;

  constructor(private noteService: NoteService) {}

  toggleArchiveStatus(isArchived: boolean) {
    this.archiveNote.emit({ id: this.note.id, isArchived });
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

    const payload = {
      noteIdList: [this.note.id],
      color: event.color,
    };

    this.noteService.changeColor(payload).subscribe({
      next: () => {
        this.note.color = event.color;
      },
      error: (err) => console.error('Failed to change color in card', err),
    });
  }

  onSave(): void {
    if (this.editTitle.trim() && this.editDescription.trim() && this.note.id) {
      this.editNote.emit({
        noteId: this.note.id,
        title: this.editTitle.trim(),
        description: this.editDescription.trim(),
        color: this.noteColor,
      });

      this.isEditing = false;
    }
  }

  onCancel(): void {
    this.isEditing = false;
    this.editTitle = '';
    this.editDescription = '';
  }
}
