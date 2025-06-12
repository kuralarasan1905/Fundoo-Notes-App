import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Note } from 'src/app/model/note';
import { NoteService } from 'src/app/services/note_service/note.service';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';

@Component({
  selector: 'app-trash',
  standalone: true,
  imports: [CommonModule, NoteCardComponent],
  templateUrl: './trash.component.html',
  styleUrls: ['./trash.component.scss'],
})
export class TrashComponent implements OnInit, OnDestroy {
  noteList: Note[] = [];
  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;

  constructor(
    private noteService: NoteService,
    private viewService: ViewService
  ) {}

  ngOnInit() {
    this.noteService.getTrashList().subscribe({
      next: (res: any) => {
        this.noteList = res.data.data;
      },
      error: (err) => console.error('Failed to fetch trash notes:', err),
    });
    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
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
