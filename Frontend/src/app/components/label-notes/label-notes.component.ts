import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { Note, CreateNoteDto } from '../../model/note';
import { Label } from '../../model/label';
import { NoteService } from '../../services/note_service/note.service';
import { LabelService } from '../../services/label_service/label.service';
import { LoadingService } from '../../services/loading.service';
import { NoteCardComponent } from '../note-card/note-card.component';
import { IconsComponent } from '../icons/icons.component';
import { NoteDialogComponent } from '../note-dialog/note-dialog.component';

@Component({
  selector: 'app-label-notes',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    NoteCardComponent,
    IconsComponent
  ],
  templateUrl: './label-notes.component.html',
  styleUrls: ['./label-notes.component.scss']
})
export class LabelNotesComponent implements OnInit, OnDestroy {
  notes: Note[] = [];
  currentLabel: Label | null = null;
  labelId: string = '';
  isLoading: boolean = false;
  viewMode: 'grid' | 'list' = 'grid';

  // Form handling
  noteForm: FormGroup;
  isExpanded: boolean = false;
  isPinned: boolean = false;
  selectedColor: string = 'var(--bg-color)';

  private routeSub?: Subscription;
  private labelsSub?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private noteService: NoteService,
    private labelService: LabelService,
    private loadingService: LoadingService,
    private dialog: MatDialog
  ) {
    // Initialize form
    this.noteForm = this.fb.group({
      title: [''],
      content: ['']
    });
  }

  ngOnInit(): void {
    // Subscribe to route parameters to get the label ID
    this.routeSub = this.route.params.subscribe(params => {
      this.labelId = params['labelId'];

      if (this.labelId) {
        this.loadLabelInfo();
        this.loadNotesByLabel();
      }
    });

    // Subscribe to label updates
    this.labelsSub = this.labelService.labelsUpdated$.subscribe(() => {
      this.loadLabelInfo();
    });
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
    this.labelsSub?.unsubscribe();
  }

  loadLabelInfo(): void {
    this.labelService.getAllLabels().subscribe({
      next: (response: any) => {
        let labelsArray: any[] = [];

        // Handle different possible response formats
        if (response.data?.details) {
          labelsArray = response.data.details;
        } else if (response.data) {
          labelsArray = response.data;
        } else if (Array.isArray(response)) {
          labelsArray = response;
        }

        // Find the current label - handle both string and number ID comparisons
        const foundLabel = labelsArray.find(label => {
          const labelId = label.id || label.Id;

          // Try both string and number comparisons
          return labelId == this.labelId ||
                 labelId === this.labelId ||
                 String(labelId) === String(this.labelId) ||
                 Number(labelId) === Number(this.labelId);
        });

        if (foundLabel) {
          this.currentLabel = {
            id: foundLabel.id || foundLabel.Id,
            name: foundLabel.name || foundLabel.Name || foundLabel.label,
            label: foundLabel.name || foundLabel.Name || foundLabel.label,
            color: foundLabel.color || foundLabel.Color
          };
        } else {
          // Don't redirect immediately, show the error state instead
          this.currentLabel = null;
        }
      },
      error: (error) => {
        this.currentLabel = null;
      }
    });
  }

  loadNotesByLabel(): void {
    if (!this.labelId) return;


    this.isLoading = true;

    // Convert labelId to string to ensure consistent API calls
    const labelIdString = String(this.labelId);

    this.labelService.getNotesByLabel(labelIdString).subscribe({
      next: (response: any) => {


        let notesArray: any[] = [];

        // Handle different possible response formats
        if (response.data?.details) {
          notesArray = response.data.details;
        } else if (response.data) {
          notesArray = response.data;
        } else if (Array.isArray(response)) {
          notesArray = response;
        }

        // Map backend response to frontend format
        this.notes = notesArray.map(note => ({
          id: note.id || note.Id || '',
          title: note.title || note.Title || '',
          previewContent: note.content || note.Content || note.previewContent || '',
          isPined: note.isPined || note.IsPined || false,
          isArchived: note.isArchived || note.IsArchived || false,
          isDeleted: note.isDeleted || note.IsDeleted || false,
          reminder: note.reminder || note.Reminder || note.reminderDateTime,
          createdDate: note.createdDate || note.CreatedDate,
          modifiedDate: note.modifiedDate || note.ModifiedDate,
          color: note.color || note.Color || '#ffffff',
          noteLabels: this.mapNoteLabels(note.noteLabels || note.NoteLabels || [])
        }));


        this.isLoading = false;
      },
      error: (error) => {
        // If API fails, try to fallback to client-side filtering
        this.fallbackToClientSideFiltering();
      }
    });
  }

  private fallbackToClientSideFiltering(): void {
    // Load all notes and filter client-side as fallback
    this.noteService.getAllNotesSimple().subscribe({
      next: (response: any) => {

        let notesArray: any[] = [];

        if (response.data?.details) {
          notesArray = response.data.details;
        } else if (response.data) {
          notesArray = response.data;
        } else if (Array.isArray(response)) {
          notesArray = response;
        }

        // Filter notes that have the current label
        const filteredNotes = notesArray.filter(note => {
          const noteLabels = note.noteLabels || note.NoteLabels || [];
          return noteLabels.some((label: any) => {
            const labelId = label.id || label.Id;
            return labelId == this.labelId ||
                   String(labelId) === String(this.labelId) ||
                   Number(labelId) === Number(this.labelId);
          });
        });

        // Map the filtered notes to our frontend format
        this.notes = filteredNotes.map(note => ({
          id: note.id || note.Id || '',
          title: note.title || note.Title || '',
          previewContent: note.content || note.Content || note.previewContent || '',
          isPined: note.isPined || note.IsPined || false,
          isArchived: note.isArchived || note.IsArchived || false,
          isDeleted: note.isDeleted || note.IsDeleted || false,
          reminder: note.reminder || note.Reminder || note.reminderDateTime,
          createdDate: note.createdDate || note.CreatedDate,
          modifiedDate: note.modifiedDate || note.ModifiedDate,
          color: note.color || note.Color || '',
          noteLabels: this.mapNoteLabels(note.noteLabels || note.NoteLabels || [])
        }));


        this.isLoading = false;
      },
      error: (error) => {

        this.notes = [];
        this.isLoading = false;
      }
    });
  }

  // Helper method to map note labels from backend format
  private mapNoteLabels(backendLabels: any[]): Label[] {
    if (!Array.isArray(backendLabels)) {
      return [];
    }

    return backendLabels.map(label => ({
      id: label.Id || label.id || '',
      name: label.Name || label.name || label.label || '',
      label: label.Name || label.name || label.label || '',
      color: label.Color || label.color || undefined,
      isDeleted: label.IsDeleted || label.isDeleted || false,
      userId: label.UserId || label.userId || undefined
    }));
  }

  // Handle note creation within this label context
  onNoteCreated(note: Note): void {
    // If we have a current label, automatically add it to the new note
    if (this.currentLabel && note.id) {
      this.labelService.addLabelToNote({
        noteId: note.id,
        labelId: this.currentLabel.id
      }).subscribe({
        next: () => {
          // Add the label to the note object
          if (!note.noteLabels) {
            note.noteLabels = [];
          }
          note.noteLabels.push(this.currentLabel!);

          // Add the note to our local list
          this.notes.unshift(note);
        },
        error: (error) => {
          // Still add the note to the list even if labeling failed
          this.notes.unshift(note);
        }
      });
    } else {
      // Add the note to our local list
      this.notes.unshift(note);
    }
  }

  // Handle note editing
  onEditNote(note: Note): void {
    const dialogRef = this.dialog.open(NoteDialogComponent, {
      data: note,
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      panelClass: 'custom-dialog-container',
      hasBackdrop: true,
      disableClose: false,
      autoFocus: false,
      restoreFocus: false,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        // Handle archive operation
        if (result.archived) {
          this.notes = this.notes.filter((n) => n.id !== result.noteId);
          return;
        }

        // Handle delete operation
        if (result.deleted) {
          this.notes = this.notes.filter((n) => n.id !== result.noteId);
          return;
        }

        // Refresh the notes to get updated data
        this.loadNotesByLabel();
      }
    });
  }

  // Handle note archiving
  onArchiveNote(event: { id: string; isArchived: boolean }): void {
    // Update local state
    const noteIndex = this.notes.findIndex(note => note.id === event.id);
    if (noteIndex !== -1) {
      this.notes[noteIndex].isArchived = event.isArchived;

      // If archived, remove from current view
      if (event.isArchived) {
        this.notes.splice(noteIndex, 1);
      }
    }
  }

  // Handle moving note to trash
  onMoveToTrash(noteId: string): void {
    // Remove from current view
    this.notes = this.notes.filter(note => note.id !== noteId);
  }

  // Handle pin/unpin toggle
  onTogglePin(event: { id: string; isPinned: boolean }): void {
    // Update local state
    const noteIndex = this.notes.findIndex(note => note.id === event.id);
    if (noteIndex !== -1) {
      this.notes[noteIndex].isPined = event.isPinned;
    }
  }

  // Toggle view mode
  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
  }

  // Navigate back to all notes
  goBackToNotes(): void {
    this.router.navigate(['/notes']);
  }

  // Form handling methods
  expandForm(): void {
    this.isExpanded = true;
  }

  togglePin(): void {
    this.isPinned = !this.isPinned;
  }

  onColorSelected(event: { color: string; index: number }): void {
    this.selectedColor = event.color;
  }

  onCloseNote(shouldSave: boolean): void {
    const formValue = this.noteForm.value;
    const hasContent = formValue.title?.trim() || formValue.content?.trim();

    if (shouldSave && hasContent) {
      const payload: CreateNoteDto = {
        title: formValue.title?.trim() || '',
        content: formValue.content?.trim() || '',
        color: this.selectedColor !== 'var(--bg-color)' ? this.selectedColor : undefined,
        reminderDateTime: undefined,
        labelIds: this.currentLabel ? [this.currentLabel.id] : [] // Auto-assign current label
      };



      // Show loading spinner
      this.loadingService.show();

      this.noteService.addNoteSimple(payload).subscribe({
        next: (response: any) => {

          this.loadingService.hide();

          // Create note object for local state
          const newNote: Note = {
            id: response.id || response.noteId || Date.now().toString(),
            title: payload.title,
            previewContent: payload.content,
            color: payload.color || '',
            isPined: this.isPinned,
            isArchived: false,
            isDeleted: false,
            createdDate: new Date().toISOString(),
            modifiedDate: new Date().toISOString(),
            noteLabels: this.currentLabel ? [this.currentLabel] : []
          };

          // Add to local list
          this.notes.unshift(newNote);

          // Reset form
          this.resetForm();
        },
        error: (err) => {

          this.loadingService.hide();
        }
      });
    } else {

    }

    // Reset form state
    this.resetForm();
  }

  private resetForm(): void {
    this.noteForm.reset();
    this.selectedColor = 'var(--bg-color)';
    this.isExpanded = false;
    this.isPinned = false;
  }

  // TrackBy function for better performance
  trackByNoteId(_index: number, note: Note): string {
    return note.id;
  }
}
