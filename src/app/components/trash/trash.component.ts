import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Note } from 'src/app/model/note';
import { NoteService } from 'src/app/services/note_service/note.service';
import { NoteCardComponent } from '../note-card/note-card.component';

@Component({
  selector: 'app-trash',
  standalone: true,
  imports: [CommonModule, NoteCardComponent],
  templateUrl: './trash.component.html',
  styleUrls: ['./trash.component.scss'],
})
export class TrashComponent implements OnInit {
  noteList: Note[] = [];

  constructor(private noteService: NoteService) {}

  ngOnInit() {
    this.noteService.getTrashList().subscribe({
      next: (res: any) => {
        this.noteList = res.data.data;
      },
      error: (err) => console.error('Failed to fetch trash notes:', err),
    });
  }

  onRestoreNote(noteId: string) {
    this.noteService
      .postTrashNote({ noteIdList: [noteId], isDeleted: false })
      .subscribe({
        next: () => {
          this.noteList = this.noteList.filter((n) => n.id !== noteId);
        },
        error: (err) => console.error('Failed to restore note:', err),
      });
  }

  onDeleteForever(noteId: string) {
    const confirmed = confirm(
      'Are you sure you want to permanently delete this note?'
    );
    if (!confirmed) return;

    this.noteService
      .deleteForeverNotes({ noteIdList: [noteId], isDeleted: true })
      .subscribe({
        next: () => {
          this.noteList = this.noteList.filter((n) => n.id !== noteId);
        },
        error: (err) => console.error('Failed to delete forever:', err),
      });
  }

  onDeleteNote(noteId: string) {}
}
