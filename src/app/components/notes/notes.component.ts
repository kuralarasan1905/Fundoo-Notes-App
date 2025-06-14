import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { IconsComponent } from '../icons/icons.component';
import { Note } from 'src/app/model/note';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';
import { NoteService } from 'src/app/services/note_service/note.service';
import { SearchService } from 'src/app/search.service';
import { MatDialog } from '@angular/material/dialog';
import { NoteDialogComponent } from '../note-dialog/note-dialog.component';
import { MatTooltipModule } from '@angular/material/tooltip';

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
    MatTooltipModule,
  ],
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss'],
})
export class NotesComponent implements OnInit, OnDestroy {
  notes: FormGroup;
  isExpanded = false;
  noteList: Note[] = [];
  isPinned: boolean = false;
  selectedColor = 'var(--bg-color)';

  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;

  private searchSub!: Subscription;
  searchText = '';

  onColorSelected(event: { color: string; index: number }) {
    this.selectedColor = event.color;
  }

  constructor(
    private viewService: ViewService,
    private fb: FormBuilder,
    private noteService: NoteService,
    private searchService: SearchService,
    private dialog: MatDialog
  ) {
    this.notes = this.fb.group({
      title: [''],
      description: [''],
    });
  }

  ngOnInit() {
    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });
    this.loadNotes();
    this.searchSub = this.searchService.search$.subscribe((query) => {
      this.searchText = query;
    });
  }

  loadNotes() {
    this.noteService.getAllNotes().subscribe({
      next: (res: any) => {
        this.noteList = res.data.data.filter(
          (note: any) => !note.isArchived && !note.isDeleted
        );
      },
      error: (err) => console.error('Failed to load notes', err),
    });
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
    this.searchSub?.unsubscribe();
  }

  expandForm(): void {
    this.isExpanded = true;
  }

  togglePin() {
    this.isPinned = !this.isPinned;
  }

  onCloseNote(shouldSave: boolean) {
    const formValue = this.notes.value;

    if (
      shouldSave &&
      (formValue.title?.trim() || formValue.description?.trim())
    ) {
      const payload = {
        title: formValue.title.trim(),
        description: formValue.description.trim(),
        isPined: this.isPinned,
        isArchived: false,
        color: this.selectedColor,
      };

      this.noteService.addNotes(payload).subscribe({
        next: (res: any) => {
          const addedNote = res.status.details;
          console.log('Note Added', addedNote);
          this.noteList.unshift({
            id: addedNote.id,
            title: addedNote.title,
            description: addedNote.description,
            color: addedNote.color,
            isPined: addedNote.isPined ?? this.isPinned,
          });
        },
        error: (err) => {
          console.error('Error adding note:', err);
        },
      });
    }

    this.notes.reset();
    this.selectedColor = 'var(--bg-color)';
    this.isExpanded = false;
    this.isPinned = false;
  }

  onEditNote(note: Note) {
    const dialogRef = this.dialog.open(NoteDialogComponent, {
      data: note,
      width: '600px',
      panelClass: 'custom-dialog-container',
    });

    dialogRef.afterClosed().subscribe((updatedNote) => {
      if (updatedNote) {
        const index = this.noteList.findIndex(
          (n) => n.id === updatedNote.noteId
        );
        if (index !== -1) {
          this.noteList[index] = {
            ...this.noteList[index],
            title: updatedNote.title,
            description: updatedNote.description,
            color: updatedNote.color,
            isPined: updatedNote.isPined ?? this.noteList[index].isPined,
          };
        }
      }
    });
  }

  get filteredNotes(): Note[] {
    const query = this.searchText.toLowerCase();
    return this.noteList.filter(
      (note) =>
        note.title.toLowerCase().includes(query) ||
        note.description.toLowerCase().includes(query)
    );
  }

  onArchiveNote(payload: { id: string; isArchived: boolean }) {
    this.noteService
      .postArchiveList({
        noteIdList: [payload.id],
        isArchived: payload.isArchived,
      })
      .subscribe({
        next: () => {
          this.noteList = this.noteList.filter((n) => n.id !== payload.id);
        },
        error: (err) => console.error('Archive/unarchive failed:', err),
      });
  }

  onTrashNote(noteId: string) {
    this.noteService
      .postTrashNote({ noteIdList: [noteId], isDeleted: true })
      .subscribe({
        next: () => {
          this.noteList = this.noteList.filter((n) => n.id !== noteId);
        },
        error: (err) => console.error('Failed to move note to trash:', err),
      });
  }

  onTogglePin(payload: { id: string; isPined: boolean }) {
    this.noteService
      .pinUnpinNotes({ noteIdList: [payload.id], isPined: payload.isPined })
      .subscribe({
        next: () => {
          const index = this.noteList.findIndex((n) => n.id === payload.id);
          if (index !== -1) {
            this.noteList[index].isPined = payload.isPined;
          }
        },
        error: (err) => console.error('Pin/unpin failed:', err),
      });
  }

  get pinnedNotes(): Note[] {
    return this.filteredNotes.filter((n) => n.isPined);
  }
  get otherNotes(): Note[] {
    return this.filteredNotes.filter((n) => !n.isPined);
  }
}
