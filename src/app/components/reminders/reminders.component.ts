import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NoteService } from 'src/app/services/note_service/note.service';
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
    this.noteService.getReminderList().subscribe({
      next: (res: any) => {
        this.reminderNotes = res.data.data;
      },
      error: (err) => console.error('Failed to load reminders', err),
    });
  }

  onEditNote(event: any): void {}
  onDeleteNote(noteId: string): void {}
}
