import {
  Component,
  EventEmitter,
  HostListener,
  OnInit,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';

@Component({
  selector: 'app-toolbar',
  standalone: true,
  imports: [CommonModule, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit {
  @Output() toggle = new EventEmitter<void>();
  @Output() viewToggle = new EventEmitter<boolean>();

  isSearchOpen = false;
  isLargeScreen = window.innerWidth > 796;
  isGridView = false;

  ngOnInit(): void {
    this.updateScreenSize();
  }

  @HostListener('window:resize')
  updateScreenSize() {
    this.isLargeScreen = window.innerWidth > 796;
    if (this.isLargeScreen) this.isSearchOpen = false;
  }

  openSearch() {
    this.isSearchOpen = true;
  }

  closeSearch() {
    this.isSearchOpen = false;
  }

  toggleView() {
    this.isGridView = !this.isGridView;
    this.viewToggle.emit(this.isGridView);
  }
}
