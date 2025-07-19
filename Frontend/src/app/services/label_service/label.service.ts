import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpService } from '../http_service/http.service';

@Injectable({
  providedIn: 'root',
})
export class LabelService {
  // Subject to notify components when labels are updated
  private labelsUpdated = new Subject<void>();

  // Observable that components can subscribe to
  labelsUpdated$ = this.labelsUpdated.asObservable();

  constructor(private http: HttpService) {}

  // Method to notify all subscribers that labels have been updated
  private notifyLabelsUpdated(): void {
    this.labelsUpdated.next();
  }

  // Get all labels
  getAllLabels(): Observable<any> {
    const headers = this.http.getHeader();
    return this.http.getApi('/Labels', headers);
  }

  // Create a new label
  createLabel(payload: { label: string }): Observable<any> {
    const headers = this.http.getHeader();
    // Format payload for ASP.NET Core backend (Pascal case)
    // Backend expects 'Name' and 'Color' properties based on CreateLabelCommand
    const formattedPayload = {
      Name: payload.label,
      Color: '#ffffff' // Default color
    };
    const result = this.http.postApi('/Labels', formattedPayload, headers);

    // Notify all subscribers when label is created successfully
    result.subscribe({
      next: () => this.notifyLabelsUpdated(),
      error: () => {} // Don't notify on error
    });

    return result;
  }

  // Update a label
  updateLabel(payload: { id: string; label: string }): Observable<any> {
    const headers = this.http.getHeader();
    // Format payload for ASP.NET Core backend (Pascal case)
    // Backend expects 'Name' property based on UpdateLabelCommand
    const formattedPayload = {
      Name: payload.label
    };
    const result = this.http.putApi(`/Labels/${payload.id}`, formattedPayload, headers);

    // Notify all subscribers when label is updated successfully
    result.subscribe({
      next: () => this.notifyLabelsUpdated(),
      error: () => {} // Don't notify on error
    });

    return result;
  }

  // Delete a label
  deleteLabel(labelId: string): Observable<any> {
    const headers = this.http.getHeader();
    const result = this.http.deleteApi(`/Labels/${labelId}`, headers);

    // Notify all subscribers when label is deleted successfully
    result.subscribe({
      next: () => this.notifyLabelsUpdated(),
      error: () => {} // Don't notify on error
    });

    return result;
  }

  // Add label to note - Updated to use proper Swagger API endpoint
  addLabelToNote(payload: { noteId: string; labelId: string }): Observable<any> {
    const headers = this.http.getHeader();

    // Format payload for ASP.NET Core backend (Pascal case)
    // Based on Swagger API documentation
    const formattedPayload = {
      NoteId: parseInt(payload.noteId), // Convert to int if needed
      LabelId: parseInt(payload.labelId) // Convert to int if needed
    };

    return this.http.postApi('/Notes/addLabelToNote', formattedPayload, headers);
  }

  // Remove label from note - Updated to use proper Swagger API endpoint
  removeLabelFromNote(payload: { noteId: string; labelId: string }): Observable<any> {
    const headers = this.http.getHeader();

    // Format payload for ASP.NET Core backend (Pascal case)
    // Based on Swagger API documentation
    const formattedPayload = {
      NoteId: parseInt(payload.noteId), // Convert to int if needed
      LabelId: parseInt(payload.labelId) // Convert to int if needed
    };

    return this.http.postApi('/Notes/removeLabelFromNote', formattedPayload, headers);
  }

  // Bulk add labels to multiple notes
  addLabelsToNotes(payload: { noteIds: string[]; labelIds: string[] }): Observable<any> {
    const headers = this.http.getHeader();

    // Format payload for ASP.NET Core backend (Pascal case)
    const formattedPayload = {
      NoteIds: payload.noteIds.map(id => parseInt(id)),
      LabelIds: payload.labelIds.map(id => parseInt(id))
    };

    return this.http.postApi('/Notes/addLabelsToNotes', formattedPayload, headers);
  }

  // Bulk remove labels from multiple notes
  removeLabelsFromNotes(payload: { noteIds: string[]; labelIds: string[] }): Observable<any> {
    const headers = this.http.getHeader();

    // Format payload for ASP.NET Core backend (Pascal case)
    const formattedPayload = {
      NoteIds: payload.noteIds.map(id => parseInt(id)),
      LabelIds: payload.labelIds.map(id => parseInt(id))
    };

    return this.http.postApi('/Notes/removeLabelsFromNotes', formattedPayload, headers);
  }

  // Update note labels (replace all labels for a note)
  updateNoteLabels(payload: { noteId: string; labelIds: string[] }): Observable<any> {
    const headers = this.http.getHeader();

    // Format payload for ASP.NET Core backend (Pascal case)
    const formattedPayload = {
      NoteId: parseInt(payload.noteId),
      LabelIds: payload.labelIds.map(id => parseInt(id))
    };

    return this.http.postApi('/Notes/updateNoteLabels', formattedPayload, headers);
  }

  // Get notes by label
  getNotesByLabel(labelId: string): Observable<any> {
    const headers = this.http.getHeader();
    return this.http.getApi(`/Notes/byLabel/${labelId}`, headers);
  }
}
