// User interface for profile information
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  profilePicture?: string;
  createdDate?: string;
  modifiedDate?: string;
  isActive?: boolean;
  phoneNumber?: string;
  dateOfBirth?: string;
}

// User profile for account management
export interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  profilePicture?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  bio?: string;
  preferences?: UserPreferences;
}

// User preferences for account settings
export interface UserPreferences {
  theme: 'light' | 'dark' | 'auto';
  language: string;
  timezone: string;
  emailNotifications: boolean;
  pushNotifications: boolean;
  defaultNoteColor: string;
  defaultView: 'grid' | 'list';
}

// Authentication response from backend
export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

// Login request DTO
export interface LoginRequest {
  email: string;
  password: string;
}

// Register request DTO
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  service?: string;
}

// Update profile request DTO
export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  bio?: string;
  profilePicture?: string;
}

// Change password request DTO
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// Update preferences request DTO
export interface UpdatePreferencesRequest {
  theme?: 'light' | 'dark' | 'auto';
  language?: string;
  timezone?: string;
  emailNotifications?: boolean;
  pushNotifications?: boolean;
  defaultNoteColor?: string;
  defaultView?: 'grid' | 'list';
}

// Account settings interface
export interface AccountSettings {
  profile: UserProfile;
  security: {
    lastPasswordChange: string;
    twoFactorEnabled: boolean;
    loginSessions: LoginSession[];
  };
  privacy: {
    profileVisibility: 'public' | 'private';
    dataSharing: boolean;
    analyticsOptOut: boolean;
  };
}

// Login session interface
export interface LoginSession {
  id: string;
  deviceName: string;
  location: string;
  ipAddress: string;
  lastActive: string;
  isCurrent: boolean;
}

// API response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}
