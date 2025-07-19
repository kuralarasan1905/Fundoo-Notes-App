import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { HttpService } from '../http_service/http.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpService) {
  }

  login(payload: any): Observable<any> {
    return this.http.postApi('/auth/login/', payload).pipe(
      tap((response: any) => {
        // Store user data if available
        if (response.user) {
          localStorage.setItem('userData', JSON.stringify(response.user));
        }
      }),
      catchError((error) => {
        console.error('Login error:', error);
        throw error;
      })
    );
  }

  register(payload: any): Observable<any> {
    return this.http.postApi('/auth/register', payload);
  }
}
