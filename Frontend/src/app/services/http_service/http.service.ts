import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  baseUrl = 'https://localhost:7256/api';
  
  // JWT Authentication control
  private enableJwtAuth = true; // Enable by default for production

  constructor(private http: HttpClient) {
  }

  // Headers with JWT authentication
  getHeader() {
    const token = localStorage.getItem('token');

    // Check if JWT authentication is enabled
    if (!this.enableJwtAuth) {
      const header = new HttpHeaders({
        'Content-Type': 'application/json'
      });
      return header;
    }

    if (!token) {
      // Return headers without Authorization (will likely fail with 401)
      const header = new HttpHeaders({
        'Content-Type': 'application/json'
      });
      return header;
    }

    // Create headers with JWT token for backend validation
    const header = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    // Validate token format
    if (token) {
      try {
        const tokenParts = token.split('.');
        if (tokenParts.length === 3) {
          // Check if token is expired
          const payload = JSON.parse(atob(tokenParts[1]));
          const expiry = new Date(payload.exp * 1000);
          const now = new Date();

          if (expiry < now) {
            console.error('JWT token expired - user needs to log in again');
          }
        }
      } catch (e) {
        console.error('Error parsing JWT token:', e);
      }
    }

    return header;
  }

  // Simple GET method
  getApi(endpoint: string, headers: HttpHeaders = new HttpHeaders()) {
    const fullUrl = this.baseUrl + endpoint;
    return this.http.get(fullUrl, { headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`GET ${endpoint} failed:`, error);
        return throwError(() => error);
      })
    );
  }

  // Simple POST method
  postApi(endpoint: string, payload: any, headers: HttpHeaders = new HttpHeaders()) {
    const fullUrl = this.baseUrl + endpoint;
    return this.http.post(fullUrl, payload, { headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`POST ${endpoint} failed:`, error);
        return throwError(() => error);
      })
    );
  }

  // Simple PUT method
  putApi(endpoint: string, payload: any, headers: HttpHeaders = new HttpHeaders()) {
    const fullUrl = this.baseUrl + endpoint;
    return this.http.put(fullUrl, payload, { headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`PUT ${endpoint} failed:`, error);
        return throwError(() => error);
      })
    );
  }

  // Simple PATCH method
  patchApi(endpoint: string, payload: any, headers: HttpHeaders = new HttpHeaders()) {
    const fullUrl = this.baseUrl + endpoint;
    return this.http.patch(fullUrl, payload, { headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`PATCH ${endpoint} failed:`, error);
        return throwError(() => error);
      })
    );
  }

  // Simple DELETE method
  deleteApi(endpoint: string, headers: HttpHeaders = new HttpHeaders()) {
    const fullUrl = this.baseUrl + endpoint;
    return this.http.delete(fullUrl, { headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`DELETE ${endpoint} failed:`, error);
        return throwError(() => error);
      })
    );
  }

  // Enable JWT authentication
  enableAuthentication(): void {
    this.enableJwtAuth = true;
  }

  // Disable JWT authentication
  disableAuthentication(): void {
    this.enableJwtAuth = false;
  }
}
