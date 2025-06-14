import { Injectable } from '@angular/core';
import { HttpService } from '../http_service/http.service';

@Injectable({
  providedIn: 'root',
})
export class NoteService {
  constructor(private http: HttpService) {}

  addNotes(payload: any) {
    const headers = this.http.getHeader();
    return this.http.postApi('/notes/addNotes/', payload, headers);
  }

  getAllNotes() {
    return this.http.getApi('/notes/getNotesList', this.http.getHeader());
  }

  postArchiveList(payload: { noteIdList: string[]; isArchived: boolean }) {
    let headers = this.http.getHeader();
    return this.http.postApi('/notes/archiveNotes', payload, headers);
  }

  getArchiveNoteListApiCall() {
    let headers = this.http.getHeader();
    return this.http.getApi('/notes/getArchiveNotesList', headers);
  }

  postTrashNote(payload: { noteIdList: string[]; isDeleted: boolean }) {
    const headers = this.http.getHeader();
    return this.http.postApi('/notes/trashNotes', payload, headers);
  }

  getTrashList() {
    let headers = this.http.getHeader();
    return this.http.getApi('/notes/getTrashNotesList', headers);
  }

  deleteForeverNotes(payload: { noteIdList: string[]; isDeleted: boolean }) {
    const headers = this.http.getHeader();
    return this.http.postApi('/notes/deleteForeverNotes', payload, headers);
  }

  updateNotes(payload: any) {
    const headers = this.http.getHeader();
    return this.http.postApi('/notes/updateNotes', payload, headers);
  }

  pinUnpinNotes(payload: any) {
    const headers = this.http.getHeader();
    return this.http.postApi('/notes/pinUnpinNotes', payload, headers);
  }

  changeColor(payload: any) {
    const headers = this.http.getHeader();
    return this.http.postApi('/notes/changesColorNotes', payload, headers);
  }
}
