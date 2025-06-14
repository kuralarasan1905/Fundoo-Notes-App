import { Component, Inject } from '@angular/core';
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
export class NoteDialogComponent {
  title: string;
  description: string;
  noteColor: string;

  constructor(
    public dialogRef: MatDialogRef<NoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public note: Note,
    private noteService: NoteService
  ) {
    this.title = note.title;
    this.description = note.description;
    this.noteColor = note.color || '#202124';
  }

  onClose() {
    if (!this.title.trim() && !this.description.trim()) {
      this.dialogRef.close();
      return;
    }

    this.note.color = this.noteColor;

    const payload = {
      noteId: this.note.id,
      title: this.title.trim(),
      description: this.description.trim(),
      color: this.noteColor,
    };

    this.noteService.updateNotes(payload).subscribe({
      next: () => this.dialogRef.close(payload),
      error: (err) => console.error('Failed to update note', err),
    });
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
      error: (err) => console.error('Failed to change color in dialog', err),
    });
  }
}
