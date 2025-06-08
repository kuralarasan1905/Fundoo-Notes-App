import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { IconsComponent } from '../icons/icons.component';
import { Note } from 'src/app/model/note';
import { NoteCardComponent } from '../note-card/note-card.component';

@Component({
  selector: 'app-notes',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    IconsComponent,
    NoteCardComponent,
  ],
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss'],
})
export class NotesComponent {
  notes: FormGroup;
  isExpanded = false;
  noteList: Note[] = [];

  constructor(private fb: FormBuilder) {
    this.notes = this.fb.group({
      title: [''],
      description: [''],
    });
  }

  expandForm(): void {
    this.isExpanded = true;
  }

  onSubmit(): void {
    const formValue = this.notes.value;

    if (formValue.title?.trim() || formValue.description?.trim()) {
      const newNote: Note = {
        id: Date.now().toString(),
        title: formValue.title.trim(),
        description: formValue.description.trim(),
      };

      this.noteList.unshift(newNote);
      this.notes.reset();
      this.isExpanded = false;
    } else {
      this.isExpanded = false;
    }
  }

  closeForm(): void {
    this.isExpanded = false;
    this.notes.reset();
  }

  onEditNote(updated: { noteId: string; title: string; description: string }) {
    const index = this.noteList.findIndex((n) => n.id === updated.noteId);
    if (index > -1) {
      this.noteList[index] = {
        ...this.noteList[index],
        title: updated.title,
        description: updated.description,
      };
    }
  }

  onDeleteNote(noteId: string) {
    this.noteList = this.noteList.filter((n) => n.id !== noteId);
  }
}
