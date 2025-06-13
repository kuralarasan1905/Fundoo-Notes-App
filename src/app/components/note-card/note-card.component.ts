import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Note } from 'src/app/model/note';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { IconsComponent } from '../icons/icons.component';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { NoteDialogComponent } from '../note-dialog/note-dialog.component';

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

  isEditing = false;
  editTitle = '';
  editDescription = '';
  noteColor: string = '#202124';
  hover: boolean = false;

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

  setColor(color: string) {
    this.noteColor = color;
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
