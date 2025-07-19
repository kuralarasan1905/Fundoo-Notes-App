import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { SidenavComponent } from './sidenav/sidenav.component';
import { RouterModule } from '@angular/router';
import { ViewService } from 'src/app/services/view.service';
import { HttpService } from 'src/app/services/http_service/http.service';
import { LabelService } from 'src/app/services/label_service/label.service';
import { NoteService } from 'src/app/services/note_service/note.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatSidenavModule,
    ToolbarComponent,
    SidenavComponent,
    RouterModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  isSidebarOpen = false;
  searchText = '';

  constructor(
    private viewService: ViewService,
    private httpService: HttpService,
    private labelService: LabelService,
    private noteService: NoteService
  ) {}

  ngOnInit() {
    // Add debug methods to window for testing
    (window as any).testEditLabels = () => this.testEditLabelsFeature();
    (window as any).testTrashFeature = () => this.testTrashFeature();
    (window as any).testBackendConnection = () => this.testBackendConnection();

    console.log('🔧 Debug methods available:');
    console.log('  - testEditLabels() - Test edit labels functionality');
    console.log('  - testTrashFeature() - Test trash functionality');
    console.log('  - testBackendConnection() - Test backend connectivity');
  }

  onSearchChange(query: string) {
    this.searchText = query.toLowerCase();
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  handleViewToggle(isGrid: boolean) {
    this.viewService.setViewMode(isGrid ? 'grid' : 'list');
  }

  // Debug method to test edit labels functionality
  testEditLabelsFeature() {
    console.group('TESTING EDIT LABELS FEATURE');

    // Test 1: Get all labels
    console.log('Test 1: Getting all labels...');
    this.labelService.getAllLabels().subscribe({
      next: (response) => {
        console.log(' Get labels SUCCESS:', response);

        // Test 2: Create a test label
        console.log('Test 2: Creating test label...');
        this.labelService.createLabel({ label: 'Test Label ' + Date.now() }).subscribe({
          next: (createResponse) => {
            console.log(' Create label SUCCESS:', createResponse);
            console.log('🎉 Edit Labels feature is working!');
          },
          error: (createError) => {
            console.error(' Create label FAILED:', createError);
            this.diagnoseError(createError, 'Create Label');
          }
        });
      },
      error: (error) => {
        console.error(' Get labels FAILED:', error);
        this.diagnoseError(error, 'Get Labels');
      }
    });

    console.groupEnd();
  }

  // Debug method to test trash functionality
  testTrashFeature() {
    console.group(' TESTING TRASH FEATURE');

    // Test 1: Get trash list
    console.log('Test 1: Getting trash list...');
    this.noteService.getTrashList().subscribe({
      next: (response) => {
        console.log(' Get trash list SUCCESS:', response);
        console.log('🎉 Trash feature is working!');
      },
      error: (error) => {
        console.error(' Get trash list FAILED:', error);
        this.diagnoseError(error, 'Get Trash List');
      }
    });

    console.groupEnd();
  }

  // Debug method to test backend connection
  testBackendConnection() {
    console.group(' TESTING BACKEND CONNECTION');

    console.log('Backend URL:', this.httpService.baseUrl);
    console.log('Testing basic connectivity...');

    // Test basic endpoint
    this.httpService.getApi('/Notes', this.httpService.getHeader()).subscribe({
      next: (response) => {
        console.log(' Backend connection SUCCESS:', response);
        console.log('🎉 Backend is reachable and responding!');
      },
      error: (error) => {
        console.error(' Backend connection FAILED:', error);
        this.diagnoseError(error, 'Backend Connection');
      }
    });

    console.groupEnd();
  }

  // Helper method to diagnose errors
  private diagnoseError(error: any, context: string) {
    console.group(` DIAGNOSING ${context.toUpperCase()} ERROR`);

    console.log('Error details:', {
      status: error.status,
      statusText: error.statusText,
      message: error.message,
      url: error.url,
      error: error.error
    });

    if (error.status === 0) {
      console.error(' DIAGNOSIS: Network/CORS error');
      console.error('   - Check if backend server is running');
      console.error('   - Check CORS configuration');
      console.error('   - Check if URL is correct');
    } else if (error.status === 401) {
      console.error(' DIAGNOSIS: Authentication error');
      console.error('   - Check if user is logged in');
      console.error('   - Check JWT token validity');
      console.error('   - Check Authorization header');
    } else if (error.status === 404) {
      console.error(' DIAGNOSIS: Endpoint not found');
      console.error('   - Check API endpoint URL');
      console.error('   - Check backend controller routing');
    } else if (error.status === 500) {
      console.error(' DIAGNOSIS: Server error');
      console.error('   - Check backend logs');
      console.error('   - Check database connection');
    }

    console.groupEnd();
  }
}
