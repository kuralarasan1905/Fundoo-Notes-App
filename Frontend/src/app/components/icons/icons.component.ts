import {
  Component,
  EventEmitter,
  Output,
  Input,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { LabelDialogComponent } from '../label-dialog/label-dialog.component';
import { NoteService, Note } from 'src/app/services/note_service/note.service';
import { ReminderService, ReminderOption } from 'src/app/services/reminder.service';

@Component({
  selector: 'app-icons',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule, MatTooltipModule, MatDividerModule],
  templateUrl: './icons.component.html',
  styleUrls: ['./icons.component.scss'],
})
export class IconsComponent implements OnInit, OnDestroy {
  @Input() showAll: boolean = false;
  @Input() isArchived: boolean = false;
  @Input() inTrash: boolean = false;
  @Input() inDialog: boolean = false; // New input to detect if icons are in dialog
  @Input() note?: Note;
  @Output() colorSelect = new EventEmitter<{ color: string; index: number }>();
  @Output() closeBtn = new EventEmitter<boolean>();
  @Output() archiveNote = new EventEmitter<void>();
  @Output() toggleArchive = new EventEmitter<boolean>();
  @Output() moveToTrash = new EventEmitter<void>();
  @Output() deletePermanently = new EventEmitter<void>();
  @Output() restoreNote = new EventEmitter<void>();
  @Output() reminderSet = new EventEmitter<string>();
  @Output() reminderRemove = new EventEmitter<void>();
  @Output() labelsUpdated = new EventEmitter<any>();
  @Output() copyNote = new EventEmitter<void>();
  @Output() addDrawing = new EventEmitter<void>();
  @Output() showCheckboxes = new EventEmitter<void>();
  @Output() copyToGoogleDocs = new EventEmitter<void>();
  @Output() versionHistory = new EventEmitter<void>();

  showPalette = false;
  colors: string[] = [];
  private isDark = true;
  private observer: MutationObserver | undefined;

  selectedColorIndex: number | null = null; // <- Index of selected color
  reminderOptions: ReminderOption[] = []; // Google Keep style reminder options

  constructor(
    private dialog: MatDialog,
    private noteService: NoteService,
    private reminderService: ReminderService
  ) {}

  // Professional color palette (14 solid colors)
  private professionalPalette = [
    '#ffffff', // Default (no color/white)
    '#ff6b6b', // Red
    '#4ecdc4', // Teal
    '#45b7d1', // Blue
    '#96ceb4', // Green
    '#ffeaa7', // Yellow
    '#dda0dd', // Purple
    '#fab1a0', // Orange
    '#fd79a8', // Pink
    '#6c5ce7', // Indigo
    '#a29bfe', // Light purple
    '#fdcb6e', // Gold
    '#e17055', // Brown
    '#b2bec3', // Gray
  ];

  ngOnInit() {
    this.setPalette();

    // Initialize reminder options from ReminderService
    this.reminderOptions = this.reminderService.getReminderOptions();

    this.observer = new MutationObserver(() => {
      const prevTheme = this.isDark;
      this.setPalette();

      if (this.selectedColorIndex !== null && this.isDark !== prevTheme) {
        const newColor = this.colors[this.selectedColorIndex];
        this.colorSelect.emit({
          color: newColor,
          index: this.selectedColorIndex,
        });
      }
    });

    this.observer.observe(document.body, {
      attributes: true,
      attributeFilter: ['class'],
    });
  }

  ngOnDestroy() {
    this.observer?.disconnect();
  }

  private setPalette() {
    // Use the same professional palette for both light and dark themes
    this.colors = this.professionalPalette;
  }

  togglePalette() {
    this.showPalette = !this.showPalette;
  }

  selectColor(color: string) {
    const index = this.colors.indexOf(color);
    if (index !== -1) {
      this.selectedColorIndex = index;
      this.colorSelect.emit({ color: color, index });
      this.showPalette = false;
    }
  }

  // Get tooltip text for color swatches
  getColorTooltip(color: string, index: number): string {
    if (index === 0) {
      return 'Default';
    }

    // Professional color name mapping
    const colorNames: { [key: string]: string } = {
      '#ffffff': 'Default',
      '#ff6b6b': 'Red',
      '#4ecdc4': 'Teal',
      '#45b7d1': 'Blue',
      '#96ceb4': 'Green',
      '#ffeaa7': 'Yellow',
      '#dda0dd': 'Purple',
      '#fab1a0': 'Orange',
      '#fd79a8': 'Pink',
      '#6c5ce7': 'Indigo',
      '#a29bfe': 'Light Purple',
      '#fdcb6e': 'Gold',
      '#e17055': 'Brown',
      '#b2bec3': 'Gray'
    };

    return colorNames[color] || 'Color';
  }

  // Set the current selected color (useful when opening palette)
  setSelectedColor(color: string) {
    const index = this.colors.indexOf(color);
    if (index !== -1) {
      this.selectedColorIndex = index;
    }
  }

  closeBox(): void {
    this.closeBtn.emit(true);
  }

  setReminder(option: string) {
    console.log(' Setting reminder option:', option);

    try {
      // Use ReminderService to get the correct date for the option
      const reminderOption = this.reminderOptions.find(opt =>
        opt.id === option ||
        opt.id === `${option}` ||
        opt.id === `later-${option}` ||
        (option === 'today' && opt.id === 'later-today') ||
        (option === 'nextWeek' && opt.id === 'next-week')
      );

      if (reminderOption) {
        const reminderDate = reminderOption.getValue();
        const isoString = reminderDate.toISOString();
        console.log(' Reminder date from service:', isoString);
        console.log(' Formatted display:', this.reminderService.formatReminderTime(isoString));
        this.reminderSet.emit(isoString);
      } else {
        // Fallback to legacy logic for backward compatibility
        console.log(' Using fallback reminder logic for option:', option);
        this.setReminderFallback(option);
      }
    } catch (error) {
      console.error(' Error setting reminder:', error);
      this.setReminderFallback(option);
    }
  }

  // Fallback method for backward compatibility
  private setReminderFallback(option: string) {
    const now = new Date();
    let reminderDate: Date | undefined;

    switch (option) {
      case 'today':
        reminderDate = new Date(now);
        reminderDate.setHours(20, 0, 0, 0); // 8 PM today
        break;
      case 'tomorrow':
        reminderDate = new Date(now);
        reminderDate.setDate(now.getDate() + 1);
        reminderDate.setHours(8, 0, 0, 0); // 8 AM tomorrow
        break;
      case 'nextWeek':
        reminderDate = new Date(now);
        reminderDate.setDate(now.getDate() + 7);
        reminderDate.setHours(8, 0, 0, 0); // 8 AM next week
        break;
    }

    if (reminderDate) {
      const isoString = reminderDate.toISOString();
      console.log(' Fallback reminder date:', isoString);
      this.reminderSet.emit(isoString);
    } else {
      console.error(' No reminder date set for option:', option);
    }
  }

  openLabelDialog(): void {
    if (this.note) {
      const dialogRef = this.dialog.open(LabelDialogComponent, {
        data: { note: this.note },
        width: '400px',
        panelClass: 'custom-dialog-container'
      });

      dialogRef.afterClosed().subscribe((updatedLabels) => {
        if (updatedLabels) {
          this.labelsUpdated.emit(updatedLabels);
        }
      });
    }
  }

  onCopyNote(): void {
    if (this.note) {
      this.noteService.copyNote(this.note).subscribe({
        next: (copiedNote) => {
          console.log('Note copied successfully:', copiedNote);
          this.copyNote.emit();
        },
        error: (err) => {
          console.error('Failed to copy note:', err);
        }
      });
    }
  }

  onAddDrawing(): void {
    console.log('Add drawing functionality - to be implemented');
    this.addDrawing.emit();
  }

  onShowCheckboxes(): void {
    console.log('Show checkboxes functionality - to be implemented');
    this.showCheckboxes.emit();
  }

  onCopyToGoogleDocs(): void {
    if (this.note) {
      // Create a simple text version for copying
      const noteText = `${this.note.title}\n\n${this.note.previewContent}`;
      navigator.clipboard.writeText(noteText).then(() => {
        console.log('Note content copied to clipboard');
      }).catch(err => {
        console.error('Failed to copy to clipboard:', err);
      });
    }
    this.copyToGoogleDocs.emit();
  }

  onVersionHistory(): void {
    console.log('Version history functionality - to be implemented');
    this.versionHistory.emit();
  }
}
