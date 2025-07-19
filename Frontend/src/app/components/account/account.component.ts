import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AccountService } from '../../services/account.service';
import { LoadingService } from '../../services/loading.service';
import { 
  User, 
  UserProfile, 
  UserPreferences, 
  UpdateProfileRequest, 
  ChangePasswordRequest,
  UpdatePreferencesRequest,
  LoginSession
} from '../../model/user';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    MatListModule
  ],
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.scss']
})
export class AccountComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  userProfile: UserProfile | null = null;
  userPreferences: UserPreferences | null = null;
  loginSessions: LoginSession[] = [];

  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  preferencesForm!: FormGroup;

  isLoading = false;
  isEditingProfile = false;
  selectedTabIndex = 0;

  private subscriptions: Subscription[] = [];

  // Theme options
  themeOptions = [
    { value: 'light', label: 'Light' },
    { value: 'dark', label: 'Dark' },
    { value: 'auto', label: 'Auto' }
  ];

  // Language options
  languageOptions = [
    { value: 'en', label: 'English' },
    { value: 'es', label: 'Spanish' },
    { value: 'fr', label: 'French' },
    { value: 'de', label: 'German' }
  ];

  // View options
  viewOptions = [
    { value: 'grid', label: 'Grid View' },
    { value: 'list', label: 'List View' }
  ];

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private loadingService: LoadingService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadUserData();
    this.subscribeToUserChanges();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private initializeForms(): void {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      dateOfBirth: [''],
      bio: ['', [Validators.maxLength(500)]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.preferencesForm = this.fb.group({
      theme: ['dark'],
      language: ['en'],
      emailNotifications: [true],
      pushNotifications: [true],
      defaultView: ['grid']
    });
  }

  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  private loadUserData(): void {
    this.isLoading = true;

    // Load user profile
    this.accountService.getUserProfile().subscribe({
      next: (profile) => {
        console.log(' Account Component - Received profile data:', profile);
        console.log('📱 Phone Number:', profile.phoneNumber);
        console.log(' Date of Birth:', profile.dateOfBirth);
        console.log(' Bio:', profile.bio);

        this.userProfile = profile;
        this.populateProfileForm(profile);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load user profile:', error);
        this.isLoading = false;
        // Use current user data as fallback
        const currentUser = this.accountService.getCurrentUser();
        if (currentUser) {
          this.populateProfileFormFromUser(currentUser);
        }
      }
    });

    // Load login sessions
    this.accountService.getLoginSessions().subscribe({
      next: (sessions) => {
        this.loginSessions = sessions;
      },
      error: (error) => {
        console.error('Failed to load login sessions:', error);
      }
    });
  }

  private subscribeToUserChanges(): void {
    // Subscribe to current user changes
    const userSub = this.accountService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    // Subscribe to preferences changes
    const prefSub = this.accountService.userPreferences$.subscribe(preferences => {
      this.userPreferences = preferences;
      if (preferences) {
        this.populatePreferencesForm(preferences);
      }
    });

    this.subscriptions.push(userSub, prefSub);
  }

  private populateProfileForm(profile: UserProfile): void {
    console.log('🔧 Populating profile form with data:', profile);

    // Format date for HTML date input (YYYY-MM-DD)
    let formattedDateOfBirth = '';
    if (profile.dateOfBirth) {
      try {
        // Handle both ISO string format and date-only format
        const date = new Date(profile.dateOfBirth);
        if (!isNaN(date.getTime())) {
          // Format as YYYY-MM-DD for HTML date input
          formattedDateOfBirth = date.toISOString().split('T')[0];
        }
        console.log(' Date formatting:', {
          original: profile.dateOfBirth,
          parsed: date,
          formatted: formattedDateOfBirth
        });
      } catch (error) {
        console.error(' Error formatting date:', error);
        formattedDateOfBirth = '';
      }
    }

    const formData = {
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
      phoneNumber: profile.phoneNumber || '',
      dateOfBirth: formattedDateOfBirth,
      bio: profile.bio || ''
    };

    console.log(' Form data being set:', formData);

    this.profileForm.patchValue(formData);

    // Log form values after patching
    console.log(' Form values after patching:', this.profileForm.value);
  }

  private populateProfileFormFromUser(user: User): void {
    this.profileForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      phoneNumber: user.phoneNumber,
      dateOfBirth: '',
      bio: ''
    });
  }

  private populatePreferencesForm(preferences: UserPreferences): void {
    this.preferencesForm.patchValue({
      theme: preferences.theme,
      language: preferences.language,
      emailNotifications: preferences.emailNotifications,
      pushNotifications: preferences.pushNotifications,
      defaultView: preferences.defaultView
    });
  }

  // Profile methods
  toggleEditProfile(): void {
    this.isEditingProfile = !this.isEditingProfile;
    if (!this.isEditingProfile && this.userProfile) {
      this.populateProfileForm(this.userProfile);
    }
  }

  saveProfile(): void {
    if (this.profileForm.valid) {
      this.loadingService.show();
      
      const profileData: UpdateProfileRequest = this.profileForm.value;
      
      this.accountService.updateProfile(profileData).subscribe({
        next: (response) => {
          this.loadingService.hide();
          this.isEditingProfile = false;
          this.snackBar.open('Profile updated successfully!', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
          
          if (response.data) {
            this.userProfile = response.data;
          }
        },
        error: (error) => {
          this.loadingService.hide();
          this.snackBar.open('Failed to update profile. Please try again.', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        }
      });
    }
  }

  // Password methods
  changePassword(): void {
    if (this.passwordForm.valid) {
      this.loadingService.show();
      
      const passwordData: ChangePasswordRequest = this.passwordForm.value;
      
      this.accountService.changePassword(passwordData).subscribe({
        next: () => {
          this.loadingService.hide();
          this.passwordForm.reset();
          this.snackBar.open('Password changed successfully!', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        },
        error: (error) => {
          this.loadingService.hide();
          this.snackBar.open('Failed to change password. Please check your current password.', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        }
      });
    }
  }

  // Preferences methods
  savePreferences(): void {
    if (this.preferencesForm.valid) {
      const preferencesData: UpdatePreferencesRequest = this.preferencesForm.value;
      
      this.accountService.updatePreferences(preferencesData).subscribe({
        next: () => {
          this.snackBar.open('Preferences updated successfully!', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        },
        error: (error) => {
          this.snackBar.open('Failed to update preferences.', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        }
      });
    }
  }

  // Session methods
  revokeSession(sessionId: string): void {
    this.accountService.revokeSession(sessionId).subscribe({
      next: () => {
        this.loginSessions = this.loginSessions.filter(s => s.id !== sessionId);
        this.snackBar.open('Session revoked successfully!', 'Close', {
          duration: 3000,
          verticalPosition: 'top'
        });
      },
      error: (error) => {
        this.snackBar.open('Failed to revoke session.', 'Close', {
          duration: 3000,
          verticalPosition: 'top'
        });
      }
    });
  }

  // Utility methods
  getInitials(user: User | null): string {
    if (!user) return 'U';
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) { // 5MB limit
        this.snackBar.open('File size must be less than 5MB', 'Close', {
          duration: 3000,
          verticalPosition: 'top'
        });
        return;
      }

      this.loadingService.show();
      this.accountService.uploadProfilePicture(file).subscribe({
        next: (response) => {
          this.loadingService.hide();
          this.snackBar.open('Profile picture updated successfully!', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        },
        error: (error) => {
          this.loadingService.hide();
          this.snackBar.open('Failed to upload profile picture.', 'Close', {
            duration: 3000,
            verticalPosition: 'top'
          });
        }
      });
    }
  }

  logout(): void {
    this.accountService.logout();
    this.router.navigate(['/login']);
    this.snackBar.open('Logged out successfully!', 'Close', {
      duration: 2000,
      verticalPosition: 'top'
    });
  }
}
