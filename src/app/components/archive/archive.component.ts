import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Note } from 'src/app/model/note';
import { NoteService } from 'src/app/services/note_service/note.service';
import { NoteCardComponent } from '../note-card/note-card.component';

@Component({
  selector: 'app-archive',
  standalone: true,
  imports: [CommonModule, NoteCardComponent],
  templateUrl: './archive.component.html',
  styleUrls: ['./archive.component.scss'],
})
export class ArchiveComponent implements OnInit {
  archivedNotes: Note[] = [];

  constructor(private noteService: NoteService) {}

  ngOnInit() {
    this.loadArchivedNotes();
  }

  loadArchivedNotes() {
    this.noteService.getArchiveNoteListApiCall().subscribe({
      next: (res: any) => {
        this.archivedNotes = res.data.data;
      },
      error: (err) => console.error('Error loading archived notes', err),
    });
  }

  onEditNote(event: any): void {}

  onDeleteNote(noteId: string): void {}
}
