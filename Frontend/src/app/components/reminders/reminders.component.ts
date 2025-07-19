import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NoteService } from 'src/app/services/note_service/note.service';
import { ReminderService } from 'src/app/services/reminder.service';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Note } from 'src/app/model/note';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-reminders',
  standalone: true,
  imports: [CommonModule, NoteCardComponent, MatIconModule],
  templateUrl: './reminders.component.html',
  styleUrls: ['./reminders.component.scss'],
})
export class RemindersComponent implements OnInit, OnDestroy {
  reminderNotes: Note[] = [];
  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;
  isDarkMode: boolean = false;

  constructor(
    private noteService: NoteService,
    private reminderService: ReminderService,
    private viewService: ViewService
  ) {}

  ngOnInit() {
    this.loadReminderNotes();
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

  private detectTheme() {
    this.isDarkMode = document.body.classList.contains('dark-theme');
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
  }

  loadReminderNotes() {
    console.log(' Loading reminder notes using ReminderService...');

    this.reminderService.loadReminderNotes().subscribe({
      next: (res: any) => {
        console.log(' Reminder API response:', res);
        console.log(' Raw response type:', typeof res);
        console.log(' Is array:', Array.isArray(res));

        let rawNotes: any[] = [];

        // Handle different response structures
        if (Array.isArray(res)) {
          rawNotes = res;
        } else if (res && res.data) {
          if (Array.isArray(res.data)) {
            rawNotes = res.data;
          } else if (res.data.data && Array.isArray(res.data.data)) {
            rawNotes = res.data.data;
          }
        } else {
          rawNotes = [];
        }

        console.log(' Raw notes before transformation:', rawNotes);

        // Transform the data to match frontend Note interface
        let transformedNotes = rawNotes.map((note: any) => {
          console.log(' Transforming note:', note);

          const transformedNote: Note = {
            id: note.id || note.noteId || '',
            title: note.title || '',
            previewContent: note.previewContent || note.content || note.description || '',
            isPined: note.isPined || note.isPinned || false,
            isArchived: note.isArchived || false,
            isDeleted: note.isDeleted || false,
            reminder: note.reminder || note.reminderDateTime || note.reminderDate,
            createdDate: note.createdDate || note.createdAt,
            modifiedDate: note.modifiedDate || note.updatedAt || note.modifiedAt,
            color: note.color || '#202124',
            noteLabels: note.noteLabels || note.labels || []
          };

          console.log(' Transformed note:', transformedNote);
          return transformedNote;
        });

        // Filter notes with active reminders using ReminderService
        transformedNotes = this.reminderService.filterNotesWithReminders(transformedNotes);
        console.log(' Notes with active reminders:', transformedNotes.length);

        // Sort notes by reminder time (Google Keep style)
        this.reminderNotes = this.reminderService.sortNotesByReminder(transformedNotes);

        // Update the ReminderService cache
        this.reminderService.updateReminderNotesCache(this.reminderNotes);

        console.log(' Final processed reminder notes:', this.reminderNotes);
        console.log(' Number of reminder notes:', this.reminderNotes.length);

        // Log reminder status breakdown
        const groups = this.reminderService.groupReminderNotesByStatus(this.reminderNotes);
        console.log(' Reminder status breakdown:', {
          overdue: groups.overdue.length,
          today: groups.today.length,
          upcoming: groups.upcoming.length
        });
      },
      error: (err) => {
        console.error(' Failed to load reminders:', err);
        this.reminderNotes = [];
      },
    });
  }

  onEditNote(event: any): void {
    // Handle note editing if needed
    console.log(' Edit note in reminders:', event);
  }

  onDeleteNote(noteId: string): void {
    // Handle note deletion if needed
    console.log(' Delete note in reminders:', noteId);
  }

  onRefreshNotes(): void {
    // Refresh the reminder notes list
    this.loadReminderNotes();
  }
}
