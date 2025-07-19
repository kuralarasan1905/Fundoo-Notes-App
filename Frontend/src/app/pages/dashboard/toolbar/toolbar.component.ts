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
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { filter } from 'rxjs/operators';
import { AccountService } from 'src/app/services/account.service';
import { User } from 'src/app/model/user';

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
    MatTooltipModule,
    MatDividerModule,
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
  isDarkTheme = true;
  title: string = 'Keep';
  currentUser: User | null = null;

  constructor(
    private searchService: SearchService,
    private router: Router,
    private route: ActivatedRoute,
    private accountService: AccountService
  ) {}

  onSearchInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchService.setSearch(input.value);
  }

  ngOnInit(): void {
    this.updateScreenSize();

    this.viewToggle.emit(this.isGridView);

    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
      this.isDarkTheme = true;
      document.body.classList.add('dark-theme');
    } else {
      this.isDarkTheme = false;
      document.body.classList.remove('dark-theme');
    }

    // Subscribe to current user
    this.accountService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        let currentRoute = this.route.root;

        while (currentRoute.firstChild) {
          currentRoute = currentRoute.firstChild;
        }

        const currentPath = currentRoute.snapshot.routeConfig?.path;

        switch (currentPath) {
          case 'reminders':
            this.title = 'Reminders';
            break;
          case 'edit-labels':
            this.title = 'Edit Labels';
            break;
          case 'archive':
            this.title = 'Archive';
            break;
          case 'trash':
            this.title = 'Trash';
            break;
          default:
            this.title = 'Keep';
        }
      });
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;

    if (this.isDarkTheme) {
      document.body.classList.add('dark-theme');
      localStorage.setItem('theme', 'dark');
    } else {
      document.body.classList.remove('dark-theme');
      localStorage.setItem('theme', 'light');
    }
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

  // Account methods
  getInitials(): string {
    if (!this.currentUser) return 'U';
    return `${this.currentUser.firstName.charAt(0)}${this.currentUser.lastName.charAt(0)}`.toUpperCase();
  }

  navigateToAccount(): void {
    this.router.navigate(['/dashboard/account']);
  }

  logout(): void {
    this.accountService.logout();
    this.router.navigate(['/login']);
  }
}
