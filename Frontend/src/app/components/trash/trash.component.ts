import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Note } from 'src/app/model/note';
import { NoteService } from 'src/app/services/note_service/note.service';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-trash',
  standalone: true,
  imports: [CommonModule, NoteCardComponent, MatIconModule, MatTooltipModule],
  templateUrl: './trash.component.html',
  styleUrls: ['./trash.component.scss'],
})
export class TrashComponent implements OnInit, OnDestroy {
  noteList: Note[] = [];
  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;
  private routerSub?: Subscription;
  isDarkMode: boolean = false;

  constructor(
    private noteService: NoteService,
    private viewService: ViewService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadTrashNotes();

    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });

    // Listen for navigation events to refresh trash when user navigates to trash page
    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        if (event instanceof NavigationEnd && event.url.includes('/trash')) {
          console.log(' Navigated to trash page, refreshing...');
          this.loadTrashNotes();
        }
      });

    this.detectTheme();

    const observer = new MutationObserver(() => this.detectTheme());
    observer.observe(document.body, {
      attributes: true,
      attributeFilter: ['class'],
    });
  }

  loadTrashNotes() {
    console.log(' Loading trash notes...');
    this.noteService.getTrashList().subscribe({
      next: (res: any) => {
        console.log(' Trash list response:', res);
        console.log(' Response type:', typeof res);
        console.log(' Response structure:', Object.keys(res || {}));

        // Handle different response structures
        if (res.data && Array.isArray(res.data)) {
          this.noteList = res.data;
        } else if (res.data && res.data.data && Array.isArray(res.data.data)) {
          this.noteList = res.data.data;
        } else if (Array.isArray(res)) {
          this.noteList = res;
        } else {
          this.noteList = [];
          console.warn('Unexpected trash list response structure:', res);
        }
        console.log(' Loaded trash notes count:', this.noteList.length);
        console.log(' Loaded trash notes:', this.noteList);
      },
      error: (err) => {
        console.error(' Failed to fetch trash notes:', err);
        console.error(' Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          url: err.url,
          error: err.error
        });
      },
    });
  }

  private detectTheme() {
    this.isDarkMode = document.body.classList.contains('dark-theme');
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
    this.routerSub?.unsubscribe();
  }

  // Add method to refresh trash list
  refreshTrashList() {
    this.loadTrashNotes();
  }



  onRestoreNote(noteId: string) {
    console.log(' Restoring note:', noteId);
    this.noteService
      .restoreNoteFromTrash(noteId)
      .subscribe({
        next: (response) => {
          console.log(' Note restored successfully:', response);
          this.noteList = this.noteList.filter((n) => n.id !== noteId);
        },
        error: (err) => {
          console.error(' Failed to restore note:', err);
          console.error('Error details:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            url: err.url,
            error: err.error
          });

          // Show user-friendly error message
          if (err.status === 0) {
            alert('Network error: Please check if the backend server is running.');
          } else if (err.status === 401) {
            alert('Authentication error: Please log in again.');
          } else if (err.status === 404) {
            alert('Note not found. It may have been already deleted.');
          } else {
            alert(`Failed to restore note: ${err.message || 'Unknown error'}`);
          }
        },
      });
  }

  onDeleteForever(noteId: string) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '330px',
      panelClass: 'custom-confirm-dialog',
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      console.log(' Permanently deleting note:', noteId);
      this.noteService
        .deleteNotePermanently(noteId)
        .subscribe({
          next: (response) => {
            console.log(' Note permanently deleted:', response);
            this.noteList = this.noteList.filter((n) => n.id !== noteId);
          },
          error: (err) => {
            console.error(' Failed to delete forever:', err);
            console.error('Error details:', {
              status: err.status,
              statusText: err.statusText,
              message: err.message,
              url: err.url,
              error: err.error
            });

            // Show user-friendly error message
            if (err.status === 0) {
              alert('Network error: Please check if the backend server is running.');
            } else if (err.status === 401) {
              alert('Authentication error: Please log in again.');
            } else if (err.status === 404) {
              alert('Note not found. It may have been already deleted.');
            } else {
              alert(`Failed to permanently delete note: ${err.message || 'Unknown error'}`);
            }
          },
        });
    });
  }

  emptyRecycleBin() {
    console.log(' Empty trash clicked');
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '330px',
      panelClass: 'custom-confirm-dialog',
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      const noteIdList = this.noteList.map((n) => n.id);
      if (noteIdList.length === 0) {
        console.log(' No notes in trash to empty');
        return;
      }

      console.log(' Emptying trash - deleting notes:', noteIdList);
      this.noteService
        .deleteForeverNotes({ noteIdList, isDeleted: true })
        .subscribe({
          next: (response) => {
            console.log(' Trash emptied successfully:', response);
            this.noteList = [];
          },
          error: (err) => {
            console.error(' Failed to empty trash:', err);
            console.error('Error details:', {
              status: err.status,
              statusText: err.statusText,
              message: err.message,
              url: err.url,
              error: err.error
            });

            // Show user-friendly error message
            if (err.status === 0) {
              alert('Network error: Please check if the backend server is running.');
            } else if (err.status === 401) {
              alert('Authentication error: Please log in again.');
            } else {
              alert(`Failed to empty trash: ${err.message || 'Unknown error'}`);
            }
          },
        });
    });
  }
}
