import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { SidenavComponent } from './sidenav/sidenav.component';
// import { NotesComponent } from 'src/app/components/notes/notes.component';
// import { NoteCardComponent } from 'src/app/components/note-card/note-card.component';
// import { Note } from 'src/app/model/note';
import { RouterLink, RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatSidenavModule,
    ToolbarComponent,
    SidenavComponent,
    RouterLink,
    RouterModule,

    // NotesComponent,
    // NoteCardComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent {
  isSidebarOpen = false;
  // notes: Note[] = [];

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  // handleNoteAdded(note: Note) {
  //   const newNote = { ...note, id: Date.now().toString() };
  //   this.notes.unshift(newNote);
  // }

  // handleNoteEdit(updated: {
  //   noteId: string;
  //   title: string;
  //   description: string;
  // }) {
  //   const index = this.notes.findIndex((note) => note.id === updated.noteId);
  //   if (index > -1) {
  //     this.notes[index].title = updated.title;
  //     this.notes[index].description = updated.description;
  //   }
  // }

  // handleNoteDelete(noteId: string) {
  //   this.notes = this.notes.filter((note) => note.id !== noteId);
  // }
}
