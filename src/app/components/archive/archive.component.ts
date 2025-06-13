import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Note } from 'src/app/model/note';
import { NoteService } from 'src/app/services/note_service/note.service';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-archive',
  standalone: true,
  imports: [CommonModule, NoteCardComponent, MatIconModule],
  templateUrl: './archive.component.html',
  styleUrls: ['./archive.component.scss'],
})
export class ArchiveComponent implements OnInit, OnDestroy {
  archivedNotes: Note[] = [];
  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;

  constructor(
    private noteService: NoteService,
    private viewService: ViewService
  ) {}

  ngOnInit() {
    this.loadArchivedNotes();
    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
  }

  onArchiveNote(payload: { id: string; isArchived: boolean }): void {
    this.noteService
      .postArchiveList({
        noteIdList: [payload.id],
        isArchived: payload.isArchived,
      })
      .subscribe({
        next: () => {
          this.archivedNotes = this.archivedNotes.filter(
            (n) => n.id !== payload.id
          );
        },
        error: (err) => console.error('Unarchive failed:', err),
      });
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
