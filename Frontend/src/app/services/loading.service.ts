import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor() { }

  // Show loading spinner
  show() {
    this.loadingSubject.next(true);
  }

  // Hide loading spinner
  hide() {
    this.loadingSubject.next(false);
  }

  // Set loading state
  setLoading(loading: boolean) {
    this.loadingSubject.next(loading);
  }
}
