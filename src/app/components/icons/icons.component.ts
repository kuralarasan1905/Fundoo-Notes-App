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

@Component({
  selector: 'app-icons',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule],
  templateUrl: './icons.component.html',
  styleUrls: ['./icons.component.scss'],
})
export class IconsComponent implements OnInit, OnDestroy {
  @Input() showAll: boolean = false;
  @Input() isArchived: boolean = false;
  @Input() inTrash: boolean = false;
  @Output() colorSelect = new EventEmitter<{ color: string; index: number }>();
  @Output() closeBtn = new EventEmitter<boolean>();
  @Output() archiveNote = new EventEmitter<void>();
  @Output() toggleArchive = new EventEmitter<boolean>();
  @Output() moveToTrash = new EventEmitter<void>();
  @Output() deletePermanently = new EventEmitter<void>();
  @Output() restoreNote = new EventEmitter<void>();

  showPalette = false;
  colors: string[] = [];
  private isDark = true;
  private observer: MutationObserver | undefined;

  selectedColorIndex: number | null = null; // <- Index of selected color

  private darkPalette = [
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

  private lightPalette = [
    '#f28b82',
    '#fbbc04',
    '#fff475',
    '#ccff90',
    '#a7ffeb',
    '#cbf0f8',
    '#aecbfa',
    '#d7aefb',
    '#fdcfe8',
    '#e6c9a8',
  ];

  ngOnInit() {
    this.setPalette();

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
    this.isDark = document.body.classList.contains('dark-theme');
    this.colors = this.isDark ? this.darkPalette : this.lightPalette;
  }

  togglePalette() {
    this.showPalette = !this.showPalette;
  }

  selectColor(color: string) {
    const index = this.colors.indexOf(color);
    if (index !== -1) {
      this.selectedColorIndex = index;
      this.colorSelect.emit({ color, index });
      this.showPalette = false;
    }
  }

  closeBox(): void {
    this.closeBtn.emit(true);
  }
}
