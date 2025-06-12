import { Component, EventEmitter, Output, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-icons',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule],
  templateUrl: './icons.component.html',
  styleUrls: ['./icons.component.scss'],
})
export class IconsComponent {
  @Input() showAll: boolean = false;
  @Input() isArchived: boolean = false;
  @Input() inTrash: boolean = false;
  @Output() colorSelect = new EventEmitter<string>();
  @Output() closeBtn = new EventEmitter<boolean>();
  @Output() archiveNote = new EventEmitter<void>();
  @Output() toggleArchive = new EventEmitter<boolean>();
  @Output() moveToTrash = new EventEmitter<void>();
  @Output() deletePermanently = new EventEmitter<void>();
  @Output() restoreNote = new EventEmitter<void>();

  showPalette = false;
  colors = [
    '#77172e',
    '#692b17',
    '#7c4a03',
    '#264d3b',
    '#0c625d',
    '#256377',
    '#284255',
    '#472e5b',
    '#6c394f',
    '#4b443a',
  ];

  togglePalette() {
    this.showPalette = !this.showPalette;
  }

  selectColor(color: string) {
    this.colorSelect.emit(color);
    this.showPalette = false;
  }

  closeBox(): void {
    this.closeBtn.emit(true);
  }
}
