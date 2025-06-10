import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { IconsComponent } from '../icons/icons.component';
import { Note } from 'src/app/model/note';
import { NoteCardComponent } from '../note-card/note-card.component';
import { Subscription } from 'rxjs';
import { ViewService } from 'src/app/services/view.service';

@Component({
  selector: 'app-notes',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    IconsComponent,
    NoteCardComponent,
  ],
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss'],
})
export class NotesComponent implements OnInit, OnDestroy {
  notes: FormGroup;
  isExpanded = false;
  noteList: Note[] = [];

  selectedColor = '#202124';

  viewMode: 'grid' | 'list' = 'grid';
  private viewSub!: Subscription;

  onColorSelected(color: string) {
    this.selectedColor = color;
  }

  constructor(private viewService: ViewService, private fb: FormBuilder) {
    this.notes = this.fb.group({
      title: [''],
      description: [''],
    });
  }

  ngOnInit() {
    this.viewSub = this.viewService.viewMode$.subscribe((mode) => {
      this.viewMode = mode;
    });
  }

  ngOnDestroy() {
    this.viewSub?.unsubscribe();
  }

  expandForm(): void {
    this.isExpanded = true;
  }

  onCloseNote(shouldSave: boolean) {
    const formValue = this.notes.value;

    if (
      shouldSave &&
      (formValue.title?.trim() || formValue.description?.trim())
    ) {
      const newNote: Note = {
        id: Date.now().toString(),
        title: formValue.title.trim(),
        description: formValue.description.trim(),
        color: this.selectedColor,
      };

      this.noteList.unshift(newNote);
    }

    this.notes.reset();
    this.selectedColor = '#202124';
    this.isExpanded = false;
  }

  onEditNote(updated: {
    noteId: string;
    title: string;
    description: string;
    color: string;
  }) {
    const index = this.noteList.findIndex((n) => n.id === updated.noteId);
    if (index > -1) {
      this.noteList[index] = {
        ...this.noteList[index],
        title: updated.title,
        description: updated.description,
        color: updated.color,
      };
    }
  }

  onDeleteNote(noteId: string) {
    this.noteList = this.noteList.filter((n) => n.id !== noteId);
  }
}
