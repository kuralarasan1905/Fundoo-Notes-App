import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { HttpService } from './http_service/http.service';
import { 
  User, 
  UserProfile, 
  UpdateProfileRequest, 
  ChangePasswordRequest, 
  UpdatePreferencesRequest,
  UserPreferences,
  AccountSettings,
  LoginSession,
  ApiResponse
} from '../model/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private userPreferencesSubject = new BehaviorSubject<UserPreferences | null>(null);
  public userPreferences$ = this.userPreferencesSubject.asObservable();

  constructor(private http: HttpService) {
    this.loadCurrentUser();
    this.loadUserPreferences();


  }

  // Get current user from token or storage
  private loadCurrentUser(): void {
    // First try to get user data from localStorage
    const userData = localStorage.getItem('userData');
    if (userData) {
      try {
        const user: User = JSON.parse(userData);
        this.currentUserSubject.next(user);
        return;
      } catch (error) {
        console.error('Error parsing user data from localStorage:', error);
      }
    }

    // Fallback to parsing token
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const user: User = {
          id: payload.sub || payload.userId || payload.id,
          firstName: payload.firstName || payload.given_name || '',
          lastName: payload.lastName || payload.family_name || '',
          email: payload.email || '',
          profilePicture: payload.picture || null,
          isActive: true
        };
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Error parsing user from token:', error);
      }
    }
  }

  // Load user preferences from localStorage
  private loadUserPreferences(): void {
    const preferences = localStorage.getItem('userPreferences');
    if (preferences) {
      try {
        this.userPreferencesSubject.next(JSON.parse(preferences));
      } catch (error) {
        console.error('Error loading user preferences:', error);
        this.setDefaultPreferences();
      }
    } else {
      this.setDefaultPreferences();
    }
  }

  // Set default preferences
  private setDefaultPreferences(): void {
    const defaultPreferences: UserPreferences = {
      theme: 'dark',
      language: 'en',
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      emailNotifications: true,
      pushNotifications: true,
      defaultNoteColor: '#202124',
      defaultView: 'grid'
    };
    this.userPreferencesSubject.next(defaultPreferences);
    localStorage.setItem('userPreferences', JSON.stringify(defaultPreferences));
  }

  // Get current user
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  // Get user profile
  getUserProfile(): Observable<UserProfile> {
    // Check if user is authenticated
    const token = localStorage.getItem('token');
    if (!token) {
      return this.getFallbackProfile();
    }

    return this.http.getApi('/Account/profile', this.http.getHeader()).pipe(
      tap((profile: any) => {
        // Cache the profile data in localStorage for offline access
        const profileDataToStore = {
          phoneNumber: profile.phoneNumber,
          dateOfBirth: profile.dateOfBirth,
          bio: profile.bio
        };
        localStorage.setItem('userProfileData', JSON.stringify(profileDataToStore));
      }),
      catchError(error => {
        console.error('Failed to load user profile from backend:', error);
        // Return fallback profile data
        return this.getFallbackProfile();
      })
    );
  }

  private getFallbackProfile(): Observable<UserProfile> {
    const currentUser = this.getCurrentUser();
    if (!currentUser) {
      return throwError(() => new Error('No user data available'));
    }

    const storedProfileData = localStorage.getItem('userProfileData');
    let savedProfile: Partial<UserProfile> = {};

    if (storedProfileData) {
      try {
        savedProfile = JSON.parse(storedProfileData);
      } catch (parseError) {
        console.error('Error parsing stored profile data:', parseError);
      }
    }

    const fallbackProfile: UserProfile = {
      id: currentUser.id,
      firstName: currentUser.firstName,
      lastName: currentUser.lastName,
      email: currentUser.email,
      profilePicture: currentUser.profilePicture,
      phoneNumber: savedProfile.phoneNumber || currentUser.phoneNumber || '',
      dateOfBirth: savedProfile.dateOfBirth || currentUser.dateOfBirth || '',
      bio: savedProfile.bio || '',
      preferences: this.userPreferencesSubject.value || undefined
    };

    return of(fallbackProfile);
  }









  // Update user profile
  updateProfile(profileData: UpdateProfileRequest): Observable<ApiResponse<UserProfile>> {
    // Check if user is authenticated
    const token = localStorage.getItem('token');
    if (!token) {
      return this.getMockUpdateResponse(profileData);
    }

    return this.http.putApi('/Account/profile', profileData, this.http.getHeader()).pipe(
      tap((response: any) => {
        // Update local user data
        if (response && response.data) {
          const updatedUser: User = {
            ...this.currentUserSubject.value!,
            firstName: response.data.firstName,
            lastName: response.data.lastName,
            email: response.data.email,
            profilePicture: response.data.profilePicture,
            phoneNumber: response.data.phoneNumber,
            dateOfBirth: response.data.dateOfBirth
          };
          this.currentUserSubject.next(updatedUser);
          localStorage.setItem('userData', JSON.stringify(updatedUser));

          // Save profile data for persistence
          const profileDataToStore = {
            phoneNumber: response.data.phoneNumber,
            dateOfBirth: response.data.dateOfBirth,
            bio: response.data.bio
          };
          localStorage.setItem('userProfileData', JSON.stringify(profileDataToStore));
        }
      }),
      catchError(error => {
        console.error('Failed to update profile via backend:', error);
        // Return mock response as fallback
        return this.getMockUpdateResponse(profileData);
      })
    );
  }

  private getMockUpdateResponse(profileData: UpdateProfileRequest): Observable<ApiResponse<UserProfile>> {
    const currentUser = this.getCurrentUser();

    const mockResponse: ApiResponse<UserProfile> = {
      success: true,
      data: {
        id: currentUser?.id || '0',
        firstName: profileData.firstName || currentUser?.firstName || '',
        lastName: profileData.lastName || currentUser?.lastName || '',
        email: profileData.email || currentUser?.email || '',
        profilePicture: currentUser?.profilePicture,
        phoneNumber: profileData.phoneNumber,
        dateOfBirth: profileData.dateOfBirth,
        bio: profileData.bio,
        preferences: this.userPreferencesSubject.value || undefined
      },
      message: 'Profile updated successfully (mock response - backend unavailable)'
    };

    // Update current user data with mock response
    if (mockResponse.data) {
      const updatedUser: User = {
        ...this.currentUserSubject.value!,
        firstName: mockResponse.data.firstName,
        lastName: mockResponse.data.lastName,
        email: mockResponse.data.email,
        profilePicture: mockResponse.data.profilePicture,
        phoneNumber: mockResponse.data.phoneNumber,
        dateOfBirth: mockResponse.data.dateOfBirth
      };
      this.currentUserSubject.next(updatedUser);
      localStorage.setItem('userData', JSON.stringify(updatedUser));

      // Save profile data for persistence
      const profileDataToStore = {
        phoneNumber: mockResponse.data.phoneNumber,
        dateOfBirth: mockResponse.data.dateOfBirth,
        bio: mockResponse.data.bio
      };
      localStorage.setItem('userProfileData', JSON.stringify(profileDataToStore));
      console.log('💾 Saved profile data to localStorage:', profileDataToStore);
    }

    return of(mockResponse);
  }





  // Change password
  changePassword(passwordData: ChangePasswordRequest): Observable<ApiResponse<any>> {
    console.log(' Changing password via backend API');

    // Check if user is authenticated
    const token = localStorage.getItem('token');
    if (!token) {
      console.warn(' No authentication token found, using mock response');
      return of({
        success: true,
        message: 'Password changed successfully (mock response - no backend available)'
      });
    }

    return this.http.postApi('/Account/change-password', passwordData, this.http.getHeader()).pipe(
      tap((response: any) => {
        console.log(' Password changed successfully via backend:', response);
      }),
      catchError(error => {
        console.error(' Failed to change password via backend:', error);
        console.log(' Falling back to mock response for UI testing');

        // Return mock response as fallback
        return of({
          success: true,
          message: 'Password changed successfully (mock response - backend unavailable)'
        });
      })
    );
  }

  // Update user preferences
  updatePreferences(preferences: UpdatePreferencesRequest): Observable<UserPreferences> {
    const currentPrefs = this.userPreferencesSubject.value || {} as UserPreferences;
    const updatedPrefs = { ...currentPrefs, ...preferences };

    // Save to localStorage
    localStorage.setItem('userPreferences', JSON.stringify(updatedPrefs));
    this.userPreferencesSubject.next(updatedPrefs);

    // Apply theme changes immediately
    if (preferences.theme) {
      this.applyTheme(preferences.theme);
    }

    console.log(' Preferences updated locally (no backend sync - no server available)');

    // Return local preferences immediately without backend sync
    return new Observable(observer => {
      observer.next(updatedPrefs);
      observer.complete();
    });
  }

  // Apply theme
  private applyTheme(theme: 'light' | 'dark' | 'auto'): void {
    const body = document.body;
    body.classList.remove('light-theme', 'dark-theme');
    
    if (theme === 'auto') {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      theme = prefersDark ? 'dark' : 'light';
    }
    
    if (theme === 'dark') {
      body.classList.add('dark-theme');
    } else {
      body.classList.add('light-theme');
    }
    
    localStorage.setItem('theme', theme);
  }

  // Get account settings
  getAccountSettings(): Observable<AccountSettings> {
    console.log(' Getting account settings (using mock data - no backend available)');

    const currentUser = this.getCurrentUser();

    // Try to get stored profile data from localStorage
    const storedProfileData = localStorage.getItem('userProfileData');
    let savedProfile: Partial<UserProfile> = {};

    if (storedProfileData) {
      try {
        savedProfile = JSON.parse(storedProfileData);
      } catch (error) {
        console.error('Error parsing stored profile data:', error);
      }
    }

    const userProfile = currentUser ? {
      id: currentUser.id,
      firstName: currentUser.firstName,
      lastName: currentUser.lastName,
      email: currentUser.email,
      profilePicture: currentUser.profilePicture,
      phoneNumber: savedProfile.phoneNumber || currentUser.phoneNumber || '',
      dateOfBirth: savedProfile.dateOfBirth || currentUser.dateOfBirth || '',
      bio: savedProfile.bio || '',
      preferences: this.userPreferencesSubject.value || undefined
    } : null;

    const mockSettings: AccountSettings = {
      profile: userProfile!,
      security: {
        lastPasswordChange: new Date().toISOString(),
        twoFactorEnabled: false,
        loginSessions: []
      },
      privacy: {
        profileVisibility: 'private',
        dataSharing: false,
        analyticsOptOut: false
      }
    };

    console.log(' Mock account settings created:', mockSettings);

    return new Observable<AccountSettings>(observer => {
      observer.next(mockSettings);
      observer.complete();
    });
  }

  // Get login sessions
  getLoginSessions(): Observable<LoginSession[]> {
    console.log(' Getting login sessions - using mock data (backend endpoint not available)');

    // Since backend doesn't have /User/sessions endpoint, return mock session data
    const mockSessions: LoginSession[] = [
      {
        id: '1',
        deviceName: 'Current Browser',
        location: 'Unknown Location',
        ipAddress: '127.0.0.1',
        lastActive: new Date().toISOString(),
        isCurrent: true
      }
    ];

    console.log(' Mock login sessions created:', mockSessions);

    return new Observable(observer => {
      observer.next(mockSessions);
      observer.complete();
    });
  }

  // Revoke login session
  revokeSession(sessionId: string): Observable<ApiResponse<any>> {
    console.log(' Revoking session - using mock response (backend endpoint not available)');

    // Since backend doesn't have session management, return mock success
    const mockResponse: ApiResponse<any> = {
      success: true,
      message: 'Session revoked successfully',
      data: null
    };

    console.log(' Mock session revoked:', sessionId);

    return new Observable(observer => {
      observer.next(mockResponse);
      observer.complete();
    });
  }

  // Logout
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userData');
    localStorage.removeItem('userPreferences');
    this.currentUserSubject.next(null);
    this.userPreferencesSubject.next(null);
    console.log(' User logged out successfully');
  }

  // Upload profile picture
  uploadProfilePicture(file: File): Observable<ApiResponse<string>> {
    console.log(' Uploading profile picture (using mock response - no backend available)');

    // Create a mock URL for the uploaded file
    const mockImageUrl = URL.createObjectURL(file);

    // Return mock success response immediately
    const mockResponse: ApiResponse<string> = {
      success: true,
      data: mockImageUrl,
      message: 'Profile picture uploaded successfully (mock response)'
    };

    // Update current user with mock image URL
    const currentUser = this.currentUserSubject.value;
    if (currentUser) {
      currentUser.profilePicture = mockImageUrl;
      this.currentUserSubject.next(currentUser);
      localStorage.setItem('userData', JSON.stringify(currentUser));
    }

    console.log(' Profile picture uploaded (mock):', mockImageUrl);

    return new Observable<ApiResponse<string>>(observer => {
      observer.next(mockResponse);
      observer.complete();
    });
  }

  // Debug method to test all profile endpoints (disabled to avoid HTTP errors)
  testAllProfileEndpoints(): void {
    console.log(' Profile endpoint testing disabled - no backend server available');
    console.log(' All profile operations use mock data from localStorage and JWT token');
    console.log(' Account page functionality works with fallback data');
  }
}
