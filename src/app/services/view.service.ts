import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ViewService {
  private viewModeSubject = new BehaviorSubject<'grid' | 'list'>('grid');
  viewMode$ = this.viewModeSubject.asObservable();

  setViewMode(mode: 'grid' | 'list') {
    this.viewModeSubject.next(mode);
  }
}
