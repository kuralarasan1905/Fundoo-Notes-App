import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Note } from 'src/app/model/note';
import { NoteService } from 'src/app/services/note_service/note.service';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-archive',
  standalone: true,
  imports: [CommonModule, NoteCardComponent, MatIconModule, MatTooltipModule],
  templateUrl: './archive.component.html',
  styleUrls: ['./archive.component.scss'],
})
export class ArchiveComponent implements OnInit, OnDestroy {
  archivedNotes: Note[] = [];
  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;
  isDarkMode: boolean = false;

  constructor(
    private noteService: NoteService,
    private viewService: ViewService
  ) {}

  ngOnInit() {
    this.loadArchivedNotes();
    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });

    this.detectTheme();

    const observer = new MutationObserver(() => this.detectTheme());
    observer.observe(document.body, {
      attributes: true,
      attributeFilter: ['class'],
    });
  }

  // Add method to refresh archived notes (can be called when navigating to archive)
  refreshArchivedNotes() {
    console.log(' Refreshing archived notes...');
    this.loadArchivedNotes();
  }

  private detectTheme() {
    this.isDarkMode = document.body.classList.contains('dark-theme');
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
  }

  onArchiveNote(payload: { id: string; isArchived: boolean }): void {
    console.log(' Unarchive note request:', payload);

    this.noteService
      .toggleArchive(payload.id)
      .subscribe({
        next: (response) => {
          console.log(' Unarchive successful:', response);
          // Remove the note from archived list since it's now unarchived
          this.archivedNotes = this.archivedNotes.filter(
            (n) => n.id !== payload.id
          );
        },
        error: (err) => {
          console.error(' Unarchive failed:', err);
          console.error('Error details:', {
            status: err.status,
            message: err.message,
            url: err.url
          });
        },
      });
  }

  loadArchivedNotes() {
    console.log(' Loading archived notes...');

    this.noteService.getArchiveNoteListApiCall().subscribe({
      next: (res: any) => {
        console.log(' Archive API response:', res);

        // Handle different response structures
        if (Array.isArray(res)) {
          // Direct array response
          this.archivedNotes = res;
        } else if (res.data) {
          // Nested response structure
          if (Array.isArray(res.data)) {
            this.archivedNotes = res.data;
          } else if (res.data.data && Array.isArray(res.data.data)) {
            this.archivedNotes = res.data.data;
          } else {
            this.archivedNotes = [];
          }
        } else {
          this.archivedNotes = [];
        }

        console.log(' Loaded archived notes:', this.archivedNotes);
      },
      error: (err: any) => {
        console.error(' Error loading archived notes:', err);
        console.error('Error details:', {
          status: err.status,
          message: err.message,
          url: err.url
        });
        this.archivedNotes = [];
      },
    });
  }

  onEditNote(event: any): void {}

  onDeleteNote(noteId: string): void {}
}
