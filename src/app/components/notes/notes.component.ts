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
export class NotesComponent implements OnInit, OnDestroy {
  notes: FormGroup;
  isExpanded = false;
  noteList: Note[] = [];

  selectedColor = '#202124';

  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;

  private searchSub!: Subscription;
  searchText = '';

  onColorSelected(color: string) {
    this.selectedColor = color;
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

  onCloseNote(shouldSave: boolean) {
    const formValue = this.notes.value;

    if (
      shouldSave &&
      (formValue.title?.trim() || formValue.description?.trim())
    ) {
      const payload = {
        title: formValue.title.trim(),
        description: formValue.description.trim(),
        isPined: false,
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
          });
        },
        error: (err) => {
          console.error('Error adding note:', err);
        },
      });
    }

    this.notes.reset();
    this.selectedColor = '#202124';
    this.isExpanded = false;
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
}
