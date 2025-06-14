import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, RouterModule],
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],
})
export class SidenavComponent implements OnInit {
  @Input() isExpanded: boolean = false;
  selectedLabel: string = '';
  hoverLabel: string = '';

  navItems = [
    { icon: 'lightbulb_outline', label: 'Notes', route: 'notes' },
    { icon: 'notifications', label: 'Reminders', route: 'reminders' },
    { icon: 'edit', label: 'Edit labels', route: 'edit-labels' },
    { icon: 'archive', label: 'Archive', route: 'archive' },
    { icon: 'delete_outline', label: 'Trash', route: 'trash' },
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.updateSelectedLabel(this.router.url);

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event) => {
        const url = (event as NavigationEnd).urlAfterRedirects;
        this.updateSelectedLabel(url);
      });
  }

  updateSelectedLabel(url: string) {
    const path = url.split('/').pop();
    const found = this.navItems.find((item) => item.route === path);
    if (found) {
      this.selectedLabel = found.label;
    }
  }

  select(label: string, route: string) {
    this.selectedLabel = label;
    this.router.navigate(['/dashboard', route]);
  }

  setHover(label: string) {
    this.hoverLabel = label;
  }

  clearHover() {
    this.hoverLabel = '';
  }
}
