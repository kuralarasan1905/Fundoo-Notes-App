import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],
})
export class SidenavComponent {
  @Input() isExpanded: boolean = false;
  selectedLabel: string = '';
  hoverLabel: string = '';

  navItems = [
    { icon: 'lightbulb', label: 'Notes' },
    { icon: 'notifications', label: 'Reminders' },
    { icon: 'edit', label: 'Edit labels' },
    { icon: 'archive', label: 'Archive' },
    { icon: 'delete', label: 'Trash' },
  ];

  select(label: string) {
    this.selectedLabel = label;
  }

  setHover(label: string) {
    this.hoverLabel = label;
  }

  clearHover() {
    this.hoverLabel = '';
  }
}
