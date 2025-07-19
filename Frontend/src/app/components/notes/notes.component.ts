import { Component, OnDestroy, OnInit, HostListener, ElementRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { IconsComponent } from '../icons/icons.component';
import { Note, CreateNoteDto } from 'src/app/model/note';
import { Label } from 'src/app/model/label';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';
import { ActivatedRoute } from '@angular/router';
import { LabelService } from 'src/app/services/label_service/label.service';
import { NoteService } from 'src/app/services/note_service/note.service';
import { LoadingService } from 'src/app/services/loading.service';
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
    MatButtonModule,
    IconsComponent,
    NoteCardComponent,
    MatTooltipModule,
  ],
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss'],
})
export class NotesComponent implements OnInit, OnDestroy {
  @ViewChild('titleInput') titleInput!: ElementRef<HTMLInputElement>;

  notes: FormGroup;
  isExpanded = false;
  noteList: Note[] = [];
  isPinned: boolean = false;
  selectedColor = 'var(--bg-color)';

  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;

  private searchSub!: Subscription;
  searchText = '';

  // Label filtering
  private routeSub!: Subscription;
  selectedLabelId: string | null = null;
  selectedLabelName: string = '';
  labelFilteredNotes: Note[] = [];

  // Flag to track if backend is available (public for template)
  backendAvailable = true;

  onColorSelected(event: { color: string; index: number }) {
    this.selectedColor = event.color;
  }

  constructor(
    private viewService: ViewService,
    private fb: FormBuilder,
    private noteService: NoteService,
    private loadingService: LoadingService,
    private searchService: SearchService,
    private dialog: MatDialog,
    private elementRef: ElementRef,
    private route: ActivatedRoute,
    private labelService: LabelService
  ) {
    this.notes = this.fb.group({
      title: [''],
      content: [''], // Changed from 'description' to 'content'
    });
  }

  ngOnInit() {
    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });



    // Set backend as available by default
    this.backendAvailable = true;

    // Subscribe to route query parameters for label filtering
    this.routeSub = this.route.queryParams.subscribe(params => {
      this.selectedLabelId = params['label'] || null;
      this.selectedLabelName = params['labelName'] || '';


      if (this.selectedLabelId) {
        this.loadNotesByLabel(this.selectedLabelId);
      } else {
        this.loadNotes();
      }
    });

    // Google Keep style: Subscribe to notes stream
    this.searchSub = this.searchService.search$.subscribe((query) => {
      this.searchText = query;
    });
  }



  // Test backend connection






  loadNotes() {
    console.log('STUDY: Loading notes from backend...');

    // Try the getNotesList endpoint first
    this.noteService.getNotesSimple().subscribe({
      next: (response: any) => {
        console.log('Backend response from getNotesList:', response);
        this.processNotesResponse(response);
      },
      error: (err) => {
        console.log('getNotesList failed, trying /Notes endpoint:', err);

        // If getNotesList fails, try the /Notes endpoint
        this.noteService.getAllNotesSimple().subscribe({
          next: (response: any) => {
            console.log('Backend response from /Notes:', response);
            this.processNotesResponse(response);
          },
          error: (err2) => {
            console.error('Both endpoints failed:', err2);
            this.noteList = [];
            this.backendAvailable = false;
          },
        });
      },
    });
  }

  loadNotesByLabel(labelId: string) {
    console.log('Loading notes by label:', labelId);

    // TEMPORARY: Skip backend API and go straight to client-side filtering for debugging
    console.log('🔧 DEBUGGING: Skipping backend API, using client-side filtering');
    this.loadAllNotesAndFilterByLabel(labelId);

    // TODO: Re-enable backend API once we confirm client-side filtering works
    /*
    this.labelService.getNotesByLabel(labelId).subscribe({
      next: (response: any) => {
        console.log(' Backend API SUCCESS - Notes by label response:', response);
        this.processNotesResponse(response);
      },
      error: (error) => {
        console.error(' Backend API FAILED - Error details:', error);
        console.error(' Error status:', error.status);
        console.error(' Error message:', error.message);
        // Fallback: load all notes and filter client-side
        this.loadAllNotesAndFilterByLabel(labelId);
      }
    });
    */
  }

  private loadAllNotesAndFilterByLabel(labelId: string) {
    console.log('Loading all notes and filtering by label:', labelId);

    // Load all notes first
    this.noteService.getNotesSimple().subscribe({
      next: (response: any) => {
        console.log('All notes loaded, now filtering by label:', labelId);
        this.processNotesResponse(response);

        // Debug: Check what labels each note has
        console.log(' DEBUG: All notes before filtering:', this.noteList.map(note => ({
          id: note.id,
          title: note.title,
          noteLabels: note.noteLabels
        })));

        // Filter notes by label client-side
        const filteredNotes = this.noteList.filter(note => {
          console.log(` Checking note "${note.title}" for label ${labelId}:`, note.noteLabels);

          if (!note.noteLabels || note.noteLabels.length === 0) {
            console.log(` Note "${note.title}" has no labels`);
            return false;
          }

          const hasLabel = note.noteLabels.some(label => {
            console.log(` Comparing label ${label.id} with ${labelId}`);
            return label.id === labelId;
          });

          console.log(`${hasLabel ? '' : ''} Note "${note.title}" ${hasLabel ? 'has' : 'does not have'} label ${labelId}`);
          return hasLabel;
        });

        this.noteList = filteredNotes;
        console.log('Filtered notes by label:', this.noteList);
      },
      error: (error) => {
        console.error('Failed to load notes for filtering:', error);
        this.noteList = [];
        this.backendAvailable = false;
      }
    });
  }

  private processNotesResponse(response: any) {
    console.log(' DEBUG: Raw backend response:', response);
    console.log(' DEBUG: Response type:', typeof response);
    console.log(' DEBUG: Is array?', Array.isArray(response));

    // Handle the response - it might be an array or an object with data property
    let notes: Note[] = [];
    if (Array.isArray(response)) {
      notes = response;
      console.log(' Using direct array response');
    } else if (response && response.data && Array.isArray(response.data)) {
      notes = response.data;
      console.log(' Using response.data array');
    } else if (response && Array.isArray(response.notes)) {
      notes = response.notes;
      console.log(' Using response.notes array');
    } else if (response && response.result && Array.isArray(response.result)) {
      notes = response.result;
      console.log(' Using response.result array');
    } else {
      console.log(' Unknown response format, trying to extract notes...');
      // Try to find any array property in the response
      for (const key in response) {
        if (Array.isArray(response[key])) {
          notes = response[key];
          console.log(` Found array in response.${key}`);
          break;
        }
      }
    }

    console.log(' DEBUG: Extracted notes array:', notes);
    console.log(' DEBUG: First note structure:', notes[0]);

    // Map backend fields to frontend Note interface
    const mappedNotes: Note[] = notes.map((backendNote: any) => {
      // Extract the note ID - don't use fallback IDs that don't exist in backend
      const noteId = backendNote.Id || backendNote.id || backendNote.noteId;

      if (!noteId) {
        console.error(' Backend note missing ID:', backendNote);
        console.error(' Available properties:', Object.keys(backendNote));
      }

      // Debug color values from backend
      const backendColor = backendNote.Color || backendNote.color || backendNote.noteColor;
      console.log(' DEBUG: Backend color for note', noteId, ':', {
        rawColor: backendColor,
        Color: backendNote.Color,
        color: backendNote.color,
        noteColor: backendNote.noteColor,
        finalColor: backendColor || '#ffffff'
      });

      const mappedNote: Note = {
        // Your backend uses Pascal case field names - NO FALLBACK to Date.now()
        id: noteId,
        title: backendNote.Title || backendNote.title || backendNote.noteTitle || '',
        previewContent: backendNote.PreviewContent || backendNote.previewContent || backendNote.content || backendNote.description || '',
        color: backendColor || '#ffffff',
        isPined: backendNote.IsPinned || backendNote.isPined || backendNote.isPinned || backendNote.pinned || false,
        isArchived: backendNote.IsArchived || backendNote.isArchived || backendNote.archived || false,
        isDeleted: backendNote.IsTrashed || backendNote.isDeleted || backendNote.deleted || false,
        createdDate: backendNote.CreatedDate || backendNote.createdDate || backendNote.createdAt || new Date().toISOString(),
        modifiedDate: backendNote.ModifiedDate || backendNote.modifiedDate || backendNote.updatedAt || new Date().toISOString(),
        // Map note labels from backend
        noteLabels: this.mapNoteLabels(backendNote.NoteLabels || backendNote.noteLabels || backendNote.labels || [])
      };

      console.log(' DEBUG: Backend note:', backendNote);
      console.log(' DEBUG: Mapped note:', mappedNote);
      return mappedNote;
    });

    // Filter out archived, deleted notes, and notes without valid IDs
    this.noteList = mappedNotes.filter(
      (note: Note) => note.id && !note.isArchived && !note.isDeleted
    );

    console.log(' Notes filtering results:');
    console.log('  - Total backend notes:', notes.length);
    console.log('  - Mapped notes:', mappedNotes.length);
    console.log('  - Valid notes (with IDs):', this.noteList.length);
    console.log('  - Notes without IDs:', mappedNotes.filter(n => !n.id).length);

    console.log(' Final notes list:', this.noteList);
    this.backendAvailable = true;
  }

  // Helper method to map note labels from backend format
  private mapNoteLabels(backendLabels: any[]): Label[] {
    if (!Array.isArray(backendLabels)) {
      return [];
    }

    return backendLabels.map(label => ({
      id: label.Id || label.id || '',
      name: label.Name || label.name || label.label || '',
      label: label.Name || label.name || label.label || '', // For backward compatibility
      color: label.Color || label.color || undefined,
      isDeleted: label.IsDeleted || label.isDeleted || false,
      userId: label.UserId || label.userId || undefined
    }));
  }

  // TrackBy function for better performance and debugging
  trackByNoteId(_index: number, note: Note): string {
    return note.id;
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
    this.searchSub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }

  expandForm(): void {
    this.isExpanded = true;
    // Focus on title input after view updates
    setTimeout(() => {
      if (this.titleInput?.nativeElement) {
        this.titleInput.nativeElement.focus();
      }
    }, 0);
  }

  togglePin() {
    this.isPinned = !this.isPinned;
  }

  // Listen for clicks outside the component
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    try {
      if (this.isExpanded && event.target) {
        const target = event.target as HTMLElement;
        const noteForm = this.elementRef.nativeElement?.querySelector('.note-form');

        // Check if the click was on a Material UI overlay (menu, palette, etc.)
        const isOnOverlay = target.closest('.cdk-overlay-container') ||
                           target.closest('.mat-menu-panel') ||
                           target.closest('.color-palette');

        // Check if the click was outside the note form and not on an overlay
        if (noteForm && !noteForm.contains(target) && !isOnOverlay) {
          // User clicked outside the form, save the note
          this.onCloseNote(true);
        }
      }
    } catch (error) {
      console.error('Error in document click handler:', error);
    }
  }

  // Listen for Escape key press
  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent): void {
    try {
      if (this.isExpanded) {
        // User pressed Escape, save the note and close
        this.onCloseNote(true);
        event.preventDefault();
      }
    } catch (error) {
      console.error('Error in escape key handler:', error);
    }
  }

  onCloseNote(shouldSave: boolean) {
    console.log(' onCloseNote called with shouldSave:', shouldSave);

    const formValue = this.notes.value;
    const hasContent = formValue.title?.trim() || formValue.content?.trim();

    console.log(' Form value:', formValue);
    console.log(' Has content:', hasContent);

    if (shouldSave && hasContent) {
      const payload: CreateNoteDto = {
        title: formValue.title?.trim() || '',
        content: formValue.content?.trim() || '',
        color: this.selectedColor !== 'var(--bg-color)' ? this.selectedColor : undefined,
        reminderDateTime: undefined, // Can be set later
        labelIds: this.selectedLabelId ? [this.selectedLabelId] : [] // Auto-assign current label if filtering
      };

      console.log('� STUDY: About to call simple addNote with payload:', payload);

      // Show loading spinner for note creation
      this.loadingService.show();

      // Use simple method that calls the correct backend endpoint
      this.noteService.addNoteSimple(payload).subscribe({
        next: (response: any) => {
          console.log(' Note created successfully:', response);
          // Hide loading spinner
          this.loadingService.hide();

          // Add the new note to the local list
          if (response) {
            // Create a note object from the response
            const newNote: Note = {
              id: response.Id || response.id || Date.now().toString(),
              title: response.Title || response.title || payload.title,
              previewContent: response.PreviewContent || response.content || payload.content,
              color: response.Color || response.color || payload.color || '#ffffff',
              isPined: response.IsPinned || response.isPined || false,
              isArchived: response.IsArchived || response.isArchived || false,
              isDeleted: response.IsTrashed || response.isDeleted || false,
              createdDate: response.CreatedDate || response.createdDate || new Date().toISOString(),
              modifiedDate: response.ModifiedDate || response.modifiedDate || new Date().toISOString(),
            };

            // Add to the beginning of the list (Google Keep style)
            this.noteList.unshift(newNote);
          }

          // Refresh the list from backend to ensure consistency
          this.loadNotes();
        },
        error: (err) => {
          console.error(' Error creating note:', err);
          // Hide loading spinner
          this.loadingService.hide();
        },
      });
    } else {
      console.log(' Note not saved - shouldSave:', shouldSave, 'hasContent:', hasContent);
    }

    // Reset form state
    this.resetForm();
  }

  private resetForm(): void {
    this.notes.reset();
    this.selectedColor = 'var(--bg-color)';
    this.isExpanded = false;
    this.isPinned = false;
  }



  // Method to cancel note creation without saving
  cancelNote(): void {
    this.resetForm();
  }

  onEditNote(note: Note) {
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
          this.noteList = this.noteList.filter((n) => n.id !== result.noteId);
          console.log('Note archived and removed from list');
          return;
        }

        // Handle delete operation
        if (result.deleted) {
          this.noteList = this.noteList.filter((n) => n.id !== result.noteId);
          console.log('Note moved to trash and removed from list');
          return;
        }

        // Handle regular note update
        if (result.noteId) {
          const index = this.noteList.findIndex(
            (n) => n.id === result.noteId
          );
          if (index !== -1) {
            this.noteList[index] = {
              ...this.noteList[index],
              title: result.title,
              previewContent: result.content, // Changed from 'description' to 'content'
              color: result.color,
              isPined: result.isPined ?? this.noteList[index].isPined,
            };
          }
        }
      }
    });
  }

  get filteredNotes(): Note[] {
    const query = this.searchText.toLowerCase();
    const notesToFilter = this.noteList; // Use the main note list

    return notesToFilter.filter(
      (note) =>
        note.title.toLowerCase().includes(query) ||
        note.previewContent.toLowerCase().includes(query) // Changed from 'description' to 'content'
    );
  }

  // Get current notes list for display (handles label filtering)
  get currentNotes(): Note[] {
    return this.noteList;
  }

  // Get label name for display
  getLabelDisplayName(): string {
    if (!this.selectedLabelId) return '';
    return this.selectedLabelName || 'Selected Label';
  }

  // Get placeholder text for note input
  getNotePlaceholder(): string {
    if (this.isExpanded) {
      return this.selectedLabelId ? `Title (will be labeled: ${this.selectedLabelName})` : 'Title';
    }
    return this.selectedLabelId ? `Take a note for ${this.selectedLabelName}...` : 'Take a note...';
  }

  onArchiveNote(payload: { id: string; isArchived: boolean }) {
    console.log(' Archive note request:', payload);

    this.noteService
      .toggleArchive(payload.id)
      .subscribe({
        next: (response) => {
          console.log(' Archive toggle successful:', response);
          // Remove the note from the current list since it's now archived/unarchived
          this.noteList = this.noteList.filter((n) => n.id !== payload.id);
        },
        error: (err) => {
          console.error(' Archive/unarchive failed:', err);
          console.error('Error details:', {
            status: err.status,
            message: err.message,
            url: err.url
          });
        },
      });
  }

  onTrashNote(noteId: string) {
    this.noteService
      .moveNoteToTrash(noteId)
      .subscribe({
        next: () => {
          this.noteList = this.noteList.filter((n) => n.id !== noteId);
        },
        error: (err) => console.error('Failed to move note to trash:', err),
      });
  }

  onTogglePin(payload: { id: string; isPinned: boolean }) {
    console.log(' Pin/unpin request received:', payload);

    // Validate payload
    if (!payload.id) {
      console.error(' Invalid note ID for pin/unpin:', payload.id);
      return;
    }

    if (payload.isPinned === undefined || payload.isPinned === null) {
      console.error(' Invalid isPinned value:', payload.isPinned);
      return;
    }

    // Show loading state (optional)
    this.loadingService.show();

    // Call the backend API with single note format
    this.noteService
      .pinUnpinNotes({ noteId: payload.id, isPinned: payload.isPinned })
      .subscribe({
        next: (response) => {
          console.log(' Pin/unpin successful:', response);

          // Update the local note list immediately
          const index = this.noteList.findIndex((n) => n.id === payload.id);
          if (index !== -1) {
            this.noteList[index].isPined = payload.isPinned;
            console.log(' Local note updated:', payload.id, payload.isPinned ? 'pinned' : 'unpinned');
          }

          // Hide loading state
          this.loadingService.hide();

          // Optional: Refresh notes from backend to ensure consistency
          // this.loadNotes();
        },
        error: (err) => {
          console.error(' Pin/unpin failed:', err);

          // Hide loading state
          this.loadingService.hide();

          // Show user-friendly error message based on error type
          if (err.status === 401) {
            console.error(' Authentication required - please log in again');
          } else if (err.status === 404) {
            console.error(' Pin/unpin endpoint not found on backend');
          } else if (err.status === 400) {
            console.error(' Invalid request - check note ID and payload format');
          } else if (err.status === 0) {
            console.error(' Backend server not reachable');
          } else {
            console.error(' Unexpected error occurred');
          }
        },
      });
  }

  get pinnedNotes(): Note[] {
    return this.filteredNotes.filter((n) => n.isPined);
  }
  get otherNotes(): Note[] {
    return this.filteredNotes.filter((n) => !n.isPined);
  }
}
