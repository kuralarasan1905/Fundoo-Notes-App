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
import { FormsModule } from '@angular/forms';
import { SearchService } from 'src/app/search.service';
import { Router } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-toolbar',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    FormsModule,
    MatMenuModule,
  ],
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit {
  @Output() toggle = new EventEmitter<void>();
  @Output() viewToggle = new EventEmitter<boolean>();
  @Output() search = new EventEmitter<string>();
  searchQuery = '';
  isSearchOpen = false;
  isLargeScreen = window.innerWidth >= 600;
  isGridView = this.isLargeScreen;

  constructor(private searchService: SearchService, private router: Router) {}

  onSearchInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchService.setSearch(input.value);
  }

  ngOnInit(): void {
    this.updateScreenSize();
    this.viewToggle.emit(this.isGridView);
  }

  @HostListener('window:resize')
  updateScreenSize() {
    this.isLargeScreen = window.innerWidth >= 600;
    if (!this.isLargeScreen) {
      this.isGridView = false;
      this.viewToggle.emit(false);
    }
    if (this.isLargeScreen) {
      this.isSearchOpen = false;
    }
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

  onRefresh() {
    const currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
  }
}
