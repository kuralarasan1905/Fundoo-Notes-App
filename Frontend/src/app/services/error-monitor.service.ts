import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ErrorLog {
  id: string;
  timestamp: Date;
  type: 'network' | 'auth' | 'api' | 'cors' | 'unknown';
  message: string;
  details: any;
  url?: string;
  status?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorMonitorService {
  private errorsSubject = new BehaviorSubject<ErrorLog[]>([]);
  public errors$ = this.errorsSubject.asObservable();

  constructor() {
    // Make error monitoring available globally for development debugging only
    if (this.isDevelopment()) {
      (window as any).getErrors = () => this.getErrors();
      (window as any).clearErrors = () => this.clearErrors();
      (window as any).analyzeErrors = () => this.analyzeErrors();
    }
  }

  // Check if running in development mode
  private isDevelopment(): boolean {
    return !window.location.hostname.includes('production') && 
           (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1');
  }

  // Log an error
  logError(error: any, context?: string): void {
    const errorLog: ErrorLog = {
      id: this.generateId(),
      timestamp: new Date(),
      type: this.categorizeError(error),
      message: this.extractErrorMessage(error),
      details: error,
      url: error.url || context,
      status: error.status
    };

    const currentErrors = this.errorsSubject.value;
    this.errorsSubject.next([...currentErrors, errorLog]);

    // Only log to console in development mode
    if (this.isDevelopment()) {
      console.group(` ERROR LOGGED [${errorLog.type.toUpperCase()}]`);
      console.error('Message:', errorLog.message);
      console.error('Type:', errorLog.type);
      console.error('Status:', errorLog.status);
      console.error('URL:', errorLog.url);
      console.error('Details:', errorLog.details);
      console.error('Timestamp:', errorLog.timestamp.toISOString());
      console.groupEnd();
    }
  }

  // Get all errors (development only)
  getErrors(): ErrorLog[] {
    const errors = this.errorsSubject.value;
    if (this.isDevelopment()) {
      console.table(errors.map(e => ({
        Time: e.timestamp.toLocaleTimeString(),
        Type: e.type,
        Status: e.status,
        Message: e.message,
        URL: e.url
      })));
    }
    return errors;
  }

  // Clear all errors
  clearErrors(): void {
    this.errorsSubject.next([]);
    if (this.isDevelopment()) {
      console.log('🧹 Error log cleared');
    }
  }

  // Analyze error patterns (development only)
  analyzeErrors(): void {
    if (!this.isDevelopment()) return;

    const errors = this.errorsSubject.value;
    
    console.group(' ERROR ANALYSIS');
    console.log(`Total errors: ${errors.length}`);
    
    if (errors.length === 0) {
      console.log(' No errors to analyze');
      console.groupEnd();
      return;
    }

    // Group by type
    const byType = errors.reduce((acc, error) => {
      acc[error.type] = (acc[error.type] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    console.log('Errors by type:', byType);

    // Group by status code
    const byStatus = errors.reduce((acc, error) => {
      const status = error.status || 'unknown';
      acc[status] = (acc[status] || 0) + 1;
      return acc;
    }, {} as Record<string | number, number>);

    console.log('Errors by status:', byStatus);

    // Recent errors (last 5 minutes)
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    const recentErrors = errors.filter(e => e.timestamp > fiveMinutesAgo);
    console.log(`Recent errors (last 5 min): ${recentErrors.length}`);

    // Common error patterns
    this.identifyCommonPatterns(errors);

    console.groupEnd();
  }

  // Identify common error patterns and provide solutions (development only)
  private identifyCommonPatterns(errors: ErrorLog[]): void {
    if (!this.isDevelopment()) return;

    console.log('\n COMMON PATTERNS & SOLUTIONS:');

    // Check for CORS errors
    const corsErrors = errors.filter(e => 
      e.status === 0 || 
      e.message.toLowerCase().includes('cors') ||
      e.message.toLowerCase().includes('access-control')
    );
    
    if (corsErrors.length > 0) {
      console.warn(` CORS Issues (${corsErrors.length} errors):`);
      console.log(' Solution: Configure CORS in your ASP.NET Core backend');
    }

    // Check for auth errors
    const authErrors = errors.filter(e => e.status === 401);
    if (authErrors.length > 0) {
      console.warn(`🔐 Authentication Issues (${authErrors.length} errors):`);
      console.log(' Solution: Check if user is logged in and token is valid');
    }

    // Check for network errors
    const networkErrors = errors.filter(e => e.status === 0 && !e.message.includes('cors'));
    if (networkErrors.length > 0) {
      console.warn(` Network Issues (${networkErrors.length} errors):`);
      console.log(' Solution: Check if backend server is running');
    }

    // Check for 404 errors
    const notFoundErrors = errors.filter(e => e.status === 404);
    if (notFoundErrors.length > 0) {
      console.warn(` Endpoint Not Found (${notFoundErrors.length} errors):`);
      console.log(' Solution: Verify API endpoint URLs and controller routes');
    }

    // Check for 500 errors
    const serverErrors = errors.filter(e => e.status === 500);
    if (serverErrors.length > 0) {
      console.warn(`🔥 Server Errors (${serverErrors.length} errors):`);
      console.log(' Solution: Check backend logs for exceptions');
    }
  }

  // Categorize error type
  private categorizeError(error: any): 'network' | 'auth' | 'api' | 'cors' | 'unknown' {
    if (error.status === 0) {
      if (error.message?.toLowerCase().includes('cors') || 
          error.message?.toLowerCase().includes('access-control')) {
        return 'cors';
      }
      return 'network';
    }
    
    if (error.status === 401 || error.status === 403) {
      return 'auth';
    }
    
    if (error.status >= 400 && error.status < 600) {
      return 'api';
    }
    
    return 'unknown';
  }

  // Extract meaningful error message
  private extractErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }
    
    if (error.message) {
      return error.message;
    }
    
    if (error.error?.message) {
      return error.error.message;
    }
    
    if (error.statusText) {
      return error.statusText;
    }
    
    return 'Unknown error';
  }

  // Generate unique ID
  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substring(2);
  }

  // Get error statistics
  getErrorStats(): any {
    const errors = this.errorsSubject.value;
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000);
    
    return {
      total: errors.length,
      lastHour: errors.filter(e => e.timestamp > oneHourAgo).length,
      byType: errors.reduce((acc, error) => {
        acc[error.type] = (acc[error.type] || 0) + 1;
        return acc;
      }, {} as Record<string, number>),
      mostRecent: errors[errors.length - 1]
    };
  }
}
