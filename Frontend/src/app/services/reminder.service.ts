import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { NoteService } from './note_service/note.service';

export interface ReminderOption {
  id: string;
  label: string;
  icon: string;
  getValue: () => Date;
  description: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReminderService {
  private reminderNotesSubject = new BehaviorSubject<any[]>([]);
  public reminderNotes$ = this.reminderNotesSubject.asObservable();

  // Google Keep-style reminder options
  private reminderOptions: ReminderOption[] = [
    {
      id: 'later-today',
      label: 'Later today',
      icon: 'schedule',
      description: '8:00 PM',
      getValue: () => {
        const date = new Date();
        date.setHours(20, 0, 0, 0); // 8 PM today
        return date;
      }
    },
    {
      id: 'tomorrow',
      label: 'Tomorrow',
      icon: 'wb_sunny',
      description: '8:00 AM',
      getValue: () => {
        const date = new Date();
        date.setDate(date.getDate() + 1);
        date.setHours(8, 0, 0, 0); // 8 AM tomorrow
        return date;
      }
    },
    {
      id: 'next-week',
      label: 'Next week',
      icon: 'date_range',
      description: 'Monday 8:00 AM',
      getValue: () => {
        const date = new Date();
        const daysUntilNextMonday = (8 - date.getDay()) % 7 || 7;
        date.setDate(date.getDate() + daysUntilNextMonday);
        date.setHours(8, 0, 0, 0); // 8 AM next Monday
        return date;
      }
    }
  ];

  constructor(private noteService: NoteService) {}

  // Get available reminder options
  getReminderOptions(): ReminderOption[] {
    return this.reminderOptions.map(option => ({
      ...option,
      description: this.getOptionDescription(option)
    }));
  }

  // Get formatted description for reminder option
  private getOptionDescription(option: ReminderOption): string {
    const date = option.getValue();
    const now = new Date();
    
    switch (option.id) {
      case 'later-today':
        return date.toLocaleTimeString('en-US', { 
          hour: 'numeric', 
          minute: '2-digit',
          hour12: true 
        });
      case 'tomorrow':
        return `Tomorrow, ${date.toLocaleTimeString('en-US', { 
          hour: 'numeric', 
          minute: '2-digit',
          hour12: true 
        })}`;
      case 'next-week':
        return `${date.toLocaleDateString('en-US', { 
          weekday: 'long' 
        })}, ${date.toLocaleTimeString('en-US', { 
          hour: 'numeric', 
          minute: '2-digit',
          hour12: true 
        })}`;
      default:
        return option.description;
    }
  }

  // Format reminder time for display (Google Keep style)
  formatReminderTime(reminderDateTime: string): string {
    if (!reminderDateTime) return '';

    const reminderDate = new Date(reminderDateTime);
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const reminderDay = new Date(reminderDate.getFullYear(), reminderDate.getMonth(), reminderDate.getDate());
    
    const diffTime = reminderDay.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    const timeString = reminderDate.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });

    if (diffDays === 0) {
      // Today
      return `Today, ${timeString}`;
    } else if (diffDays === 1) {
      // Tomorrow
      return `Tomorrow, ${timeString}`;
    } else if (diffDays > 1 && diffDays <= 7) {
      // This week
      const dayName = reminderDate.toLocaleDateString('en-US', { weekday: 'long' });
      return `${dayName}, ${timeString}`;
    } else {
      // Future date
      const dateString = reminderDate.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric'
      });
      return `${dateString}, ${timeString}`;
    }
  }

  // Check if reminder is overdue
  isReminderOverdue(reminderDateTime: string): boolean {
    if (!reminderDateTime) return false;
    return new Date(reminderDateTime) < new Date();
  }

  // Get reminder status (upcoming, overdue, today)
  getReminderStatus(reminderDateTime: string): 'upcoming' | 'overdue' | 'today' | 'none' {
    if (!reminderDateTime) return 'none';

    const reminderDate = new Date(reminderDateTime);
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const reminderDay = new Date(reminderDate.getFullYear(), reminderDate.getMonth(), reminderDate.getDate());

    if (reminderDate < now) {
      return 'overdue';
    } else if (reminderDay.getTime() === today.getTime()) {
      return 'today';
    } else {
      return 'upcoming';
    }
  }

  // Add reminder to note
  addReminderToNote(noteId: string, reminderOption: string | Date): Observable<any> {
    console.log(' Adding reminder to note:', noteId, 'Option:', reminderOption);

    let reminderDate: Date;

    if (typeof reminderOption === 'string') {
      // Find the option and get its date
      const option = this.reminderOptions.find(opt => opt.id === reminderOption);
      if (option) {
        reminderDate = option.getValue();
      } else {
        // Try to parse as date string
        reminderDate = new Date(reminderOption);
        if (isNaN(reminderDate.getTime())) {
          throw new Error('Invalid reminder option or date');
        }
      }
    } else {
      reminderDate = reminderOption;
    }

    console.log(' Reminder date calculated:', reminderDate.toISOString());

    return this.noteService.addUpdateReminder({
      noteIdList: [noteId],
      reminder: reminderDate.toISOString()
    });
  }

  // Remove reminder from note
  removeReminderFromNote(noteId: string): Observable<any> {
    console.log(' Removing reminder from note:', noteId);

    return this.noteService.removeReminder({
      noteIdList: [noteId]
    });
  }

  // Load reminder notes
  loadReminderNotes(): Observable<any[]> {
    console.log(' Loading reminder notes...');
    return this.noteService.getReminderList().pipe(
      map((response: any) => {
        // Handle the mock response structure from noteService.getReminderList()
        if (response && response.data && Array.isArray(response.data)) {
          return response.data;
        }
        // Fallback to empty array if response structure is unexpected
        return [];
      })
    );
  }

  // Update local reminder notes cache
  updateReminderNotesCache(notes: any[]): void {
    this.reminderNotesSubject.next(notes);
  }

  // Get cached reminder notes
  getCachedReminderNotes(): any[] {
    return this.reminderNotesSubject.value;
  }

  // Sort notes by reminder time (Google Keep style)
  sortNotesByReminder(notes: any[]): any[] {
    return notes.sort((a, b) => {
      const aReminder = a.reminder || a.reminderDateTime;
      const bReminder = b.reminder || b.reminderDateTime;

      if (!aReminder && !bReminder) return 0;
      if (!aReminder) return 1;
      if (!bReminder) return -1;

      return new Date(aReminder).getTime() - new Date(bReminder).getTime();
    });
  }

  // Filter notes with active reminders
  filterNotesWithReminders(notes: any[]): any[] {
    return notes.filter(note => {
      const reminder = note.reminder || note.reminderDateTime;
      return reminder && reminder.trim() !== '';
    });
  }

  // Group reminder notes by status
  groupReminderNotesByStatus(notes: any[]): {
    overdue: any[];
    today: any[];
    upcoming: any[];
  } {
    const groups = {
      overdue: [] as any[],
      today: [] as any[],
      upcoming: [] as any[]
    };

    notes.forEach(note => {
      const reminder = note.reminder || note.reminderDateTime;
      const status = this.getReminderStatus(reminder);
      
      if (status === 'overdue') {
        groups.overdue.push(note);
      } else if (status === 'today') {
        groups.today.push(note);
      } else if (status === 'upcoming') {
        groups.upcoming.push(note);
      }
    });

    return groups;
  }
}
