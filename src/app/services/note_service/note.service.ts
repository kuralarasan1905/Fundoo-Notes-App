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
}
