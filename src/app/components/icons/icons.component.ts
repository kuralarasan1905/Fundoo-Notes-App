import { Component, EventEmitter, Output, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-icons',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './icons.component.html',
  styleUrls: ['./icons.component.scss'],
})
export class IconsComponent {
  @Input() showAll: boolean = false;
  @Output() colorSelect = new EventEmitter<string>();
  @Output() closeBtn = new EventEmitter<boolean>();

  showPalette = false;
  colors = ['#333333', '#1a237e', '#004d40', '#4a0000', '#2c3e50', '#2e3b2f'];

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
