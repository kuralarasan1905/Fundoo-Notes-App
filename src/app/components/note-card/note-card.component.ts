import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Note } from 'src/app/model/note';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { IconsComponent } from '../icons/icons.component';

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
  ],
  templateUrl: './note-card.component.html',
  styleUrls: ['./note-card.component.scss'],
})
export class NoteCardComponent {
  @Input() note!: Note;
  @Output() editNote = new EventEmitter<{
    noteId: string;
    title: string;
    description: string;
    color: string;
  }>();
  @Output() deleteNote = new EventEmitter<string>();

  isEditing = false;
  editTitle = '';
  editDescription = '';
  noteColor: string = '#202124';
  hover: boolean = false;

  @Output() archiveNote = new EventEmitter<string>();

  archiveThisNote() {
    this.archiveNote.emit(this.note.id);
  }

  onEdit(): void {
    this.isEditing = true;
    this.editTitle = this.note.title;
    this.editDescription = this.note.description;
    this.noteColor = this.note.color || '#202124';
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

  onDelete(): void {
    if (this.note.id) {
      this.deleteNote.emit(this.note.id);
    }
  }
}
